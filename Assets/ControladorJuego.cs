using UnityEngine;
using TMPro; // Necesario para usar texto de TextMeshPro

public class ControladorJuego : MonoBehaviour
{
    public static ControladorJuego instancia;
    public GameObject panelGameOver; // El objeto que contiene el texto de Game Over

    // Asegúrate de que en Awake esté esto:
    void Awake()
    {
        if (instancia == null) 
        {
            instancia = this;
            DontDestroyOnLoad(gameObject); // Opcional: para que no se borre entre escenas
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Nos aseguramos de que el mensaje esté oculto al empezar
        if (panelGameOver != null) panelGameOver.SetActive(false);
    }

    public void TerminarJuego()
    {
        Debug.Log("¡GAME OVER!");
        panelGameOver.SetActive(true); // Mostramos el mensaje en grande
        Time.timeScale = 0f; // Detenemos el tiempo de todo el juego
    }
}