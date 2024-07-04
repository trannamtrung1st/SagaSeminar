#!/bin/bash
bin/kafka-server-stop.sh &
sleep 20s; bin/zookeeper-server-stop.sh &