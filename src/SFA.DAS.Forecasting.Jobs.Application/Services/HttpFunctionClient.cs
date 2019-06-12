using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.Forecasting.Domain.Infrastructure;

namespace SFA.DAS.Forecasting.Jobs.Application.Services
{
    public class HttpFunctionClient<T> : IHttpFunctionClient<T>
    {
        public string XFunctionsKey { get; set; }

        public async Task<HttpResponseMessage> PostAsync(string url, T data)
        {
            var mediaType = "application/json";
            var content = new StringContent(JsonConvert.SerializeObject(data));
          
            var client = new HttpClient();
            client.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue(mediaType));

            if (!string.IsNullOrEmpty(XFunctionsKey))
            {
                client.DefaultRequestHeaders.Add("x-functions-key", XFunctionsKey);
            }

            return await client.PostAsync(url, content);
        }

    }
}
