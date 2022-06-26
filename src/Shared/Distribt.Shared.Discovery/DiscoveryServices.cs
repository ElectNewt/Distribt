namespace Distribt.Shared.Discovery;

public class DiscoveryServices
{
    public const string RabbitMQ = "RabbitMQ";
    public const string Secrets = "SecretManager";
    public const string MySql = "MySql";
    public const string MongoDb = "MongoDb";
    public const string Graylog = "Graylog";
    public const string OpenTelemetry = "OpenTelemetryCollector";

    public class Microservices
    {
        public const string Emails = "EmailsApi";
        public const string Orders = "OrdersAPi";
        public const string Subscriptions = "SubscriptionsAPi";

        public class ProductsApi
        {
            public const string ApiRead = "ProductsApiRead";
            public const string ApiWrite = "ProductsApiWrite";
        }
    }
}