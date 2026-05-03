using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CamaraSeguridad : MonoBehaviour
{
    [Header("Identidad Multiagente")]
    public string nombreAgente = "Camara_Principal";

    [Header("Configuración de Rotación")]
    public float anguloGiro = 45f;
    public float tiempoDeGiro = 2f; 
    public float tiempoDePausa = 2f; 
    private float anguloInicialY;

    [Header("Comportamiento de Seguimiento")]
    public float velocidadSeguimiento = 90f; // Velocidad a la que la cámara persigue
    private bool siguiendoIntruso = false;
    private Transform objetivoJugador;
    private Coroutine rutinaPatrulla;
    private Coroutine rutinaGiro;

    [Header("Sensores Visuales")]
    public Light luzCono; 
    public LayerMask capaObstaculos;

    private float tiempoUltimoAviso = 0f;
    public float tiempoEntreAvisos = 3f; 

    void Start()
    {
        anguloInicialY = transform.eulerAngles.y;
        if (luzCono != null) luzCono.color = Color.green;
        rutinaPatrulla = StartCoroutine(PatrullaVisual());
    }

    void Update()
    {
        if (siguiendoIntruso && objetivoJugador != null){
            // Trazar un vector hacia el jugador ignorando la altura
            Vector3 direccionPlana = (objetivoJugador.position - transform.position);
            direccionPlana.y = 0;

            if (direccionPlana != Vector3.zero){
                Quaternion rotacionHaciaJugador = Quaternion.LookRotation(direccionPlana);
                float anguloY = rotacionHaciaJugador.eulerAngles.y;

                // Limitar el giro a los 45 grados
                float difAngulo = Mathf.DeltaAngle(anguloInicialY, anguloY);
                float anguloClamp = Mathf.Clamp(difAngulo, -anguloGiro, anguloGiro);

                Quaternion rotacionFinal = Quaternion.Euler(transform.eulerAngles.x, anguloInicialY + anguloClamp, transform.eulerAngles.z);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionFinal, velocidadSeguimiento * Time.deltaTime);
            }
        }
    }

    IEnumerator PatrullaVisual()
    {
        while (true){
            rutinaGiro = StartCoroutine(GirarHacia(anguloInicialY + anguloGiro));
            yield return rutinaGiro;
            yield return new WaitForSeconds(tiempoDePausa);

            rutinaGiro = StartCoroutine(GirarHacia(anguloInicialY - anguloGiro));
            yield return rutinaGiro;
            yield return new WaitForSeconds(tiempoDePausa);
        }
    }

    IEnumerator GirarHacia(float anguloDestino){
        // Guardar la rotación desde donde arranca en este instante
        Quaternion rotacionInicial = transform.rotation;
        Quaternion rotacionFinal = Quaternion.Euler(transform.eulerAngles.x, anguloDestino, transform.eulerAngles.z);
        float tiempoPasado = 0f;

        while (tiempoPasado < tiempoDeGiro){
            transform.rotation = Quaternion.Lerp(rotacionInicial, rotacionFinal, tiempoPasado / tiempoDeGiro);
            tiempoPasado += Time.deltaTime;
            yield return null; 
        }

        transform.rotation = rotacionFinal; 
    }

    // Sensores y comunicacion

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")){
            Vector3 direccion = (other.transform.position - transform.position).normalized;
            float distancia = Vector3.Distance(transform.position, other.transform.position);
            bool detectado = false;

            if (luzCono != null){
                float anguloAlJugador = Vector3.Angle(luzCono.transform.forward, direccion);
                if (anguloAlJugador <= luzCono.spotAngle / 2f)
                {
                    if (!Physics.Raycast(transform.position, direccion, distancia, capaObstaculos)){
                        detectado = true;
                    }
                }
            }

            // Toma de decisiones
            if (detectado){
                if (!siguiendoIntruso)
                {
                    // Si ve al jugador, persigue con la vista
                    siguiendoIntruso = true;
                    objetivoJugador = other.transform;
                    if (rutinaPatrulla != null) StopCoroutine(rutinaPatrulla);
                    if (rutinaGiro != null) StopCoroutine(rutinaGiro);
                    if (luzCono != null) luzCono.color = Color.red;
                }

                if (Time.time > tiempoUltimoAviso + tiempoEntreAvisos){
                    DarAlarma(other.transform.position);
                    tiempoUltimoAviso = Time.time;
                }
            }
            else{
                PerderDeVista();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")){
            PerderDeVista();
        }
    }

    void PerderDeVista()
    {
        if (siguiendoIntruso){
            // Resetea todo y vuelve a patrullar
            siguiendoIntruso = false;
            objetivoJugador = null;
            if (luzCono != null) luzCono.color = Color.green;
            
            // Reanuda la rutina visual desde donde se quedó
            rutinaPatrulla = StartCoroutine(PatrullaVisual());
        }
    }

    private void DarAlarma(Vector3 posicion)
    {
        List<string> destinatarios = new List<string>();
        if (GestorRed.instancia != null){
            foreach (var guardia in GestorRed.instancia.todosLosAgentes){
                if (guardia != null) destinatarios.Add(guardia.nombreAgente);
            }

            MensajeACL alarma = new MensajeACL(
                Performativa.Request, 
                nombreAgente,
                destinatarios,
                $"Ladrón en {posicion.x}#{posicion.y}#{posicion.z}"
            );
            alarma.replyWith = "AlarmaCamara_" + Time.time;
            GestorRed.instancia.DistribuirMensaje(alarma);
        }
    }
}