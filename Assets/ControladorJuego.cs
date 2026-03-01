using UnityEngine;
using TMPro;

public class ControladorJuego : MonoBehaviour
{
    public static ControladorJuego instancia;
    public GameObject panelGameOver; 

    void Awake()
    {
        if (instancia == null) 
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Mensaje esté oculto al empezar
        if (panelGameOver != null) panelGameOver.SetActive(false);
    }

    public void TerminarJuego()
    {
        Debug.Log("¡GAME OVER!");
        panelGameOver.SetActive(true);
        Time.timeScale = 0f;
    }
}