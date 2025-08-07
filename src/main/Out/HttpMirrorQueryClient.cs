using NLog;
using neurUL.Common.Http;
using Polly;
using Splat;
using System;
using System.Threading;
using System.Threading.Tasks;
using ei8.Data.Mirror.Common;

namespace ei8.Data.Mirror.Client.Out
{
    public class HttpMirrorQueryClient : IMirrorQueryClient
    {
        private readonly IRequestProvider requestProvider;

        private static Policy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpMirrorQueryClient.logger.Error(ex, "Error occurred while communicating with ei8 Mirror. " + ex.InnerException?.Message)
            );
        private static readonly string GetMirrorsPathTemplate = "data/mirrors";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpMirrorQueryClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task<ItemData> GetItemById(string outBaseUrl, string id, CancellationToken token = default(CancellationToken)) =>
           await HttpMirrorQueryClient.exponentialRetryPolicy.ExecuteAsync(
               async () => await this.GetItemByIdInternal(outBaseUrl, id, token).ConfigureAwait(false));
        
        private async Task<ItemData> GetItemByIdInternal(string outBaseUrl, string id, CancellationToken token = default)
        {
            return await requestProvider.GetAsync<ItemData>(
                           $"{outBaseUrl}{HttpMirrorQueryClient.GetMirrorsPathTemplate}/{id}",
                           token: token
                           );
        }
    }
}
