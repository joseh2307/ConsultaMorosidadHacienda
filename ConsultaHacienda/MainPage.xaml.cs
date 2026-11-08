using ConsultaHacienda.Services;

namespace ConsultaHacienda;

public partial class MainPage : ContentPage
{
	private CancellationTokenSource? _cts;

	public MainPage()
	{
		InitializeComponent();
	}

	/// <summary>Permite solo letras y números ASCII (sin espacios, tildes ni caracteres especiales).</summary>
	private void OnCedulaTextChanged(object? sender, TextChangedEventArgs e)
	{
		var texto = e.NewTextValue ?? string.Empty;
		var limpio = new string(texto.Where(char.IsAsciiLetterOrDigit).ToArray());
		if (limpio != texto)
			CedulaEntry.Text = limpio;
	}

	private async void OnConsultarClicked(object? sender, EventArgs e)
	{
		var cedula = (CedulaEntry.Text ?? string.Empty).Trim();

		if (string.IsNullOrEmpty(cedula))
		{
			MostrarMensaje("✍️", "Por favor ingrese un número de cédula o identificación.");
			CedulaEntry.Focus();
			return;
		}

		_cts?.Cancel();
		_cts = new CancellationTokenSource();
		var ct = _cts.Token;

		CedulaEntry.Unfocus();
		ConsultarButton.IsEnabled = false;
		ResultadoPanel.IsVisible = false;
		MensajePanel.IsVisible = false;
		Cargando.IsVisible = Cargando.IsRunning = true;

		try
		{
			var resultado = await HaciendaService.ConsultarAsync(cedula, ct);
			if (ct.IsCancellationRequested) return;

			ResultadoPanel.BindingContext = resultado;
			ResultadoPanel.IsVisible = true;
			await ResultadoScroll.ScrollToAsync(0, 0, false);
		}
		catch (OperationCanceledException) when (ct.IsCancellationRequested)
		{
			// El usuario limpió o hizo otra consulta: no mostrar nada.
		}
		catch (ConsultaException ex)
		{
			MostrarMensaje("ℹ️", ex.Message);
		}
		catch (TaskCanceledException)
		{
			MostrarMensaje("⏱️", "La consulta tardó demasiado. Revise su conexión e intente de nuevo.");
		}
		catch (HttpRequestException)
		{
			MostrarMensaje("📶", "No se pudo conectar con Hacienda. Verifique su conexión a Internet.");
		}
		catch (Exception ex)
		{
			MostrarMensaje("❌", $"Ocurrió un error inesperado: {ex.Message}");
		}
		finally
		{
			if (!ct.IsCancellationRequested)
			{
				Cargando.IsVisible = Cargando.IsRunning = false;
				ConsultarButton.IsEnabled = true;
			}
		}
	}

	private void OnLimpiarClicked(object? sender, EventArgs e)
	{
		_cts?.Cancel();
		CedulaEntry.Text = string.Empty;
		ResultadoPanel.BindingContext = null;
		ResultadoPanel.IsVisible = false;
		Cargando.IsVisible = Cargando.IsRunning = false;
		ConsultarButton.IsEnabled = true;
		MostrarMensaje("🔎", "Ingrese una cédula y presione «Consultar» para ver la situación tributaria del contribuyente.");
		CedulaEntry.Focus();
	}

	private void OnSalirClicked(object? sender, EventArgs e)
	{
#if ANDROID
		Platform.CurrentActivity?.FinishAffinity();
		Application.Current?.Quit();
#elif IOS
		// iOS no ofrece un cierre "oficial" de apps; se termina el proceso.
		Environment.Exit(0);
#else
		Application.Current?.Quit();
#endif
	}

	private void MostrarMensaje(string icono, string texto)
	{
		MensajeIcono.Text = icono;
		MensajeTexto.Text = texto;
		MensajePanel.IsVisible = true;
	}
}
