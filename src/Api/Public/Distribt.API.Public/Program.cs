WebApplication app = DefaultDistribtWebApplication.Create();

app.MapGet("/", () => "Hello World!");

DefaultDistribtWebApplication.Run(app);
