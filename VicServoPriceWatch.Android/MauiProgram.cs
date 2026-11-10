using Microsoft.Extensions.Logging;
using VicServoPriceWatch.Android.Services;
using VicServoPriceWatch.Core.Services;

namespace VicServoPriceWatch.Android;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiMaps()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<AndroidLocationService>();
		builder.Services.AddSingleton<ApiKeyService>();
		builder.Services.AddSingleton<PostcodeService>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
