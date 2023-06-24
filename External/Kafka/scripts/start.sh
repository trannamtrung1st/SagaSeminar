#!/bin/bash
cp ./custom-config/server.properties ./config/server.properties;
cp ./custom-config/zookeeper.properties ./config/zookeeper.properties;

mkdir -p /zookeeper-data;
echo "$BROKER_ID" > /zookeeper-data/myid;

sed -i -e "s/{BROKER_ID}/$BROKER_ID/g" \
	-e "s/:{LOCAL_PORT}/:$LOCAL_PORT/g" \
	-e "s/:{DOCKER_PORT}/:$DOCKER_PORT/g" \
	./config/server.properties;

bin/zookeeper-server-start.sh ./config/zookeeper.properties &
sleep 30s; bin/kafka-server-start.sh ./config/server.properties;