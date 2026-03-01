using UnityEngine;

public class Camara : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 20f;

    [Header("Límites del Mapa")]
    public Vector2 limiteX = new Vector2(0f, 40f);
    public Vector2 limiteZ = new Vector2(-40f, 40f);

    [Header("Zoom")]
    public float velocidadZoom = 500f;
    public float alturaMinima = 5f;
    public float alturaMaxima = 30f;

    [Header("Centrar en Jugador")]
    public Transform jugador;

    void Update()
    {
        MoverCamara();
        HacerZoom();
        CentrarEnJugador();
    }

    void MoverCamara()
    {
        Vector3 direccion = Vector3.zero;

        // Movimiento con flechas
        if (Input.GetKey(KeyCode.UpArrow)) direccion += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow)) direccion += Vector3.back;
        if (Input.GetKey(KeyCode.RightArrow)) direccion += Vector3.right;
        if (Input.GetKey(KeyCode.LeftArrow)) direccion += Vector3.left;

        Vector3 movimientoFinal = direccion.normalized * velocidadMovimiento * Time.deltaTime;
        transform.Translate(movimientoFinal, Space.World);

        // Limitaciones de la cámara para que no se salga
        Vector3 posicionClamp = transform.position;
        posicionClamp.x = Mathf.Clamp(posicionClamp.x, limiteX.x, limiteX.y);
        posicionClamp.z = Mathf.Clamp(posicionClamp.z, limiteZ.x, limiteZ.y);
        transform.position = posicionClamp;
    }

    void HacerZoom()
    {
        // Leer la rueda del ratón
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            // Nos movemos hacia adelante/atrás en la dirección a la que mira la cámara
            Vector3 movimientoZoom = transform.forward * scroll * velocidadZoom * Time.deltaTime;
            Vector3 nuevaPosicion = transform.position + movimientoZoom;

            if (nuevaPosicion.y >= alturaMinima && nuevaPosicion.y <= alturaMaxima)
            {
                transform.position = nuevaPosicion;
            }
        }
    }

    void CentrarEnJugador()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jugador != null)
        {
            Vector3 posicionCentrada = new Vector3(jugador.position.x, transform.position.y, jugador.position.z - (transform.position.y / 2f)); // Un pequeño ajuste para que no se vea solo la coronilla
            transform.position = posicionCentrada;
        }
    }
}