using UnityEngine;
using System.Collections.Generic;

public class GestorRed : MonoBehaviour
{
    // Singleton para que los vigilantes puedan acceder fácilmente
    public static GestorRed instancia;

    [Header("Agentes Conectados")]
    public List<CerebroGhost> todosLosAgentes = new List<CerebroGhost>();

    void Awake()
    {
        if (instancia == null) instancia = this;
    }

    public void DistribuirMensaje(MensajeACL mensaje)
    {
        foreach (CerebroGhost agente in todosLosAgentes)
        {
            if (mensaje.receiver.Contains(agente.nombreAgente))
            {
                agente.RecibirMensaje(mensaje);
            }
        }
    }
}