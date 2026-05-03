using UnityEngine;
using TMPro; 
using System.Collections;

public class InteraccionesJugador : MonoBehaviour
{
    [Header("Elementos del Nivel")]
    public GameObject puertaSalida;
    
    [Header("Configuración de Cofres")]
    public int cofresNecesarios = 2;
    private int cofresRecogidos = 0;
    
    [Header("Interfaz (UI)")]
    public TextMeshProUGUI textoPantalla;

    // Variable para recordar qué puerta hay delante
    private PuertaDinamica puertaCercana;

    void Start()
    {
        if (textoPantalla != null){
            textoPantalla.text = "";
        }
    }

    void Update()
    {
        // Pulsar la e para abrir la puerta
        if (puertaCercana != null && Input.GetKeyDown(KeyCode.E)){
            puertaCercana.Interactuar(this.gameObject);
        }
    }

    void OnTriggerEnter(Collider otroObjeto)
    {
        // Detectar si entramos en la zona de una puerta
        if (otroObjeto.CompareTag("Puerta")){
            puertaCercana = otroObjeto.GetComponent<PuertaDinamica>();
        }

        // Cofres
        if (otroObjeto.CompareTag("Cofre")){
            cofresRecogidos++;
            
            if (textoPantalla != null){
                if (cofresRecogidos < cofresNecesarios){
                    // Todavía faltan cofres
                    textoPantalla.text = $"¡Cofre recogido! ({cofresRecogidos}/{cofresNecesarios})";
                }
                else{
                    // Se cogieron ambos cofres
                    textoPantalla.text = "¡SALIDA ABIERTA!";
                    
                    if (puertaSalida != null){
                        puertaSalida.SetActive(false); // Abrimos la salida
                    }
                }
                
                StopAllCoroutines(); // Reseteamos el temporizador del texto por si pilla dos muy rápido
                StartCoroutine(BorrarTextoDespuesDeSegundos(3.5f));
            }

            Destroy(otroObjeto.gameObject);
        }

        if (otroObjeto.CompareTag("Salida")){
            // Solo ganamos si el contador ha llegado al objetivo
            if (cofresRecogidos == cofresNecesarios){
                StopAllCoroutines(); 

                if (textoPantalla != null){
                    textoPantalla.text = "¡HAS GANADO!";
                }

                Time.timeScale = 0f;

                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
        }
    }

    void OnTriggerExit(Collider otroObjeto){
        if (otroObjeto.CompareTag("Puerta")){
            // Solo vaciamos la variable si es la misma puerta de la que nos alejamos
            PuertaDinamica puertaQueDejamos = otroObjeto.GetComponent<PuertaDinamica>();
            if (puertaQueDejamos == puertaCercana){
                puertaCercana = null;
            }
        }
    }

    private IEnumerator BorrarTextoDespuesDeSegundos(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        
        if (textoPantalla != null){
            textoPantalla.text = "";
        }
    }
}