using System.Net.Security;
using Xamarin.Android.Net;

namespace PotoDocs;

public class AndroidHttpMessageHandler : IPlatformHttpMessageHandler
{
    public HttpMessageHandler GetHttpMessageHandler() =>
        new AndroidMessageHandler
        {
            ServerCertificateCustomValidationCallback = (httpRequestMessage, certificate, chain, sslPolicyErrors) => certificate?.Issuer == "CN=localhost" || sslPolicyErrors == SslPolicyErrors.None
        };
}
