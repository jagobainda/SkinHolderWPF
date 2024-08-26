using System.Net.Http;
using System.Text.Json;

namespace SkinHolderWPF.Utils;

public static class SteamRequests
{
    private const int NumeroMaximoIntentos = 5;
    private static readonly HttpClient Client = new();
    private const string EnlaceBase = "https://steamcommunity.com/market/priceoverview/?country={0}&currency={1}&appid={2}&market_hash_name={3}";

    /// <summary>
    /// Ejecuta una serie de peticiones HTTP de forma secuencial con un retraso de 3 segundos entre cada una.
    /// </summary>
    /// <param name="marketHashNames">Lista de nombres de artículos en el mercado de Steam.</param>
    /// <param name="country">El país en el que se desea realizar la petición. Por defecto es "ES".</param>
    /// <param name="currency">La moneda en la que se desea realizar la petición. Por defecto es 3.</param>
    /// <param name="appId">El ID de la aplicación de Steam. Por defecto es 730.</param>
    /// <returns>Un array con las respuestas de la API en objetos <see cref="RespuestaPeticion"/>.</returns>
    public static async Task<RespuestaPeticion[]> EjecutarPeticiones(List<string> marketHashNames, string country = "ES", int currency = 3, int appId = 730)
    {
        List<RespuestaPeticion> respuestas = [];

        foreach (var hashName in marketHashNames)
        {
            var respuesta = await HacerPeticionAsync(country, currency, appId, hashName, true);
            respuestas.Add(respuesta);

            if (marketHashNames.Count > 20) await Task.Delay(3000);

            //if (App.GlobalRegistrosMenu != null) App.GlobalRegistrosMenu.ActualizarProgresoSteam((int)respuesta.Precio);
        }

        return respuestas.ToArray();
    }

    /// <summary>
    /// Realiza una petición HTTP GET a una API de Steam con los parámetros especificados.
    /// </summary>
    /// <param name="country">El país en el que se desea realizar la petición.</param>
    /// <param name="currency">La moneda en la que se desea realizar la petición.</param>
    /// <param name="appId">El ID de la aplicación de Steam.</param>
    /// <param name="marketHashName">El nombre del artículo en el mercado de Steam.</param>
    /// <param name="masDe20">Menos de 20 Items</param>
    /// <returns>La respuesta de la API de Steam en un objeto RespuestaPeticion.</returns>
    private static async Task<RespuestaPeticion> HacerPeticionAsync(string country, int currency, int appId, string marketHashName, bool masDe20)
    {
        var enlace = string.Format(EnlaceBase, country, currency, appId, marketHashName);
        var c = 0;

        while (c <= NumeroMaximoIntentos)
        {
            try
            {
                var response = await Client.GetAsync(enlace);
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadAsStringAsync();
                    return new RespuestaPeticion(ExtraerPrecioDeJson(respuesta), c != 0, marketHashName);
                }
            }
            catch (HttpRequestException ex)
            {
                //logModel.AddLog($"Error al hacer la petición a la api de Steam: {ex.Message}", ELogType.Error, ELogPlace.Api);
            }

            c++;
        }

        return new RespuestaPeticion(-1f, true, marketHashName);
    }

    /// <summary>
    /// Extrae el precio más bajo de un objeto JSON.
    /// </summary>
    /// <param name="input">Cadena de texto que contiene el objeto JSON.</param>
    /// <returns>El precio más bajo como un número de punto flotante. Si la cadena de entrada está vacía, devuelve -1.</returns>
    private static float ExtraerPrecioDeJson(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return -1f;
        }

        try
        {
            using var doc = JsonDocument.Parse(input);

            var priceString = doc.RootElement.GetProperty("lowest_price").GetString()?
                .Replace("-", "0")
                .Replace("€", "");

            return float.TryParse(priceString, out var price) ? price : -1f;
        }
        catch (JsonException ex)
        {
            //logModel.AddLog($"Error al extraer el precio del JSON: {ex.Message}", ELogType.Error, ELogPlace.Api);
            return -1f;
        }
    }
}

public record RespuestaPeticion(float Precio, bool Fallo, string HashName);