using UnityEngine;
public class EstadoPatrullar : IEstadoFantasma
{
    public void Entrar(CerebroGhost cerebro)
    {
        cerebro.ActualizarIcono("", Color.white);
        cerebro.agent.isStopped = false; 
        cerebro.agent.speed = 3.5f; 
        cerebro.IrAlPuntoActual();
    }
    public void Ejecutar(CerebroGhost cerebro)
    {
        if (!cerebro.agent.hasPath && !cerebro.agent.pathPending) cerebro.IrAlPuntoActual();
    }
    public void Salir(CerebroGhost cerebro) {}
}