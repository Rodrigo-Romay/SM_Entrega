using System;

public class Transicion
{
    public Func<bool> Condicion; // Una función que devuelve verdadero o falso
    public IEstadoFantasma SiguienteEstado; // A dónde ir si es verdadero

    public Transicion(Func<bool> condicion, IEstadoFantasma siguienteEstado)
    {
        Condicion = condicion;
        SiguienteEstado = siguienteEstado;
    }
}

