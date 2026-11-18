using System;
using System.Collections.Generic;
using System.Text;

namespace ArcaSim.Dominio.Reglas;

public class ValidadorCorrelatividad
{
    public static bool EsSiguienteNumero(long ultimoNumeroAutorizado, long nuevoNumero)
    {
        if (ultimoNumeroAutorizado < 0 || nuevoNumero <= 0)
            return false;

        if (ultimoNumeroAutorizado == long.MaxValue)
            return false;

        return nuevoNumero == ultimoNumeroAutorizado + 1;
    }
}