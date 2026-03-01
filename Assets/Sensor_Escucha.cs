using UnityEngine;

public class SensorEscucha : MonoBehaviour
{
    [Header("Radios de Escucha")]
    public float radioAndando = 3f;
    public float radioCorriendo = 7f;

    [Header("Configuración")]
    public float velocidadParaConsiderarCorrer = 5f; 
    
    private Vector3 posicionAnteriorJugador;

    public bool DetectarRuido(Transform objetivo)
    {
        if (objetivo == null) return false;

        // Calcular la velocidad real del jugador
        float distanciaMovida = Vector3.Distance(objetivo.position, posicionAnteriorJugador);
        float velocidadActualJugador = distanciaMovida / Time.deltaTime;
        
        posicionAnteriorJugador = objetivo.position;

        // Si el jugador está quieto no hace ruido
        if (velocidadActualJugador < 0.1f)
        {
            return false;
        }

        // Definir que radio usar, si el de correr o andar
        float radioActual = radioAndando;
        
        if (velocidadActualJugador >= velocidadParaConsiderarCorrer)
        {
            radioActual = radioCorriendo;
        }

        float distanciaAlFantasma = Vector3.Distance(transform.position, objetivo.position);

        // Verificar que el fantasma oye al jugador
        if (distanciaAlFantasma <= radioActual)
        {
            Color colorDepuracion = (radioActual == radioCorriendo) ? Color.red : Color.yellow;
            Debug.DrawLine(transform.position, objetivo.position, colorDepuracion);
            
            return true; 
        }

        return false;
    }
}