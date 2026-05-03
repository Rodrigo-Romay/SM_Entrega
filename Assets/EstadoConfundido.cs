using UnityEngine;
public class EstadoConfundido : IEstadoFantasma
{
    private float cronometroConfusion;
    public bool haTerminado = false;

    public void Entrar(CerebroGhost cerebro)
    {
        haTerminado = false;
        cerebro.ActualizarIcono("?", Color.yellow);
        cerebro.agent.isStopped = true; 
        cronometroConfusion = 0f;
    }
    public void Ejecutar(CerebroGhost cerebro)
    {
        cronometroConfusion += Time.deltaTime;
        if (cronometroConfusion >= cerebro.tiempoConfusion) haTerminado = true;
    }
    public void Salir(CerebroGhost cerebro) {}
}