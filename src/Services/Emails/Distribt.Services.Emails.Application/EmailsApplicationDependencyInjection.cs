using Distribt.Services.Emails.Application.DataAccess;
using Distribt.Services.Emails.Application.Email;
using Distribt.Services.Emails.Application.Services;
using Distribt.Shared.Setup.Databases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Distribt.Services.Emails.Application;

public static class EmailsApplicationDependencyInjection
{
    public static void AddEmailsApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDistribtMongoDbConnectionProvider(configuration);
        services.Configure<EmailRepositoryConfiguration>(configuration.GetSection("EmailRepository"));

        services.AddScoped<IEmailRepository, EmailRepository>()
            .AddScoped<IEmailProvider, FakeEmailProvider>()
            .AddScoped<ISendEmailUseCase, SendEmailUseCase>()
            .AddScoped<EmailUseCases>();
    }
}
