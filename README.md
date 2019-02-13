# Mass Transit Docker Example

This will allow a user to run Mass Transit with consumers and sagas on a Docker RabbitMQ Cluster with a Docker Mongo DB Backend for persisting the Sagas.

### Pre-Installation Steps

1. Follow the steps here to create a Docker RabbitMQ Cluster https://github.com/mmoser/masstransit-docker-example/tree/master/RabbitMQ%20Cluster
2. Follow the steps here to install/run Docker MongoDB Container https://github.com/mmoser/masstransit-docker-example/tree/master/MongoDB

### Build Steps

1. Open a terminal
2. Navigate to the same directory that this `README` resides in
3. Run the following command to build the image with the tag of `servicebus`
   ```
    docker build -t servicebus .
   ```
### Run Steps

1. Run the following command to start the service bus with the name of `servicebus1`
   ```
   docker run -d --name servicebus1 servicebus
   ```
2. Run the following to see the logs
   ```
   docker logs servicebus1 --follow
   ```

### Teardown Steps

1. In order to stop it and remove the containers run the following
   ```
   docker stop servicebus1
   docker rm servicebus1

   cd MongoDB 
   docker-compose -f mongo-compose.yml down

   docker stop rabbit2
   docker stop rabbit1
   docker rm rabbit2
   docker rm rabbit1
   ``` 