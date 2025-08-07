using System.Threading;
using System.Threading.Tasks;
using ei8.Data.Mirror.Common;

namespace ei8.Data.Mirror.Client.Out
{
    public interface IMirrorQueryClient
    {
        Task<ItemData> GetItemById(string outBaseUrl, string id, CancellationToken token = default(CancellationToken)); 
    }
}
