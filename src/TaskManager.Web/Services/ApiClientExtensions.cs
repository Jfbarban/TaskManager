namespace TaskManager.Web.Services;

public static class ApiClientExtensions
{
    public static IServiceCollection AddApiClient(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IWebTokenProvider, WebTokenProvider>();
        services.AddTransient<AuthTokenHandler>();

        services.AddHttpClient<IApiClient, ApiClient>((sp, client) =>
        {
            client.BaseAddress = ResolverBaseAddress(sp);
            client.Timeout = TimeSpan.FromSeconds(15);
        })
            .AddHttpMessageHandler<AuthTokenHandler>();

        return services;
    }

    private static Uri ResolverBaseAddress(IServiceProvider sp)
    {
        var configurada = sp.GetRequiredService<IConfiguration>()["Api:BaseUrl"];
        if (!string.IsNullOrWhiteSpace(configurada))
            return new Uri(configurada.TrimEnd('/') + "/");

        var request = sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request
            ?? throw new InvalidOperationException(
                "No hay una petición HTTP activa para determinar la dirección de la API.");

        return new Uri($"{request.Scheme}://{request.Host}/");
    }
}