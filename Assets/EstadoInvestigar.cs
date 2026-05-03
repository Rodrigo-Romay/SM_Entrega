using UnityEngine;
public class EstadoInvestigar : IEstadoFantasma
{
    public void Entrar(CerebroGhost cerebro)
    {
        cerebro.ActualizarIcono("?", Color.yellow);
        cerebro.agent.isStopped = false;
        cerebro.agent.speed = 4f; 
        cerebro.agent.SetDestination(cerebro.ultimaPosicionConocida);
    }
    public void Ejecutar(CerebroGhost cerebro) {}
    public void Salir(CerebroGhost cerebro) {}
}