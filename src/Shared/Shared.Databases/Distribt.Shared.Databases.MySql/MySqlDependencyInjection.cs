using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.Databases.MySql;

public static class MySqlDependencyInjection
{
    public static IServiceCollection AddMySqlDbContext<T>(this IServiceCollection serviceCollection, Func<IServiceProvider, Task<string>> connectionString)
    where  T : DbContext
    {
        return serviceCollection.AddDbContext<T>((serviceProvider, builder) =>
        {
            builder.UseMySQL(connectionString.Invoke(serviceProvider).Result);
        } );
    }
}