#!/usr/bin/env bash
docker network create --driver bridge --subnet 172.23.0.0/25 --gateway 172.23.0.1  zookeeper_network
mkdir -p ~/zookeeper/zoo1/data ~/zookeeper/zoo1/datalog ~/zookeeper/zoo2/data ~/zookeeper/zoo2/datalog ~/zookeeper/zoo3/data ~/zookeeper/zoo3/datalog
docker-compose up -d
