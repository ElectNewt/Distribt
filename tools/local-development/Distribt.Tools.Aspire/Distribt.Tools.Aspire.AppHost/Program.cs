using Projects;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Distribt_Services_Products_Api_Read>("productsread");
builder.AddProject<Distribt_Services_Products_Api_Write>("productswrite");
builder.AddProject<Distribt_Services_Products_Consumer>("productsconsumer");


builder.Build().Run();