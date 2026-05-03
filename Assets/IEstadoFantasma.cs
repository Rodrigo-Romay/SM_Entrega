using UnityEngine;

public interface IEstadoFantasma
{
    void Entrar(CerebroGhost cerebro);
    
    void Ejecutar(CerebroGhost cerebro);
    
    void Salir(CerebroGhost cerebro);
}

























    // Qué hace nada más entrar a este comportamiento
    // Qué hace constantemente (como el Update)
    // Qué hace justo antes de cambiar a otro comportamiento
