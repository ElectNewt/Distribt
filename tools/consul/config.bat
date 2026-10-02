#!/bin/bash
# Infrastructure
docker exec -it consul consul services register -name=RabbitMQ -address=localhost
docker exec -it consul consul services register -name=SecretManager -address=http://localhost -port=8200
docker exec -it consul consul services register -name=MySql -address=localhost -port=3307
docker exec -it consul consul services register -name=MongoDb -address=localhost -port=27017
docker exec -it consul consul services register -name=Graylog --address=localhost -port=12201
docker exec -it consul consul services register -name=OpenTelemetryCollector --address=localhost -port=4317

# Services
docker exec -it consul consul services register -name=EmailsApi -address=http://localhost -port=60120
docker exec -it consul consul services register -name=ProductsApiWrite -address=https://localhost -port=60320
docker exec -it consul consul services register -name=ProductsApiRead -address=https://localhost -port=60321
docker exec -it consul consul services register -name=OrdersApi -address=http://localhost -port=60220
docker exec -it consul consul services register -name=SubscriptionsApi -address=http://localhost -port=60420
