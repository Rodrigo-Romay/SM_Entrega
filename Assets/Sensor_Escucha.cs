using UnityEngine;

public class SensorEscucha : MonoBehaviour
{
    [Header("Radios de Escucha")]
    public float radioAndando = 3f;
    public float radioCorriendo = 7f;

    [Header("Configuración")]
    // Como tu jugador anda a 3 y corre a 7, el 5 es el punto medio perfecto para diferenciarlos
    public float velocidadParaConsiderarCorrer = 5f; 
    
    private Vector3 posicionAnteriorJugador;

    // El cerebro llamará a esta función
    public bool DetectarRuido(Transform objetivo)
    {
        if (objetivo == null) return false;

        // 1. Calculamos la velocidad real del jugador en este milisegundo
        float distanciaMovida = Vector3.Distance(objetivo.position, posicionAnteriorJugador);
        float velocidadActualJugador = distanciaMovida / Time.deltaTime;
        
        posicionAnteriorJugador = objetivo.position; // Guardamos para el siguiente frame

        // 2. Si el jugador está quieto (o girando sobre sí mismo), no hace ruido
        if (velocidadActualJugador < 0.1f)
        {
            return false;
        }

        // 3. Decidimos qué radio usar: ¿Está corriendo o andando?
        float radioActual = radioAndando; // Por defecto asumimos que anda
        
        if (velocidadActualJugador >= velocidadParaConsiderarCorrer)
        {
            radioActual = radioCorriendo; // Va muy rápido, hace más ruido
        }

        // 4. ¿Está dentro de nuestro rango de audición actual?
        float distanciaAlFantasma = Vector3.Distance(transform.position, objetivo.position);

        if (distanciaAlFantasma <= radioActual)
        {
            // Para que lo veas claro en la pestaña Scene mientras juegas:
            // Línea ROJA si te oye correr, línea AMARILLA si te oye andar
            Color colorDepuracion = (radioActual == radioCorriendo) ? Color.red : Color.yellow;
            Debug.DrawLine(transform.position, objetivo.position, colorDepuracion);
            
            return true; // ¡Te ha escuchado!
        }

        return false; // Está demasiado lejos para el ruido que está haciendo
    }
}