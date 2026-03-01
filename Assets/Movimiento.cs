using UnityEngine;

public class Movimiento : MonoBehaviour
{
    [Header("Velocidades")]
    public float velocidadAndar = 3f;
    public float velocidadCorrer = 7f;
    public float suavizadoRotacion = 700f;

    private Animator animator;
    private Rigidbody rb;  
    
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();  
    }

    void FixedUpdate()  
    {
        // 1. INPUT EXCLUSIVO DE WASD (Ignora las flechas)
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.D)) x = 1f;
        if (Input.GetKey(KeyCode.A)) x = -1f;
        if (Input.GetKey(KeyCode.W)) z = 1f;
        if (Input.GetKey(KeyCode.S)) z = -1f;

        Vector3 direccion = new Vector3(x, 0, z);

        if (direccion.magnitude >= 0.1f)
        {
            bool estaCorriendo = Input.GetKey(KeyCode.LeftShift);
            float velocidadActual = estaCorriendo ? velocidadCorrer : velocidadAndar;

            Vector3 movimiento = direccion.normalized * velocidadActual;
            
            // Usamos el MovePosition que te funciona bien para las puertas
            rb.MovePosition(rb.position + movimiento * Time.fixedDeltaTime);

            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionObjetivo, suavizadoRotacion * Time.fixedDeltaTime);

            float valorAnimacion = estaCorriendo ? 1f : 0.5f;
            animator.SetFloat("Velocidad", valorAnimacion, 0.1f, Time.fixedDeltaTime);
        }
        else
        {
            animator.SetFloat("Velocidad", 0f, 0.1f, Time.fixedDeltaTime);
        }
    }
}