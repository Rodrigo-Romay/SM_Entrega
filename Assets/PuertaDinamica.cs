using UnityEngine;
using System.Collections;

public class PuertaDinamica : MonoBehaviour
{
    [Header("Referencias a las Puertas")]
    public Transform puertaIzquierda;
    public Transform puertaDerecha;

    [Header("Configuración de Ruido")]
    public float radioRuido = 10f;

    [Header("Configuración de Movimiento")]
    public float anguloApertura = 90f; // Grados que giran al abrirse
    public float velocidadRotacion = 5f; // Lo rápido que se abren y cierran

    private bool estaAbierta = false;
    
    // Guardar las rotaciones iniciales (cerradas)
    private Quaternion rotacionCerradaIzq;
    private Quaternion rotacionCerradaDer;
    
    // Guardar las rotaciones objetivo (abiertas)
    private Quaternion rotacionAbiertaIzq;
    private Quaternion rotacionAbiertaDer;

    void Start()
    {
        if (puertaIzquierda != null && puertaDerecha != null){
            // Guardamos las posiciones iniciales como "cerradas"
            rotacionCerradaIzq = puertaIzquierda.rotation;
            rotacionCerradaDer = puertaDerecha.rotation;

            // Calculamos las posiciones abiertas. 
            rotacionAbiertaIzq = Quaternion.Euler(puertaIzquierda.eulerAngles.x, puertaIzquierda.eulerAngles.y - anguloApertura, puertaIzquierda.eulerAngles.z);
            rotacionAbiertaDer = Quaternion.Euler(puertaDerecha.eulerAngles.x, puertaDerecha.eulerAngles.y + anguloApertura, puertaDerecha.eulerAngles.z);
        }
    }

    void Update()
    {
        if (puertaIzquierda == null || puertaDerecha == null) return;
        Quaternion objetivoIzq = estaAbierta ? rotacionAbiertaIzq : rotacionCerradaIzq;
        Quaternion objetivoDer = estaAbierta ? rotacionAbiertaDer : rotacionCerradaDer;
        puertaIzquierda.rotation = Quaternion.Slerp(puertaIzquierda.rotation, objetivoIzq, Time.deltaTime * velocidadRotacion);
        puertaDerecha.rotation = Quaternion.Slerp(puertaDerecha.rotation, objetivoDer, Time.deltaTime * velocidadRotacion);
    }

    public void Interactuar(GameObject quien)
    {
        estaAbierta = !estaAbierta;
        // Solo hace ruido si la abre el jugador
        if (quien != null && quien.CompareTag("Player")){
            GenerarRuidoParaFantasmas();
        }
    }

    private void GenerarRuidoParaFantasmas(){
        Collider[] collidersCercanos = Physics.OverlapSphere(transform.position, radioRuido);

        foreach (Collider col in collidersCercanos){
            SensorEscucha sensor = col.GetComponentInChildren<SensorEscucha>();
            if (sensor != null){
                // Calculamos la distancia exacta entre el centro de la puerta y el fantasma.
                float distanciaReal = Vector3.Distance(transform.position, col.transform.position);
                
                // Si la distancia es menor o igual al radio de ruido, entonces lo escucha.
                if (distanciaReal <= radioRuido){
                    sensor.EscucharRuidoExterno(transform.position);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si un fantasma entra y está cerrada, la abre
        if (other.CompareTag("Fantasma") && !estaAbierta){
            Interactuar(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Si el fantasma sale de la zona de la puerta y está abierta
        if (other.CompareTag("Fantasma") && estaAbierta){
            StartCoroutine(CerrarConRetraso(other.gameObject));
        }
    }

    private IEnumerator CerrarConRetraso(GameObject fantasma)
    {
        yield return new WaitForSeconds(1.5f);
        
        if (estaAbierta){
            Interactuar(fantasma); 
        }
    }
}