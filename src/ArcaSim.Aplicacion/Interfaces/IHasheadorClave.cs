namespace ArcaSim.Aplicacion.Interfaces;

/// <summary>Hash de la Clave Fiscal. Solo se guarda el hash: la clave nunca queda en la base.</summary>
public interface IHasheadorClave
{
    string Hashear(string clave);

    bool Verificar(string clave, string hash);
}
