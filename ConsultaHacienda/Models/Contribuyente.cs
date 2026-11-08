using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsultaHacienda.Models;

/// <summary>
/// Respuesta de https://api.hacienda.go.cr/fe/ae?identificacion=...
/// Incluye propiedades "de presentación" para mostrar los datos de forma amigable.
/// </summary>
public class Contribuyente
{
	[JsonPropertyName("nombre")]
	public string? Nombre { get; set; }

	[JsonPropertyName("tipoIdentificacion")]
	public string? TipoIdentificacion { get; set; }

	[JsonPropertyName("regimen")]
	public Regimen? Regimen { get; set; }

	[JsonPropertyName("situacion")]
	public Situacion? Situacion { get; set; }

	[JsonPropertyName("actividades")]
	public List<Actividad> Actividades { get; set; } = new();

	/// <summary>Cualquier dato extra que devuelva Hacienda y no esté modelado.</summary>
	[JsonExtensionData]
	public Dictionary<string, JsonElement>? Extra { get; set; }

	[JsonIgnore]
	public string Identificacion { get; set; } = string.Empty;

	[JsonIgnore]
	public string TipoIdentificacionTexto => TipoIdentificacion switch
	{
		"01" => "Persona física",
		"02" => "Persona jurídica",
		"03" => "DIMEX",
		"04" => "NITE",
		null or "" => "No indicado",
		_ => $"Tipo {TipoIdentificacion}"
	};

	[JsonIgnore]
	public string RegimenTexto => string.IsNullOrWhiteSpace(Regimen?.Descripcion) ? "No indicado" : Regimen!.Descripcion!;

	[JsonIgnore]
	public bool TieneActividades => Actividades.Count > 0;

	[JsonIgnore]
	public bool SinActividades => !TieneActividades;

	[JsonIgnore]
	public string TituloActividades => $"Actividades económicas ({Actividades.Count})";

	[JsonIgnore]
	public List<DatoAdicional> DatosAdicionales
	{
		get
		{
			var lista = new List<DatoAdicional>();
			DatoAdicional.Agregar(lista, Extra, "");
			DatoAdicional.Agregar(lista, Regimen?.Extra, "Régimen · ");
			DatoAdicional.Agregar(lista, Situacion?.Extra, "Situación · ");
			return lista;
		}
	}

	[JsonIgnore]
	public bool TieneDatosAdicionales => DatosAdicionales.Count > 0;
}

public class Regimen
{
	[JsonPropertyName("codigo")]
	public int? Codigo { get; set; }

	[JsonPropertyName("descripcion")]
	public string? Descripcion { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? Extra { get; set; }
}

public class Situacion
{
	[JsonPropertyName("moroso")]
	public string? Moroso { get; set; }

	[JsonPropertyName("omiso")]
	public string? Omiso { get; set; }

	[JsonPropertyName("estado")]
	public string? Estado { get; set; }

	[JsonPropertyName("administracionTributaria")]
	public string? AdministracionTributaria { get; set; }

	[JsonPropertyName("mensaje")]
	public string? Mensaje { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? Extra { get; set; }

	[JsonIgnore] public bool EsMoroso => EsSi(Moroso);
	[JsonIgnore] public bool EsOmiso => EsSi(Omiso);
	[JsonIgnore] public string MorosoTexto => EsMoroso ? "SÍ" : (string.IsNullOrWhiteSpace(Moroso) ? "—" : "NO");
	[JsonIgnore] public string OmisoTexto => EsOmiso ? "SÍ" : (string.IsNullOrWhiteSpace(Omiso) ? "—" : "NO");
	[JsonIgnore] public Color MorosoColor => EsMoroso ? Colores.Rojo : Colores.Verde;
	[JsonIgnore] public Color OmisoColor => EsOmiso ? Colores.Rojo : Colores.Verde;

	[JsonIgnore] public string EstadoTexto => string.IsNullOrWhiteSpace(Estado) ? "No indicado" : Estado!;

	/// <summary>Verde si el estado empieza con "Inscrito"; naranja en otro caso (p. ej. "Desinscrito").</summary>
	[JsonIgnore]
	public Color EstadoColor =>
		(Estado ?? string.Empty).Trim().StartsWith("inscrito", StringComparison.OrdinalIgnoreCase)
			? Colores.Verde
			: Colores.Naranja;

	[JsonIgnore] public string AdministracionTexto => string.IsNullOrWhiteSpace(AdministracionTributaria) ? "No indicada" : AdministracionTributaria!;
	[JsonIgnore] public bool TieneMensaje => !string.IsNullOrWhiteSpace(Mensaje);

	private static bool EsSi(string? valor) =>
		string.Equals(valor?.Trim(), "SI", StringComparison.OrdinalIgnoreCase) ||
		string.Equals(valor?.Trim(), "SÍ", StringComparison.OrdinalIgnoreCase);
}

public class Actividad
{
	[JsonPropertyName("estado")]
	public string? Estado { get; set; }

	[JsonPropertyName("tipo")]
	public string? Tipo { get; set; }

	[JsonPropertyName("codigo")]
	public string? Codigo { get; set; }

	[JsonPropertyName("descripcion")]
	public string? Descripcion { get; set; }

	[JsonExtensionData]
	public Dictionary<string, JsonElement>? Extra { get; set; }

	[JsonIgnore]
	public string DescripcionTexto => string.IsNullOrWhiteSpace(Descripcion) ? "Sin descripción" : Descripcion!.Replace("  ", " ");

	[JsonIgnore]
	public string CodigoTexto => $"Código {Codigo ?? "—"}";

	[JsonIgnore]
	public string TipoTexto => Tipo?.ToUpperInvariant() switch
	{
		"P" => "Principal",
		"S" => "Secundaria",
		null or "" => "—",
		_ => Tipo!
	};

	[JsonIgnore]
	public Color TipoColor => Tipo?.ToUpperInvariant() == "P" ? Colores.Azul : Colores.Gris;

	[JsonIgnore]
	public string EstadoTexto => Estado?.ToUpperInvariant() switch
	{
		"A" => "Activa",
		"I" => "Inactiva",
		"C" => "Cancelada",
		"S" => "Suspendida",
		null or "" => "—",
		_ => Estado!
	};

	[JsonIgnore]
	public Color EstadoColor => Estado?.ToUpperInvariant() == "A" ? Colores.Verde : Colores.Naranja;

	[JsonIgnore]
	public string OtrosDatos
	{
		get
		{
			var lista = new List<DatoAdicional>();
			DatoAdicional.Agregar(lista, Extra, "");
			return string.Join("\n", lista.Select(d => $"{d.Etiqueta}: {d.Valor}"));
		}
	}

	[JsonIgnore]
	public bool TieneOtrosDatos => !string.IsNullOrEmpty(OtrosDatos);
}

/// <summary>Par etiqueta/valor para mostrar información no prevista en el modelo.</summary>
public class DatoAdicional
{
	public string Etiqueta { get; set; } = string.Empty;
	public string Valor { get; set; } = string.Empty;

	internal static void Agregar(List<DatoAdicional> lista, Dictionary<string, JsonElement>? datos, string prefijo)
	{
		if (datos is null) return;
		foreach (var (clave, valor) in datos)
			Aplanar(lista, prefijo + Humanizar(clave), valor);
	}

	private static void Aplanar(List<DatoAdicional> lista, string etiqueta, JsonElement valor)
	{
		switch (valor.ValueKind)
		{
			case JsonValueKind.Object:
				foreach (var p in valor.EnumerateObject())
					Aplanar(lista, $"{etiqueta} · {Humanizar(p.Name)}", p.Value);
				break;
			case JsonValueKind.Array:
				var i = 1;
				foreach (var item in valor.EnumerateArray())
					Aplanar(lista, $"{etiqueta} #{i++}", item);
				if (i == 1) lista.Add(new DatoAdicional { Etiqueta = etiqueta, Valor = "Ninguno" });
				break;
			case JsonValueKind.Null:
			case JsonValueKind.Undefined:
				lista.Add(new DatoAdicional { Etiqueta = etiqueta, Valor = "—" });
				break;
			case JsonValueKind.True:
				lista.Add(new DatoAdicional { Etiqueta = etiqueta, Valor = "Sí" });
				break;
			case JsonValueKind.False:
				lista.Add(new DatoAdicional { Etiqueta = etiqueta, Valor = "No" });
				break;
			default:
				lista.Add(new DatoAdicional { Etiqueta = etiqueta, Valor = valor.ToString() });
				break;
		}
	}

	/// <summary>"fechaInscripcion" → "Fecha inscripcion"</summary>
	private static string Humanizar(string clave)
	{
		if (string.IsNullOrEmpty(clave)) return clave;
		var sb = new System.Text.StringBuilder();
		foreach (var c in clave)
		{
			if (char.IsUpper(c) && sb.Length > 0) sb.Append(' ').Append(char.ToLowerInvariant(c));
			else if (c == '_') sb.Append(' ');
			else sb.Append(c);
		}
		var texto = sb.ToString();
		return char.ToUpper(texto[0], CultureInfo.CurrentCulture) + texto[1..];
	}
}

internal static class Colores
{
	public static readonly Color Verde = Color.FromArgb("#1E8E3E");
	public static readonly Color Rojo = Color.FromArgb("#D93025");
	public static readonly Color Naranja = Color.FromArgb("#E37400");
	public static readonly Color Azul = Color.FromArgb("#0B3D91");
	public static readonly Color Gris = Color.FromArgb("#6B7280");
}
