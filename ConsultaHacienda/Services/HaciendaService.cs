using System.Net;
using System.Text.Json;
using ConsultaHacienda.Models;

namespace ConsultaHacienda.Services;

/// <summary>Consulta pública de contribuyentes del Ministerio de Hacienda de Costa Rica.</summary>
public static class HaciendaService
{
	private const string UrlBase = "https://api.hacienda.go.cr/fe/ae?identificacion=";

	private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

	private static readonly JsonSerializerOptions Opciones = new() { PropertyNameCaseInsensitive = true };

	public static async Task<Contribuyente> ConsultarAsync(string identificacion, CancellationToken ct = default)
	{
		using var respuesta = await Http.GetAsync(UrlBase + Uri.EscapeDataString(identificacion), ct);
		var contenido = await respuesta.Content.ReadAsStringAsync(ct);

		switch (respuesta.StatusCode)
		{
			case HttpStatusCode.OK:
				break;
			case HttpStatusCode.NotFound:
				throw new ConsultaException("No se encontró información para esa identificación en Hacienda.");
			case HttpStatusCode.BadRequest:
				throw new ConsultaException("El número de identificación no es válido. Verifique e intente de nuevo.");
			case HttpStatusCode.TooManyRequests:
				throw new ConsultaException("Demasiadas consultas seguidas. Espere un momento e intente de nuevo.");
			default:
				throw new ConsultaException($"El servicio de Hacienda respondió con un error ({(int)respuesta.StatusCode}). Intente más tarde.");
		}

		Contribuyente? resultado;
		try
		{
			resultado = JsonSerializer.Deserialize<Contribuyente>(contenido, Opciones);
		}
		catch (JsonException)
		{
			throw new ConsultaException("La respuesta de Hacienda no tiene el formato esperado.");
		}

		if (resultado is null || string.IsNullOrWhiteSpace(resultado.Nombre))
			throw new ConsultaException("No se encontró información para esa identificación en Hacienda.");

		resultado.Identificacion = identificacion;
		return resultado;
	}
}

public class ConsultaException(string mensaje) : Exception(mensaje);
