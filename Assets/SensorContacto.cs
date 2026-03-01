using UnityEngine;

public class SensorContacto : MonoBehaviour
{
    public string tagJugador = "Player";

    public bool jugadorAtrapado = false; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
            jugadorAtrapado = true; 
        }
    }
}