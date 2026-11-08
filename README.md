# Consulta Hacienda

App móvil (Android y iPhone) para consultar la situación tributaria de un contribuyente en el
Ministerio de Hacienda de Costa Rica, usando la API pública:

```
https://api.hacienda.go.cr/fe/ae?identificacion={cedula}
```

Hecha con **.NET MAUI** (.NET 10), la continuación oficial de Xamarin.Forms. Usa el mismo XAML y C#.

## Funciones

- Campo para la cédula: solo letras y números, sin espacios ni caracteres especiales (se filtran al escribir).
- **Consultar**: llama a la API con la cédula ingresada.
- **Limpiar**: borra la cédula y los resultados.
- **Salir**: cierra la app (pide confirmación).
- El resultado se muestra en tarjetas, no como JSON:
  - Nombre, identificación y tipo (física, jurídica, DIMEX, NITE)
  - Moroso / Omiso con indicadores de color (verde = NO, rojo = SÍ)
  - Estado, administración tributaria y régimen
  - Aviso de Hacienda, cuando lo hay
  - Lista de actividades económicas (descripción, código, Principal/Secundaria, Activa/Inactiva)
  - "Información adicional" con cualquier otro campo que envíe Hacienda
- Todo el resultado está dentro de un scroll que se desliza con el dedo.
- No guarda ningún dato: solo consulta.

## Estructura

```
ConsultaHacienda/
├── MainPage.xaml / .cs        Pantalla principal (búsqueda + resultados)
├── Models/Contribuyente.cs    Modelo de la respuesta y textos/colores para mostrar
├── Services/HaciendaService.cs Llamada HTTP a la API y manejo de errores
├── Platforms/Android          Proyecto Android (permiso INTERNET)
└── Platforms/iOS              Proyecto iOS
```

## Requisitos

- .NET SDK 10
- Workload de MAUI: `dotnet workload install maui-android maui-ios`
  (o Visual Studio 2026 con la carga de trabajo ".NET Multi-platform App UI")
- Para iPhone: una Mac con Xcode (compilación directa o como "Pair to Mac" desde Visual Studio)

## Compilar y ejecutar

Android (emulador o celular conectado por USB):

```bash
dotnet build ConsultaHacienda/ConsultaHacienda.csproj -f net10.0-android -t:Run
```

Generar el APK para instalar en un celular:

```bash
dotnet publish ConsultaHacienda/ConsultaHacienda.csproj -f net10.0-android -c Release -p:AndroidPackageFormat=apk
```

El APK queda en `ConsultaHacienda/bin/Release/net10.0-android/publish/` (usar el `...-Signed.apk`).

Si en el equipo no está instalada la carga de trabajo de iOS (por ejemplo en Windows), agregar
`-p:TargetFrameworks=net10.0-android` a los comandos anteriores para compilar solo Android.

iPhone (en una Mac):

```bash
dotnet build ConsultaHacienda/ConsultaHacienda.csproj -f net10.0-ios -t:Run
```

Para instalar en un iPhone físico o publicar en la App Store hace falta una cuenta de Apple Developer
y un perfil de aprovisionamiento.

## Notas

- En iOS, Apple no ofrece una forma oficial de cerrar una app. El botón **Salir** termina el proceso,
  y Apple puede objetarlo en la revisión de la App Store. Si se publica allí, conviene ocultar ese botón en iOS.
- Respuestas de la API: `404` = identificación sin información, `400` = identificación inválida.
  La app muestra un mensaje claro en ambos casos.
