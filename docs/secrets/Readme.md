# Shared.Secrets

Disponemos de un servicio que nos permite tener las credenciales alojadas en una aplicación de terceros en vez de tener dichas credenciales en los ficheros de configuración o incluso inyectarlas en el contenedor/server que vamos autilizar de alguna otra manera.

En nuestro caso, vamos a utilizar Vault de Hashicorp.

# Implementación de Distribt.Shared.Secrets

Deberas inyectar en el contenedor o la aplicacion una variable de entorno, que será la API Token (solo desarrollo, implemnta seguridad fuera de desarrollo) para que así tu app pueda comunicarse con Vault.

Este token debe ser una variable de entonro llamada `VAULT-TOKEN`

Dentro de nuestro contenedor de dependencias debes llamar a `.AddSecretManager(Iconfiguration)` el cual nos inyectará la interfaz `ISecretManager`.

## Recibir secrest desde el gestor de credenciales

La interfaz `ISecretManager` dispone de un único método, llamado `GeT<T>(path)` el cual deveulve el tipo del objeto que hay en el path, por ejemplo:
```csharp

RabbitMQCredentials credentials = await secretManager.Get<RabbitMQCredentials>("rabbitmq/config/connection");
```
nos devuelve un tipo `RabbitMQCredentials` dentro del path `rabbitmq/cofnig/connection` en vault.

La librería no tiene funcionalidad de añadir, únicamente de recibir.

Para insertar secrets en el vault puedes utilizar la UI o el siguiente comando:
```sh
vault write rabbitmq/config/connection \
    connection_uri="http://rabbitmq:15672" \
    username="DistribtAdmin" \
    password="DistribtPass" \
```