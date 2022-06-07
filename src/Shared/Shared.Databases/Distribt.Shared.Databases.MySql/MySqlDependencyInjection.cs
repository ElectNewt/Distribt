using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Shared.Databases.MySql;

public static class MySqlDependencyInjection
{
    public static IServiceCollection AddMySqlDbContext<T>(this IServiceCollection serviceCollection,
        Func<IServiceProvider, Task<string>> connectionString)
        where T : DbContext
    {
        return serviceCollection.AddDbContext<T>((serviceProvider, builder) =>
        {
            builder.UseMySQL(connectionString.Invoke(serviceProvider).Result);
        });
    }

    public static IServiceCollection AddMysqlHealthCheck(this IServiceCollection serviceCollection,
        Func<IServiceProvider, Task<string>> connectionString)
    {
        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        string mySqlConnectionString = connectionString.Invoke(serviceProvider).Result;
        serviceCollection.AddHealthChecks().AddMySql(mySqlConnectionString);
        return serviceCollection;
    }
}