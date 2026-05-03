using UnityEngine;
using System; 

public class SensorContacto : MonoBehaviour
{
    public string tagJugador = "Player";
    public event Action OnJugadorAtrapado;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
            // Disparamos el evento
            OnJugadorAtrapado?.Invoke(); 
        }
    }
}