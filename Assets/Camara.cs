using UnityEngine;

public class Camara : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 20f;
    public float grosorBordePantalla = 15f; // Píxeles desde el borde para mover con ratón
    public bool usarBordesRaton = true;

    [Header("Límites del Mapa (Para no salirte volando)")]
    public Vector2 limiteX = new Vector2(0f, 40f);
    public Vector2 limiteZ = new Vector2(-40f, 40f);

    [Header("Zoom")]
    public float velocidadZoom = 500f;
    public float alturaMinima = 5f;  // Lo más cerca del suelo
    public float alturaMaxima = 30f; // Lo más alto para ver todo el mapa

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

        // 1. Movimiento con Flechas del teclado
        if (Input.GetKey(KeyCode.UpArrow)) direccion += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow)) direccion += Vector3.back;
        if (Input.GetKey(KeyCode.RightArrow)) direccion += Vector3.right;
        if (Input.GetKey(KeyCode.LeftArrow)) direccion += Vector3.left;

        // Aplicamos el movimiento respecto al mundo (Space.World)
        Vector3 movimientoFinal = direccion.normalized * velocidadMovimiento * Time.deltaTime;
        transform.Translate(movimientoFinal, Space.World);

        // 3. Limitar la cámara para que no se salga del mapa
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

            // Comprobamos que no se pase de los límites de altura
            if (nuevaPosicion.y >= alturaMinima && nuevaPosicion.y <= alturaMaxima)
            {
                transform.position = nuevaPosicion;
            }
        }
    }

    void CentrarEnJugador()
    {
        // Si pulsamos el Espacio y tenemos asignado al jugador, saltamos a su posición
        if (Input.GetKeyDown(KeyCode.Space) && jugador != null)
        {
            // Mantenemos la altura actual y la rotación de la cámara, solo cambiamos X y Z
            Vector3 posicionCentrada = new Vector3(jugador.position.x, transform.position.y, jugador.position.z - (transform.position.y / 2f)); // Un pequeño ajuste para que no se vea solo la coronilla
            transform.position = posicionCentrada;
        }
    }
}