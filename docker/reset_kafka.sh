#!/usr/bin/env bash
docker-compose -f kafka.yml stop
docker-compose -f kafka.yml rm -y
rm -rf /storage/var/lib/zookeeper
rm -rf /storage/var/lib/kafka
docker-compose -f kafka.yml up -d