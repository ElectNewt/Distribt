using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.Databases.MySql;

public static class MySqlDependencyInjection
{
    public static IServiceCollection AddMySqlDbContext<T>(this IServiceCollection serviceCollection,
        Func<IServiceProvider, Task<string>> connectionString)
        where T : DbContext
    {
        return serviceCollection.AddDbContext<T>(async (serviceProvider, builder) =>
        {
            builder.UseMySQL(await connectionString.Invoke(serviceProvider));
        });
    }

    public static async Task<IServiceCollection> AddMysqlHealthCheck(this IServiceCollection serviceCollection,
        Func<IServiceProvider, Task<string>> connectionString)
    {
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        string mySqlConnectionString = await connectionString.Invoke(serviceProvider);
        serviceCollection.AddHealthChecks().AddMySql(mySqlConnectionString);
        return serviceCollection;
    }
}