#!/bin/bash

## this is the code for the first part of the tutorial. 
## example code for get KeyValue secrets. 
docker exec -it vault vault kv put secret/rabbitmq username=DistribtAdmin password=DistribtPass


## this is the code for the rabbitmq integration with vault
docker exec -it vault vault secrets enable rabbitmq

docker exec -it vault vault write rabbitmq/config/connection \
    connection_uri="http://rabbitmq:15672" \
    username="DistribtAdmin" \
    password="DistribtPass" 

docker exec -it vault vault write rabbitmq/roles/distribt-role \
    vhosts='{"/":{"write": ".*", "read": ".*"}}'


    
## User&Pass for mongoDb
docker exec -it vault vault kv put secret/mongodb username=distribtUser password=distribtPassword

##User&Pass for MySql
docker exec -it vault vault kv put secret/mysql username=distribtUser password=distribtPassword
