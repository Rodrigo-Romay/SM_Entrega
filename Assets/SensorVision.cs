using UnityEngine;
using System;

public class SensorVision : MonoBehaviour
{
    [Header("Configuración de Visión")]
    [Range(0, 180)]
    public float anguloVision = 45f;   
    public LayerMask capaObstaculos;  

    // 1. Los eventos que escuchará el CerebroGhost
    public event Action<Transform> OnJugadorVisto;
    public event Action OnJugadorPerdido;

    private bool viendoActualmente = false;

    // Solo hacemos cálculos si el jugador entra en el SphereCollider
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Transform objetivo = other.transform;
            
            // Como el sensor es hijo del fantasma, usamos el padre para calcular el frente.
            Transform ejecutor = transform.parent != null ? transform.parent : transform;

            Vector3 direccion = (objetivo.position - ejecutor.position).normalized;
            float angulo = Vector3.Angle(ejecutor.forward, direccion);

            bool loVeoEsteFrame = false;

            // Cono de visión
            if (angulo < anguloVision)
            {
                RaycastHit hit;
                float distanciaAlJugador = Vector3.Distance(ejecutor.position, objetivo.position);

                // Raycast
                if (Physics.Raycast(ejecutor.position + Vector3.up, direccion, out hit, distanciaAlJugador, capaObstaculos, QueryTriggerInteraction.Ignore))
                {
                    // Si hitea con algo humano
                    if (hit.transform == objetivo)
                    {
                        Debug.DrawRay(ejecutor.position + Vector3.up, direccion * distanciaAlJugador, Color.green);
                        loVeoEsteFrame = true;
                    }
                    else
                    {
                        Debug.DrawRay(ejecutor.position + Vector3.up, direccion * hit.distance, Color.red);
                    }
                }
                else
                {
                    Debug.DrawRay(ejecutor.position + Vector3.up, direccion * distanciaAlJugador, Color.green);
                    loVeoEsteFrame = true;
                }
            }

            // AQUÍ SE HACE LA GESTIÓN POR EVENTO, SOLO SE DISPARA CUANDO HAY UN CAMBIO EN EL ESTADO
            if (loVeoEsteFrame && !viendoActualmente)
            {
                viendoActualmente = true;
                OnJugadorVisto?.Invoke(objetivo); 
            }
            else if (!loVeoEsteFrame && viendoActualmente)
            {
                PerderVision(); 
            }
        }
    }

    // Jugador sale del radio
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && viendoActualmente)
        {
            PerderVision();
        }
    }

    private void PerderVision()
    {
        viendoActualmente = false;
        OnJugadorPerdido?.Invoke(); // Gritamos que lo perdimos
    }
}