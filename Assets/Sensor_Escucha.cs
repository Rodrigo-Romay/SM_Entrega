using UnityEngine;
using System;

public class SensorEscucha : MonoBehaviour
{
    [Header("Radios de Escucha (Ajustar en el SphereCollider)")]
    public float radioAndando = 3f;
    public float radioCorriendo = 7f;

    [Header("Configuración")]
    public float velocidadParaConsiderarCorrer = 5f; 
    
    // EVENTOS
    public event Action<Vector3> OnRuidoEscuchado;
    public event Action OnRuidoPerdido;

    private Vector3 posicionAnteriorJugador;
    private bool detectandoRuidoActualmente = false;

    // Con OnTriggerStay hacemos que el sensor compruebe si el jugador se está moviendo lo suficiente
    // como para ser oído, siempre que el jugador esté en el radio físico
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")){
            float velocidadActual = CalcularVelocidadJugador(other.transform);
            float distanciaAlFantasma = Vector3.Distance(transform.position, other.transform.position);

            float radioDeRuidoGenerado = (velocidadActual >= velocidadParaConsiderarCorrer) ? radioCorriendo : radioAndando;

            // Si el jugador se mueve y está dentro de su propio radio de ruido
            if (velocidadActual > 0.1f && distanciaAlFantasma <= radioDeRuidoGenerado){
                if (!detectandoRuidoActualmente){
                    detectandoRuidoActualmente = true;
                    OnRuidoEscuchado?.Invoke(other.transform.position); // Notificamos el evento
                }
                
            }
            else if (detectandoRuidoActualmente){
                PerderRuido();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && detectandoRuidoActualmente){
            PerderRuido();
        }
    }

    private void PerderRuido()
    {
        detectandoRuidoActualmente = false;
        OnRuidoPerdido?.Invoke(); 
    }

    private float CalcularVelocidadJugador(Transform objetivo)
    {
        float distanciaMovida = Vector3.Distance(objetivo.position, posicionAnteriorJugador);
        float velocidad = distanciaMovida / Time.deltaTime;
        posicionAnteriorJugador = objetivo.position;
        return velocidad;
    }

    public void EscucharRuidoExterno(Vector3 posicionRuido)
    {
        if (!detectandoRuidoActualmente){
            detectandoRuidoActualmente = true;
            OnRuidoEscuchado?.Invoke(posicionRuido); // Notificamos al cerebro
        }
    }
}