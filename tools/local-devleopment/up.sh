#!/bin/bash

docker-compose up -d

sleep 10 # sleep 10 seconds to give time to docker to finish the setup
echo setup vault configuration
./tools/vault/config.sh
echo completed
