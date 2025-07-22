using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ei8.Data.Mirror.Client.In
{
    public interface IMirrorClient
    {
        Task ChangeUrl(string inBaseUrl, string id, string newUrl, int expectedVersion, string authorId, CancellationToken token = default(CancellationToken));
    }
}
