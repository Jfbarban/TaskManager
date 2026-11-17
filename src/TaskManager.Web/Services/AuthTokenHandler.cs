using System.Net.Http.Headers;

namespace TaskManager.Web.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly IWebTokenProvider _tokens;

    public AuthTokenHandler(IWebTokenProvider tokens) => _tokens = tokens;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokens.ObtenerToken());
        return base.SendAsync(request, cancellationToken);
    }
}