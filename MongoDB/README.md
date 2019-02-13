# MongoDB Docker Compose

This page describes the installation of MongoDB and Mongo Express with the use of `docker compose`

### Prerequisites

* Docker is installed on your machine  https://docs.docker.com/
* You have a docker hub account. If not, go create one: https://hub.docker.com/
* Ensure that you have any previously installed MongoDB services turned off

### Install Steps

1. Open a terminal
2. Navigate to the same directory that this `README` resides in
3. Login to docker hub
    ```
    docker login --username your_username_here
    ```
4. Run this command
   ```
   docker-compose -f mongo-compose.yml up -d
   ```
5. Go to http://localhost:8081 to see mongoexpress
6. You can set up your Robo 3T (if you have it) to connect to `localhost:27017` with username `root` and password `example`
7. If you want to take it down, you can run this command
   ```
    docker-compose -f mongo-compose.yml down
   ```

