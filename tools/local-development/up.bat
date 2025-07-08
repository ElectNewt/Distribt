@echo off

docker-compose up -d

timeout /t 15 >nul
echo setup vault configuration
./tools/vault/config.bat
echo setup consul configuration
./tools/consul/config.bat
echo completed
