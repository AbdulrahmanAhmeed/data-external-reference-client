using NLog;
using neurUL.Common.Http;
using Polly;
using Splat;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Data.Mirror.Client.In
{
    public class HttpMirrorClient : IMirrorClient
    {
        private readonly IRequestProvider requestProvider;

        private static Policy exponentialRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                3,
                attempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt)),
                (ex, _) => HttpMirrorClient.logger.Error(ex, "Error occurred while communicating with ei8 Mirror. " + ex.InnerException?.Message)
            );

        private static readonly string mirrorsPath = "data/mirrors/";
        private static readonly string mirrorsPathTemplate = mirrorsPath + "{0}";
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HttpMirrorClient(IRequestProvider requestProvider = null)
        {
            this.requestProvider = requestProvider ?? Locator.Current.GetService<IRequestProvider>();
        }

        public async Task ChangeUrl(string inBaseUrl, string id, string newUrl, int expectedVersion, string authorId, CancellationToken token = default(CancellationToken)) =>
            await HttpMirrorClient.exponentialRetryPolicy.ExecuteAsync(
                async () => await this.ChangeUrlInternal(inBaseUrl, id, newUrl, expectedVersion, authorId, token).ConfigureAwait(false));

        private async Task ChangeUrlInternal(string inBaseUrl, string id, string newUrl, int expectedVersion, string authorId, CancellationToken token = default(CancellationToken))
        {
            var data = new
            {
                Url = newUrl,
                AuthorId = authorId
            };

            await this.requestProvider.PutAsync<object>(
               $"{inBaseUrl}{string.Format(HttpMirrorClient.mirrorsPathTemplate, id)}",
               data,
               token: token,
               headers: new KeyValuePair<string, string>("ETag", expectedVersion.ToString())
               );
        }
    }
}
