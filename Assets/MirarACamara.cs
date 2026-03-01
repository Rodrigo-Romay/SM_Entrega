using UnityEngine;

public class MirarACamara : MonoBehaviour
{
    void LateUpdate()
    {
        // Obligar al icono a mirar siempre en la misma dirección que la cámara del jugador
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}