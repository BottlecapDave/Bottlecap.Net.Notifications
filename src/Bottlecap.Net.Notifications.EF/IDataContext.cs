using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Bottlecap.Net.Notifications.EF
{
    public interface IDataContext
    {
        DbSet<NotificationData> Notifications { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}