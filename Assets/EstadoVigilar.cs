using UnityEngine;
public class EstadoVigilar : IEstadoFantasma
{
    private float cronometroVigilancia;
    public bool haTerminado = false; 

    public void Entrar(CerebroGhost cerebro)
    {
        haTerminado = false;
        cerebro.agent.isStopped = true; 
        cronometroVigilancia = 0f;
        cerebro.anguloInicialY = cerebro.transform.eulerAngles.y;

        cerebro.ActualizarIcono(cerebro.debeInvestigar ? "?" : "", cerebro.debeInvestigar ? Color.yellow : Color.white);
    }
    public void Ejecutar(CerebroGhost cerebro)
    {
        cronometroVigilancia += Time.deltaTime;
        float rotacionPendulo = Mathf.Sin(cronometroVigilancia * cerebro.velocidadGiro) * cerebro.anguloGiro;
        cerebro.transform.rotation = Quaternion.Euler(0, cerebro.anguloInicialY + rotacionPendulo, 0);

        if (cronometroVigilancia >= cerebro.tiempoVigilancia) haTerminado = true;
    }
    public void Salir(CerebroGhost cerebro)
    {
        if (!cerebro.debeInvestigar) cerebro.indiceActual = (cerebro.indiceActual + 1) % cerebro.puntosDeRuta.Length;
        cerebro.debeInvestigar = false; 
    }
}