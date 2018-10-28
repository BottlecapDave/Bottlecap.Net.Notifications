using Bottlecap.Net.Notifications.EF;
using Microsoft.EntityFrameworkCore;

namespace Bottlecap.Net.Notifications.ConsoleExample.Configuration
{
    /// <summary>
    /// Our data context for our EntityFramework implementation so that our repositories can interact
    /// with our notifications.
    /// </summary>
    public class DatabaseContext : DbContext, IDataContext
    {
        public DatabaseContext(DbContextOptions options)
            : base(options)
        {

        }

        public DbSet<NotificationData> Notifications { get; set; }
    }
}
