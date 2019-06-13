using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.Forecasting.Domain.Infrastructure
{
    public interface IHttpFunctionClient<in T>
    {
        string XFunctionsKey { get; set; }
        Task<HttpResponseMessage> PostAsync(string url, T data);
    }
}
