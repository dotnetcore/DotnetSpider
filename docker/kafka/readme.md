### deploy kafka

$ mkdir ~/kafka
$ tee ~/kafka/kafka_server_jaas.conf <<-'EOF'
KafkaServer {
    org.apache.kafka.common.security.plain.PlainLoginModule required
    username="admin"
    password="admin"
    user_admin="admin"
    user_alice="alice";
};
EOF
$ curl https://raw.githubusercontent.com/dotnetcore/DotnetSpider/master/kafka/docker-compose.yml -o ~/kafka/docker-compose.yml   

change KAFKA_ADVERTISED_HOST_NAME to the docker host ip

$ docker-compose -f ~/kafka/docker-compose.yml up -d
