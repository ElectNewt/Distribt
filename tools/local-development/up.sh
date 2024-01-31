#!/bin/bash

docker-compose up -d

sleep 15 # sleep 10 seconds to give time to docker to finish the setup
echo setup vault configuration
./tools/vault/config.sh
echo setup consul configuration
./tools/consul/config.sh
echo completed
