using UnityEngine;
using TMPro; 
using System.Collections;

public class InteraccionesJugador : MonoBehaviour
{
    [Header("Elementos del Nivel")]
    public GameObject puertaSalida;
    
    [Header("Interfaz (UI)")]
    public TextMeshProUGUI textoPantalla;

    private bool tieneCofre = false;

    void Start()
    {
        if (textoPantalla != null)
        {
            textoPantalla.text = "";
        }
    }

    void OnTriggerEnter(Collider otroObjeto)
    {
        // Comprobar si se cogió el cofe
        if (otroObjeto.CompareTag("Cofre"))
        {
            tieneCofre = true;
            
            if (textoPantalla != null)
            {
                textoPantalla.text = "¡SALIDA ABIERTA!";
                
                // Temporizador para el texto
                StartCoroutine(BorrarTextoDespuesDeSegundos(3.5f));
            }

            // Destruimos el cofre y abrimos la puerta
            Destroy(otroObjeto.gameObject);
            if (puertaSalida != null)
            {
                puertaSalida.SetActive(false);
            }
        }

        // Comprobar si se llegó a la salida
        if (otroObjeto.CompareTag("Salida"))
        {
            if (tieneCofre)
            {
                StopAllCoroutines(); 

                // Mensaje final
                if (textoPantalla != null)
                {
                    textoPantalla.text = "¡HAS GANADO!";
                }

                Time.timeScale = 0f;

                // Salir del play
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }
    }

    private IEnumerator BorrarTextoDespuesDeSegundos(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        
        if (textoPantalla != null)
        {
            textoPantalla.text = "";
        }
    }
}