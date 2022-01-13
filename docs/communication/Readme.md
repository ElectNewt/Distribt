# Shared.Communication

Para permitir **comunicación asíncrona** creamos una abstracción sobre la lógica del patrón `Producers/Consumers`.

Esta lógica debe ser implementada por la abstracción que deseemos utilizar para implementar el **service bus**. En nuestro caso RabbitMQ, pero puede ser cualquier otra como Kafka, ActiveMQ, Mosqito, etc.

## Tipos de mensajes
Es común, cuando creamos sistemas ditribuidos crear lo que se denomina mensajes de integración (`IntegrationMessages`), para los eventos que van fuera de nuestro dominio, y mensajes de dominio (`DomainMessages`) para los que son dentro de nuestro dominio.

Otro ejemplo muy común es cuando separamos las lecturas (reads) de las escrituras(writes) en la base de datos.
1. A través de nuestro caso de uso guardamos la informacion en la `WriteStore`
2. Generamos un evento/mensaje de dominio, el cual es interceptado por un handler que lo insertara en la `ReadStore`

### IntegrationMessages
Son aquellos mensajes que vamos a generar dentro de nuestro dominio pero van a ser escuchados por otros servicios.

Ten en cuenta que el microservicio que ejecuta los mensajes de integración nunca va a escucharlos, sino que va a escuchar los mensajes de integración de otros servivcios.

Y ten en cuenta que X número de microservicios pueden estar escuchando el mismo mensaje de integración.




### DomainMessages
Son aquellos mensajes que vamos a generar para que el dominio los escuche. 

Algunas veces tenemos aplicaciones que cruzan dominios; No es lo recomendable, pero puede pasar.


### Contenido de los mensjaes
Debido a la naturaleza de nuestra aplicación la estructura de los mensajes es la misma, pero en ambientes de producción mas complejos lo más común es que sea diferente.

Los mensajes contienen los siguientes atributos:
- MessageIdentifier: Identificador único del mensaje. No esta relacionado con ningún ID.
- Name: nombre del Evento. Util para aplicaciones externas como por ejemplo los logs, para ser capaces de localizarlo
- Metadata: Contiene la información generada por el mensaje original, como la fecha de creación, en un ambiente con múltples tenants, debería contener el tenant.
- Content: Contiene el mensaje como tal, osea `T`.

## Configuración con RabbitMQ
Para conectarnos a rabbitMQ debemos especifiar el host, usuario y password. 
Ello lo debemos hacer en una seccion de `appsettings` llamada `Bus:RabbitMQ`:

````json

"Bus": {
    "RabbitMQ": {
      "Hostname" : "localhost",
      "Username": "DistribtAdmin",
      "Password" : "DistribtPass",
    }
  }
````

### Publisher
Debe incluir a la sección anterior una subseccion llamada `Publisher`. La cual contendra la información del exange al que vas a enviar dichos eventos.

````json
 "RabbitMQ": {
      ...
      "Publisher": {
        "IntegrationExchange": "name.exange",
        "DomainExchange" : "another.name"
      }
````

Nota: si eliminas una propiedad, esta será null, y el código funcionara igual, pero no publicara, obviamente.


#### Domain
Para publicar mensajes debes incluir `Services.AddServiceBusDomainPublisher(Iconfiguration);` e inyectar la interfaz `IDomainMessagePublisher`
#### Integration
Para publicar mensajes debes incluir `Services.AddServiceBusIntegrationPublisher(Iconfiguration);` e inyectar la interfaz `IIntegrationMessagePublisher`


Finalmente utiilzar `_publisher.Publish(T)` para publicar mensajes.

### Consumer
para consumir mensajes, debes crear un controller y heredar de  `ConsumerController<T>` donde `T` es o `IntegrationMessage`o `DomainMessage`.
Además la configuración es la siguiente:
````json
 "RabbitMQ": {
        ...
      "Consumer": {
        "IntegrationExchanges" : "int.exchange",
        "IntegrationQueue" : "integration-queue"
        "DomainExchanges" : "dom.exchange",
        "DomainQueue" : "domain-queue"
      }
    }

````

debes incluir en el contenedor de dependencias o bien `Services.AddServiceBusIntegrationConsumer(Iconfiguration);` o `Services.AddServiceBusDomainConsumer(Iconfiguration);` así como incluir los handlers.

#### Handler
Para procesar los mensajes, debes crear una clase que herede de `IIntegrationMessageHandler<T>` o `: IDomainMessageHandler<T>` donde `T` es el tipo de la clase que quieres procesar.

por ejemplo un handler con `IIntegrationMessageHandler<SubscriptionDto>` procesara todos los mensajes del tipo `SbuscriptionDto`.
