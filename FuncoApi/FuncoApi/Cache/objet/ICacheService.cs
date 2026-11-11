namespace FuncoApi.Cache.objet;

/// <summary>Abstracción asíncrona para almacenar valores genéricos con caducidad.</summary>
public interface ICacheService
{
    /// <summary>Obtiene el valor asociado a una clave.</summary>
    /// <typeparam name="T">Tipo esperado del valor.</typeparam>
    /// <param name="key">Clave de caché.</param>
    /// <returns>El valor almacenado o el valor predeterminado si no existe.</returns>
    Task<T?> GetAsync<T>(string key);

    /// <summary>Guarda un valor con una duración relativa.</summary>
    /// <typeparam name="T">Tipo del valor almacenado.</typeparam>
    /// <param name="key">Clave de caché.</param>
    /// <param name="value">Valor que se va a guardar.</param>
    /// <param name="expiration">Tiempo hasta que la entrada expire.</param>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration);

    /// <summary>Elimina una entrada de caché.</summary>
    /// <param name="key">Clave de la entrada que se va a eliminar.</param>
    Task RemoveAsync(string key);
}