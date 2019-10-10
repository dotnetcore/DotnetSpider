#!/usr/bin/env bash
docker-compose -f kafka.yml stop
docker-compose -f kafka.yml down
rm -rf /storage/data/dotnetspider/zk1
rm -rf /storage/data/dotnetspider/zk2
rm -rf /storage/data/dotnetspider/zk3
rm -rf /storage/data/dotnetspider/kafka1
rm -rf /storage/data/dotnetspider/kafka2
rm -rf /storage/data/dotnetspider/kafka3
docker-compose -f kafka.yml up -d