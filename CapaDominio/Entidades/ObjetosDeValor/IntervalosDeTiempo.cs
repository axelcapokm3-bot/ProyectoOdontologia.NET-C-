namespace CapaDominio.ObjetosDeValor;

public class IntervaloDeTiempo
{

    public DateTime Inicio { get; }
    public DateTime Fin { get; }

    public IntervaloDeTiempo(DateTime inicio, DateTime fin)
    {
        if (fin <= inicio)
        {
            throw new ArgumentException("La fecha de fin debe ser posterior a la de inicio.");
        }

        Inicio = inicio;
        Fin = fin;
    }


    public TimeSpan Duracion => Fin - Inicio;

    public bool SeSolapaCon(IntervaloDeTiempo otro)
    {
        if (this.Fin <= otro.Inicio || otro.Fin <= this.Inicio)
        {
            return false;
        }

        return true;
    }

  
    public IntervaloDeTiempo ObtenerHuecos(IntervaloDeTiempo siguiente)
    {
        if (siguiente != null && siguiente.Inicio > Fin)
        {
            return new IntervaloDeTiempo(Fin, siguiente.Inicio);
        }

        return null;
    }
}