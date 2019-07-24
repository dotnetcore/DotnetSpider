#!/usr/bin/env bash
docker-compose -f kafka.yml stop
docker-compose -f kafka.yml rm zoo1 zoo2 zoo3 kafka1 kafka2 kafka3
rm -rf /storage/var/lib/zookeeper
rm -rf /storage/var/lib/kafka
docker-compose -f kafka.yml up -d