using UnityEngine;
using UnityEngine.AI;
using TMPro; 
using System;
using System.Collections.Generic;
using System.Collections;

public class CerebroGhost : MonoBehaviour
{
    public Transform humano;
    public Transform[] puntosDeRuta;
    public float distanciaCambioPunto = 0.5f;

    [Header("UI Flotante")]
    public TextMeshPro iconoEstado; 

    [Header("Comportamiento: Vigilancia")]
    public float tiempoVigilancia = 3f; 
    public float anguloGiro = 60f; 
    public float velocidadGiro = 3f; 

    [Header("Comportamiento: Investigacion")]
    public float tiempoConfusion = 2f; 
    public float toleranciaVision = 0.5f; 

    [Header("BDI y Multiagente")]
    public string nombreAgente;
    private Queue<MensajeACL> buzon = new Queue<MensajeACL>();
    private Dictionary<string, List<MensajeACL>> historialConversaciones = new Dictionary<string, List<MensajeACL>>();

    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public SensorVision sensorVis;
    [HideInInspector] public SensorNavegacion sensorNav;
    [HideInInspector] public SensorContacto sensorCont; 
    [HideInInspector] public SensorEscucha sensorEsc; 
    [HideInInspector] public int indiceActual = 0;
    [HideInInspector] public float tiempoUltimaRuta;
    [HideInInspector] public float anguloInicialY = 0f;
    [HideInInspector] public Vector3 ultimaPosicionConocida; 
    [HideInInspector] public bool debeInvestigar = false;

    // VARIABLES ACTUALIZADAS POR EVENTOS
    [HideInInspector] public bool viendoAlJugador;
    [HideInInspector] public bool oyendoAlJugador;

    [Header("Variables Contract Net")]
    [HideInInspector] public bool esSubastador = false;
    private Dictionary<string, float[]> pujasRecibidas = new Dictionary<string, float[]>();
    private static float tiempoUltimaSubastaGlobal = -999f; // Static para que la compartan todos
    public float cooldownSubasta = 5f;
    private Vector3 posLadronActual;

    // LA MÁQUINA DE ESTADOS
    private IEstadoFantasma estadoActual;
    
    public EstadoPatrullar estadoPatrullar = new EstadoPatrullar();
    public EstadoVigilar estadoVigilar = new EstadoVigilar();
    public EstadoPerseguir estadoPerseguir = new EstadoPerseguir();
    public EstadoConfundido estadoConfundido = new EstadoConfundido();
    public EstadoInvestigar estadoInvestigar = new EstadoInvestigar();

    private List<Transicion> transicionesGlobales;
    private Dictionary<IEstadoFantasma, List<Transicion>> mapaTransiciones;

    void Start()
    {
        nombreAgente = gameObject.name; // Inicializar su identidad BDI

        agent = GetComponent<NavMeshAgent>();
        sensorVis = GetComponentInChildren<SensorVision>();
        sensorNav = GetComponentInChildren<SensorNavegacion>();
        sensorEsc = GetComponentInChildren<SensorEscucha>(); 
        
        sensorCont = GetComponentInChildren<SensorContacto>(); 
        if (sensorCont == null) Debug.LogError($"¡OJO! {nombreAgente} no encuentra el script SensorContacto en su cuerpo.");

        SuscribirEventos();
        ConfigurarMapaDeTransiciones();
        
        if (puntosDeRuta.Length > 0) IrAlPuntoActual();
        CambiarEstado(estadoPatrullar);
    }

    // Bloque de eventos

    void SuscribirEventos()
    {
        if (sensorVis != null) { sensorVis.OnJugadorVisto += ReaccionarVision; sensorVis.OnJugadorPerdido += PerderVision; }
        if (sensorEsc != null) { sensorEsc.OnRuidoEscuchado += ReaccionarRuido; sensorEsc.OnRuidoPerdido += PerderRuido; }
        if (sensorCont != null) { sensorCont.OnJugadorAtrapado += EjecutarGameOver; }
    }

    void OnDestroy()
    {
        if (sensorVis != null) { sensorVis.OnJugadorVisto -= ReaccionarVision; sensorVis.OnJugadorPerdido -= PerderVision; }
        if (sensorEsc != null) { sensorEsc.OnRuidoEscuchado -= ReaccionarRuido; sensorEsc.OnRuidoPerdido -= PerderRuido; }
        if (sensorCont != null) { sensorCont.OnJugadorAtrapado -= EjecutarGameOver; }
    }

    // Reacciones a los eventos

    void ReaccionarVision(Transform objetivo)
    {
        viendoAlJugador = true;
        ultimaPosicionConocida = objetivo.position; 
        posLadronActual = objetivo.position;
        EvaluarPosibilidades(estadoActual); 

        // Si ve al ladrón y no hay subasta, asume el rol de Gestor temporalmente
        if (!esSubastador && GestorRed.instancia != null && GestorRed.instancia.todosLosAgentes.Count > 0 && Time.time > tiempoUltimaSubastaGlobal + cooldownSubasta){
            tiempoUltimaSubastaGlobal = Time.time; // Marcamos el inicio de una nueva subasta
            esSubastador = true;
            pujasRecibidas.Clear();
            Debug.Log($"[CNP] {nombreAgente} detecta al ladrón y asume rol de GESTOR.");
            
            // Envía el Call For Proposal en broadcast
            AvisarAlResto(Performativa.Cfp, $"Subasta#{posLadronActual.x}#{posLadronActual.y}#{posLadronActual.z}");
            StartCoroutine(AdjudicarSubasta());
        }
    }

    void PerderVision() { viendoAlJugador = false; }

    void ReaccionarRuido(Vector3 posicion)
    {
        oyendoAlJugador = true;
        ultimaPosicionConocida = posicion;
        EvaluarPosibilidades(estadoActual); 
    }

    void PerderRuido() { oyendoAlJugador = false; }

    void EjecutarGameOver()
    {
        Debug.Log($" El fantasma {nombreAgente} te ha atrapado.");
        
        if (ControladorJuego.instancia != null){
            ControladorJuego.instancia.TerminarJuego(); 
        }
    }


    void Update()
    {
        if (agent == null) return;

        // Leemos los mensajes
        ProcesarBuzon();

        // Evalua transiciones
        EvaluarPosibilidades(estadoActual);

        if (estadoActual != null) estadoActual.Ejecutar(this);
    }

    // === SISTEMA DE COMUNICACIÓN BDI ===

    public void RecibirMensaje(MensajeACL mensaje){
        buzon.Enqueue(mensaje);
        
        if (!string.IsNullOrEmpty(mensaje.conversationId)){
            if (!historialConversaciones.ContainsKey(mensaje.conversationId))
                historialConversaciones[mensaje.conversationId] = new List<MensajeACL>();
            
            historialConversaciones[mensaje.conversationId].Add(mensaje);
        }
    }

    private void ProcesarBuzon()
    {
        while (buzon.Count > 0){
            MensajeACL msg = buzon.Dequeue();
            AnalizarMensaje(msg);
        }
    }

    private void AnalizarMensaje(MensajeACL msg)
    {
        switch (msg.performativa){
            case Performativa.Request:
                // Si la Cámara pide ayuda, se hace una elección de gestor
                if (!esSubastador && estadoActual != estadoPerseguir && msg.sender.Contains("Camara")){
                    string[] partes = msg.content.Split(new string[] { "en " }, StringSplitOptions.None)[1].Split('#');
                    if (partes.Length == 3){
                        posLadronActual = new Vector3(float.Parse(partes[0]), float.Parse(partes[1]), float.Parse(partes[2]));

                        // Evalúa los mejores gestores
                        bool soyElMejorGestor = true;
                        float miDistancia = Vector3.Distance(transform.position, posLadronActual);

                        foreach (CerebroGhost compa in GestorRed.instancia.todosLosAgentes){
                            if (compa != null && compa != this && compa.estadoActual != compa.estadoPerseguir){
                                if (Vector3.Distance(compa.transform.position, posLadronActual) < miDistancia) soyElMejorGestor = false;
                            }
                        }

                        // Si es el más cercano, se convierte en gestor y anuncia la tarea a los demás
                        if (soyElMejorGestor && Time.time > tiempoUltimaSubastaGlobal + cooldownSubasta){
                            tiempoUltimaSubastaGlobal = Time.time; // Marcar el inicio de una nueva subasta
                            esSubastador = true;
                            pujasRecibidas.Clear();
                            Debug.Log($"[CNP] {nombreAgente} asume rol de GESTOR por cercanía a la alarma de {msg.sender}.");
                            AvisarAlResto(Performativa.Cfp, $"Subasta#{posLadronActual.x}#{posLadronActual.y}#{posLadronActual.z}");
                            StartCoroutine(AdjudicarSubasta());
                        }
                    }
                }
                break;

            case Performativa.Cfp: 
                // FASE DE LICITACIÓN
                if (estadoActual == estadoPerseguir){
                    ResponderMensaje(msg, Performativa.Refuse, "Estoy ocupado"); 
                }
                else{
                    string[] p = msg.content.Split('#');
                    posLadronActual = new Vector3(float.Parse(p[1]), float.Parse(p[2]), float.Parse(p[3]));

                    GameObject[] cofres = GameObject.FindGameObjectsWithTag("Cofre");
                    // Ordenar los cofres por nombre para que todos tengan la misma perspectiva
                    System.Array.Sort(cofres, (a, b) => a.name.CompareTo(b.name));
                    
                    GameObject salida = GameObject.FindGameObjectWithTag("Salida");

                    float distCofre1 = cofres.Length > 0 ? Vector3.Distance(transform.position, cofres[0].transform.position) : 9999f;
                    float distCofre2 = cofres.Length > 1 ? Vector3.Distance(transform.position, cofres[1].transform.position) : 9999f;
                    float distSalida = salida != null ? Vector3.Distance(transform.position, salida.transform.position) : 9999f;

                    Debug.Log($"[CNP] {nombreAgente} envía su puja múltiple (Propose).");
                    ResponderMensaje(msg, Performativa.Propose, $"{distCofre1}#{distCofre2}#{distSalida}"); 
                }
                break;

            case Performativa.Propose:
                if (esSubastador){
                    string[] dists = msg.content.Split('#');
                    if (dists.Length == 3){
                        pujasRecibidas[msg.sender] = new float[] { float.Parse(dists[0]), float.Parse(dists[1]), float.Parse(dists[2]) };
                    }
                }
                break;

            case Performativa.AcceptProposal:
                // FASE DE REALIZACIÓN:
                Debug.Log($"[CNP] {nombreAgente} ganó la subasta. Rol adjudicado: {msg.content}");
                AsumirRol(msg.content, posLadronActual);
                break;

            case Performativa.RejectProposal:
                // Si se pierde la subasta, acción por defecto
                Debug.Log($"[CNP] {nombreAgente} perdió la subasta. Rol por defecto: Emboscar");
                AsumirRol("Emboscar", posLadronActual);
                break;
        }
    }

    // --- FASE DE ADJUDICACIÓN (Ejecutada por el Gestor) ---
    private IEnumerator AdjudicarSubasta()
    {
        yield return new WaitForSeconds(0.5f); // Plazo límite

        string ganadorCofre1 = ""; float minCofre1 = 9999f;
        string ganadorCofre2 = ""; float minCofre2 = 9999f;
        string ganadorSalida = ""; float minSalida = 9999f;

        // Asignar Cofre 1 al más cercano
        foreach (var puja in pujasRecibidas){
            if (puja.Value[0] < minCofre1) { minCofre1 = puja.Value[0]; ganadorCofre1 = puja.Key; }
        }
        // Asignar Cofre 2 (excluyendo al que ya protege el Cofre 1)
        foreach (var puja in pujasRecibidas){
            if (puja.Key != ganadorCofre1 && puja.Value[1] < minCofre2) { minCofre2 = puja.Value[1]; ganadorCofre2 = puja.Key; }
        }
        // Asignar Salida (excluyendo a los que ya protegen cofres)
        foreach (var puja in pujasRecibidas){
            if (puja.Key != ganadorCofre1 && puja.Key != ganadorCofre2 && puja.Value[2] < minSalida) { minSalida = puja.Value[2]; ganadorSalida = puja.Key; }
        }

        foreach (var postor in pujasRecibidas.Keys){
            if (postor == ganadorCofre1) ComunicarFalloOAcierto(postor, Performativa.AcceptProposal, "Cofre1");
            else if (postor == ganadorCofre2) ComunicarFalloOAcierto(postor, Performativa.AcceptProposal, "Cofre2");
            else if (postor == ganadorSalida) ComunicarFalloOAcierto(postor, Performativa.AcceptProposal, "Salida");
            else ComunicarFalloOAcierto(postor, Performativa.RejectProposal, "Nada");
        }

        esSubastador = false; 
        AsumirRol("Emboscar", posLadronActual);
    }

    // Funciones auxiliares
    private void ComunicarFalloOAcierto(string destinatario, Performativa perf, string contenido)
    {
        List<string> dest = new List<string> { destinatario };
        MensajeACL msg = new MensajeACL(perf, nombreAgente, dest, contenido);
        if (GestorRed.instancia != null) GestorRed.instancia.DistribuirMensaje(msg);
    }

    private void AvisarAlResto(Performativa perf, string contenido)
    {
        List<string> destinatariosReales = new List<string>();
        if (GestorRed.instancia != null){
            foreach (CerebroGhost compa in GestorRed.instancia.todosLosAgentes){
                if (compa != null && compa.nombreAgente != this.nombreAgente) destinatariosReales.Add(compa.nombreAgente);
            }
            MensajeACL aviso = new MensajeACL(perf, nombreAgente, destinatariosReales, contenido);
            GestorRed.instancia.DistribuirMensaje(aviso);
        }
    }

    private void AsumirRol(string rol, Vector3 posLadron)
    {
        GameObject[] cofres = GameObject.FindGameObjectsWithTag("Cofre");
        System.Array.Sort(cofres, (a, b) => a.name.CompareTo(b.name));

        if (rol == "Cofre1" && cofres.Length > 0){
            ultimaPosicionConocida = cofres[0].transform.position;
        }
        else if (rol == "Cofre2" && cofres.Length > 1){
            ultimaPosicionConocida = cofres[1].transform.position;
        }
        else if (rol == "Salida"){
            GameObject puntoGuardia = GameObject.Find("PuntoGuardiaSalida");
            ultimaPosicionConocida = puntoGuardia.transform.position;
        }
        else
        {
            if (humano != null){
                Vector3 puntoFuturo = humano.position + humano.forward * 6f;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(puntoFuturo, out hit, 4f, NavMesh.AllAreas)){
                    ultimaPosicionConocida = hit.position;
                    Debug.Log($" {nombreAgente} va a interceptar al jugador por delante.");
                }
                else{
                    ultimaPosicionConocida = posLadron; 
                }
            }
            else{
                ultimaPosicionConocida = posLadron;
            }
        }

        debeInvestigar = true;
        CambiarEstado(estadoInvestigar);
    }

    private void ResponderMensaje(MensajeACL mensajeRecibido, Performativa miPerformativa, string miContenido)
    {
        MensajeACL respuesta = new MensajeACL(miPerformativa, nombreAgente, new List<string> { mensajeRecibido.sender }, miContenido);
        respuesta.conversationId = mensajeRecibido.conversationId;
        if (!string.IsNullOrEmpty(mensajeRecibido.replyWith)) respuesta.inReplyTo = mensajeRecibido.replyWith;
        if (GestorRed.instancia != null) GestorRed.instancia.DistribuirMensaje(respuesta);
    }


    // Mapa de transiciones

    void ConfigurarMapaDeTransiciones(){
        mapaTransiciones = new Dictionary<IEstadoFantasma, List<Transicion>>();
        transicionesGlobales = new List<Transicion>();

        transicionesGlobales.Add(new Transicion(() => viendoAlJugador || oyendoAlJugador, estadoPerseguir));

        mapaTransiciones.Add(estadoPatrullar, new List<Transicion>{
            new Transicion(() => sensorNav != null && sensorNav.HaLlegadoAlDestino(distanciaCambioPunto), estadoVigilar)
        });

        mapaTransiciones.Add(estadoVigilar, new List<Transicion>{
            new Transicion(() => estadoVigilar.haTerminado, estadoPatrullar)
        });

        mapaTransiciones.Add(estadoPerseguir, new List<Transicion>{
            new Transicion(() => !viendoAlJugador && !oyendoAlJugador && estadoPerseguir.tiempoSinPercibir >= toleranciaVision, estadoConfundido)
        });

        mapaTransiciones.Add(estadoConfundido, new List<Transicion>{
            new Transicion(() => estadoConfundido.haTerminado, estadoInvestigar)
        });

        mapaTransiciones.Add(estadoInvestigar, new List<Transicion>{
            new Transicion(() => sensorNav != null && sensorNav.HaLlegadoAlDestino(distanciaCambioPunto), estadoVigilar)
        });
    }

    void EvaluarPosibilidades(IEstadoFantasma estado)
    {
        foreach (var transicion in transicionesGlobales){
            if (transicion.Condicion() && estadoActual != transicion.SiguienteEstado){
                CambiarEstado(transicion.SiguienteEstado);
                return; 
            }
        }

        if (mapaTransiciones.ContainsKey(estado)){
            foreach (var transicion in mapaTransiciones[estado]){
                if (transicion.Condicion() && estadoActual != transicion.SiguienteEstado){
                    CambiarEstado(transicion.SiguienteEstado);
                    return; 
                }
            }
        }
    }

    public void CambiarEstado(IEstadoFantasma nuevoEstado)
    {
        if (estadoActual != null) estadoActual.Salir(this);
        estadoActual = nuevoEstado;
        if (estadoActual != null) estadoActual.Entrar(this);
    }

    public void ActualizarIcono(string texto, Color color) { if (iconoEstado != null) { iconoEstado.text = texto; iconoEstado.color = color; } }
    public void IrAlPuntoActual() { if (puntosDeRuta.Length > 0 && puntosDeRuta[indiceActual] != null) agent.SetDestination(puntosDeRuta[indiceActual].position); }
}