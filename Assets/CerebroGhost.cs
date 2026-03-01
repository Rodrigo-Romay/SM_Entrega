using UnityEngine;
using UnityEngine.AI;
using TMPro; 

public class CerebroGhost : MonoBehaviour
{
    public Transform humano;
    public Transform[] puntosDeRuta;
    public float distanciaCambioPunto = 0.5f;

    [Header("UI Flotante")]
    public TextMeshPro iconoEstado; 

    private NavMeshAgent agent;
    
    // Sensores
    private SensorVision sensorVis;
    private SensorNavegacion sensorNav;
    private SensorContacto sensorCont; 
    private SensorEscucha sensorEsc; 

    private int indiceActual = 0;
    private float tiempoUltimaRuta;

    [Header("Comportamiento: Vigilancia")]
    public float tiempoVigilancia = 3f; 
    public float anguloGiro = 60f; 
    public float velocidadGiro = 3f; 

    private bool estaVigilando = false;
    private float cronometroVigilancia = 0f;
    private float anguloInicialY = 0f;

    [Header("Comportamiento: Investigacion")]
    public float tiempoConfusion = 2f; 
    public float toleranciaVision = 0.5f;
    
    private bool debeInvestigar = false;
    private float cronometroConfusion = 0f;
    private float tiempoSinPercibir = 0f;
    private Vector3 ultimaPosicionConocida; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        sensorVis = GetComponentInChildren<SensorVision>();
        sensorNav = GetComponentInChildren<SensorNavegacion>();
        sensorCont = GetComponentInChildren<SensorContacto>(); 
        sensorEsc = GetComponentInChildren<SensorEscucha>(); 

        if (puntosDeRuta.Length > 0) IrAlPuntoActual();
        
        if (iconoEstado != null) iconoEstado.text = "";
    }

    void Update()
    {
        if (agent == null) return;

        // PRIORIDAD 1: SUPERVIVENCIA
        if (sensorCont != null && sensorCont.jugadorAtrapado)
        {
            ControladorJuego.instancia.TerminarJuego();
            sensorCont.jugadorAtrapado = false; 
            return; 
        }

        // PRIORIDAD 2: VISIÓN / ESCUCHA
        bool veAlJugador = sensorVis != null && sensorVis.DetectarObjetivo(humano, transform);
        bool oyeAlJugador = sensorEsc != null && sensorEsc.DetectarRuido(humano);

        if (veAlJugador || oyeAlJugador)
        {
            tiempoSinPercibir = 0f;
            ultimaPosicionConocida = humano.position;
            debeInvestigar = true; 
            cronometroConfusion = 0f; 
            
            Perseguir();
            return;
        }

        if (debeInvestigar)
        {
            tiempoSinPercibir += Time.deltaTime;
            
            if (tiempoSinPercibir < toleranciaVision)
            {
                Perseguir(); 
                return;
            }

            // PRIORIDAD 3: CONFUSIÓN
            if (cronometroConfusion < tiempoConfusion)
            {
                EstarConfundido();
                return;
            }

            // PRIORIDAD 4: INVESTIGAR
            Investigar();
            return;
        }

        // PRIORIDAD 5: PATRULLA Y VIGILANCIA
        if (estaVigilando)
        {
            Vigilar();
        }
        else
        {
            Patrullar();
        }
    }

    // ACCIONES

    void Perseguir()
    {
        if (iconoEstado != null)
        {
            iconoEstado.text = "!";
            iconoEstado.color = Color.red;
        }

        estaVigilando = false; 
        agent.isStopped = false;
        agent.speed = 5.5f; 

        if (Time.time > tiempoUltimaRuta + 0.2f)
        {
            tiempoUltimaRuta = Time.time;
            Vector3 posicionSuelo = new Vector3(humano.position.x, transform.position.y, humano.position.z);
            agent.SetDestination(posicionSuelo);
        }
    }

    void EstarConfundido()
    {
        if (iconoEstado != null)
        {
            iconoEstado.text = "?";
            iconoEstado.color = Color.yellow;
        }

        agent.isStopped = true; 
        cronometroConfusion += Time.deltaTime;
    }

    void Investigar()
    {
        if (iconoEstado != null)
        {
            iconoEstado.text = "?";
            iconoEstado.color = Color.yellow;
        }

        agent.isStopped = false;
        agent.speed = 4f; 
        agent.SetDestination(ultimaPosicionConocida);

        if (sensorNav != null && sensorNav.HaLlegadoAlDestino(distanciaCambioPunto))
        {
            debeInvestigar = false; 
            estaVigilando = true;
            cronometroVigilancia = 0f;
            anguloInicialY = transform.eulerAngles.y;
        }
    }

    void Patrullar()
    {
        if (iconoEstado != null) iconoEstado.text = "";

        if (puntosDeRuta.Length == 0 || sensorNav == null) return;

        agent.isStopped = false; 
        agent.speed = 3.5f; 

        if (sensorNav.HaLlegadoAlDestino(distanciaCambioPunto))
        {
            estaVigilando = true;
            cronometroVigilancia = 0f;
            anguloInicialY = transform.eulerAngles.y; 
        }
        else if (!agent.hasPath && !agent.pathPending)
        {
            IrAlPuntoActual();
        }
    }

    void Vigilar()
    {
        if (iconoEstado != null) 
        {
            iconoEstado.text = debeInvestigar ? "?" : "";
            if (debeInvestigar) iconoEstado.color = Color.yellow;
        }

        agent.isStopped = true; 
        cronometroVigilancia += Time.deltaTime;

        float rotacionPendulo = Mathf.Sin(cronometroVigilancia * velocidadGiro) * anguloGiro;
        transform.rotation = Quaternion.Euler(0, anguloInicialY + rotacionPendulo, 0);

        if (cronometroVigilancia >= tiempoVigilancia)
        {
            estaVigilando = false; 
            
            if (!debeInvestigar) 
            {
                indiceActual = (indiceActual + 1) % puntosDeRuta.Length;
                IrAlPuntoActual();
            }
        }
    }

    void IrAlPuntoActual()
    {
        if (puntosDeRuta.Length > 0 && puntosDeRuta[indiceActual] != null)
        {
            agent.SetDestination(puntosDeRuta[indiceActual].position);
        }
    }
}