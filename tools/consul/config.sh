#!/bin/bash
docker exec -it consul consul services register -name=RabbitMQ -address=localhost
docker exec -it consul consul services register -name=SecretManager -address=http://localhost -port=8200
docker exec -it consul consul services register -name=MySql -address=localhost -port=3307
docker exec -it consul consul services register -name=MongoDb -address=localhost -port=27017
docker exec -it consul consul services register -name=EmailsApi -address=http://localhost -port=50120
docker exec -it consul consul services register -name=ProductsApiWrite -address=https://localhost -port=50320
docker exec -it consul consul services register -name=ProductsApiRead -address=https://localhost -port=50321
docker exec -it consul consul services register -name=OrdersApi -address=http://localhost -port=50220
docker exec -it consul consul services register -name=SubscriptionsApi -address=http://localhost -port=50420