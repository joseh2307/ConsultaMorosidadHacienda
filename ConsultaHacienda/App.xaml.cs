namespace ConsultaHacienda;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		// Diseño pensado en tema claro (se ve igual con el celular en modo oscuro).
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}