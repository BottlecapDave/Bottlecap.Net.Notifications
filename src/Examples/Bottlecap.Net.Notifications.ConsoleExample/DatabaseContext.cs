using Bottlecap.Net.Notifications.EF;
using Microsoft.EntityFrameworkCore;

namespace Bottlecap.Net.Notifications.ConsoleExample
{
    public class DatabaseContext : DbContext, IDataContext
    {
        public DatabaseContext(DbContextOptions options)
            : base(options)
        {

        }

        public Microsoft.EntityFrameworkCore.DbSet<NotificationData> Notifications { get; set; }
    }
}
