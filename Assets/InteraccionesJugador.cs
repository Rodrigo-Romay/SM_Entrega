using UnityEngine;
using TMPro; 
using System.Collections; // ¡Vital para usar temporizadores (Corrutinas)!

public class InteraccionesJugador : MonoBehaviour
{
    [Header("Elementos del Nivel")]
    public GameObject puertaSalida;
    
    [Header("Interfaz (UI)")]
    public TextMeshProUGUI textoPantalla;

    private bool tieneCofre = false;

    void Start()
    {
        // Empezamos con la pantalla limpia
        if (textoPantalla != null)
        {
            textoPantalla.text = "";
        }
    }

    void OnTriggerEnter(Collider otroObjeto)
    {
        // 1. ¿Hemos tocado el cofre?
        if (otroObjeto.CompareTag("Cofre"))
        {
            tieneCofre = true;
            
            // Escribimos en la pantalla
            if (textoPantalla != null)
            {
                textoPantalla.text = "¡SALIDA ABIERTA!";
                
                // Iniciamos el temporizador oculto (3.5 segundos)
                StartCoroutine(BorrarTextoDespuesDeSegundos(3.5f));
            }

            // Destruimos el cofre y abrimos la puerta
            Destroy(otroObjeto.gameObject);
            if (puertaSalida != null)
            {
                puertaSalida.SetActive(false);
            }
        }

        // 2. ¿Hemos tocado la zona de victoria?
        if (otroObjeto.CompareTag("Salida"))
        {
            if (tieneCofre)
            {
                // Paramos cualquier temporizador previo para que no borre el mensaje final
                StopAllCoroutines(); 

                // Escribimos el mensaje final
                if (textoPantalla != null)
                {
                    textoPantalla.text = "¡HAS GANADO!";
                }

                // Congelamos el tiempo
                Time.timeScale = 0f;

                // Salimos del modo Play
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }
    }

    // --- EL TEMPORIZADOR ---
    private IEnumerator BorrarTextoDespuesDeSegundos(float tiempo)
    {
        // Le decimos a Unity: "Pausa esta función y vuelve cuando pase este tiempo"
        yield return new WaitForSeconds(tiempo);
        
        // Cuando vuelve, vacía el texto
        if (textoPantalla != null)
        {
            textoPantalla.text = "";
        }
    }
}