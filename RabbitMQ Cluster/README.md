# RabbitMQ Cluster

This page describes the installation and clustering of a RabbitMQ Cluster in Docker. It is using normal Docker commands to do this. We will build up with Docker Compose in other examples.

If you don't know what RabbitMQ is, go here: https://www.rabbitmq.com/ to read about it. The steps below are using the rabbitmq docker image from the Docker Hub here: https://hub.docker.com/_/rabbitmq and the cluster steps for RabbitMQ found here: https://www.rabbitmq.com/clustering.html

### Prerequisites

* Docker is installed on your machine  https://docs.docker.com/
* You have a docker hub account. If not, go create one: https://hub.docker.com/

### Install Steps

1. Open a terminal
2. Login to docker hub

  ```
  docker login --username your_username_here
  ```

3. We can either create a network or link the containers by name. For this example, let's create a network with the following command

  ```
  docker network create mynetwork
  ```

4. Run the following command to create the first RabbitMQ instance. We are creating a container called rabbit1

  ```
  docker run -d --hostname rabbit1 --name rabbit1 -e RABBITMQ_ERLANG_COOKIE='mysecret' -p 15672:15672 -p 5672:5672 --network mynetwork rabbitmq:3-management
  ```

 5. If you want to see the image running, type the following

  ```
  docker ps
  ```

6. Login to the management UI on http://localhost:15672 with username `guest` and password `guest`. Note that we bound the container above to that port with the `-p 15672:15672` in our arguments

7. Run the following command to create the second RabbitMQ instance.

  ```
  docker run -d --hostname rabbit2 --name rabbit2 -e RABBITMQ_ERLANG_COOKIE='mysecret' --network mynetwork rabbitmq:3-management
  ```

8. Run the command from #5 and you will see both containers running. If you look at the management UI from #6, you will only see rabbit1.

9. Bash shell into the 1st instance

  ```
  docker exec -it rabbit1 bash
  ```

10. Check the cluster status

  ```
  rabbitmqctl cluster_status
  ```

11. Stop the app

  ```
  rabbitmqctl stop_app
  ```

12. Reset the node

  ```
  rabbitmqctl reset
  ```

13. Join the second instance to the cluster

  ```
  rabbitmqctl join_cluster rabbit@rabbit2
  ```

14. Restart the app

  ```
  rabbitmqctl start_app
  ```

15. Check the cluster

  ```
  rabbitmqctl cluster_status
  ```

16. Exit bash
  ```
  exit
  ```

17. Check the http://localhost:15672 and you will now see both nodes clustered

