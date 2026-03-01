using UnityEngine;

public class SensorContacto : MonoBehaviour
{
    public string tagJugador = "Player";

    // Esta es la variable que el Cerebro va a leer
    public bool jugadorAtrapado = false; 

    private void OnTriggerEnter(Collider other)
    {
        // El sensor solo percibe, no actúa
        if (other.CompareTag(tagJugador))
        {
            jugadorAtrapado = true; 
        }
    }
}