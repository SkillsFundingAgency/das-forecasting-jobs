using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Forecasting.Domain.Infrastructure;

namespace SFA.DAS.Forecasting.Jobs.Application.Services;

public class HttpFunctionClient<T> : IHttpFunctionClient<T>
{
    public string XFunctionsKey { get; set; }

    public async Task<HttpResponseMessage> PostAsync(string url, T data)
    {
        const string mediaType = "application/json";

        using var content = new StringContent(JsonConvert.SerializeObject(data));
        using var client = new HttpClient();

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
        httpRequest.Content = content;

        httpRequest.Headers
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue(mediaType));
        
        if (!string.IsNullOrEmpty(XFunctionsKey))
        {
            httpRequest.Headers.Add("x-functions-key", XFunctionsKey);
        }

        return await client.SendAsync(httpRequest);
    }
}