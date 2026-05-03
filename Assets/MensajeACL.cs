using System.Collections.Generic;
using System;

public enum Performativa 
{ 
    Request, Inform, Agree, Refuse, 
    Cfp, Propose, AcceptProposal, RejectProposal 
}

[System.Serializable]
public class MensajeACL
{
    [UnityEngine.Header("Esenciales FIPA")]
    public Performativa performativa; // Tipo de acto
    public string sender;             // Emisor
    public List<string> receiver;     // Receptores
    public string content;            // Contenido

    [UnityEngine.Header("Control de Subastas / Conversación")]
    public string conversationId;     // Agrupa los mensajes de una misma subasta
    public string replyWith;          // Etiqueta del mensaje que se envía
    public string inReplyTo;          // Indica a qué etiqueta estamos respondiendo

    public MensajeACL() 
    {
        receiver = new List<string>();
    }

    public MensajeACL(Performativa perf, string emisor, List<string> receptores, string contenido)
    {
        performativa = perf;
        sender = emisor;
        receiver = receptores;
        content = contenido;
        
        conversationId = Guid.NewGuid().ToString(); 
    }
}