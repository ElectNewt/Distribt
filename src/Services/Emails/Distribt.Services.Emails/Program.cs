using Distribt.Services.Emails.Application;

WebApplication app = DefaultDistribtWebApplication.Create(args, builder =>
{
    builder.Services.AddEmailsApplication(builder.Configuration);
});

DefaultDistribtWebApplication.Run(app);
