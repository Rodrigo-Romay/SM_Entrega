using UnityEngine;
public class EstadoPerseguir : IEstadoFantasma
{
    public float tiempoSinPercibir; // Variable para que el Mapa la lea

    public void Entrar(CerebroGhost cerebro)
    {
        cerebro.ActualizarIcono("!", Color.red);
        cerebro.debeInvestigar = true; 
        cerebro.agent.isStopped = false;
        cerebro.agent.speed = 5.5f; 
        tiempoSinPercibir = 0f;
    }
    public void Ejecutar(CerebroGhost cerebro)
    {
        // Usamos las variables del cerebro
        if (cerebro.viendoAlJugador || cerebro.oyendoAlJugador)
        {
            tiempoSinPercibir = 0f;
            cerebro.ultimaPosicionConocida = cerebro.humano.position;

            if (Time.time > cerebro.tiempoUltimaRuta + 0.2f)
            {
                cerebro.tiempoUltimaRuta = Time.time;
                cerebro.agent.SetDestination(new Vector3(cerebro.humano.position.x, cerebro.transform.position.y, cerebro.humano.position.z));
            }
        }
        else
        {
            tiempoSinPercibir += Time.deltaTime;
            // Si el tiempo se excede, el Mapa de Transiciones nos sacará de aquí
            if (Time.time > cerebro.tiempoUltimaRuta + 0.2f)
            {
                cerebro.tiempoUltimaRuta = Time.time;
                cerebro.agent.SetDestination(cerebro.ultimaPosicionConocida);
            }
        }
    }
    public void Salir(CerebroGhost cerebro) {}
}