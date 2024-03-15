`docker-compose` command for starting all the required and optional services locally:

# macOS
```shell
# Set the proper TZ or skip to use the default (Asia/Shanghai)
echo TZ=Europe/Stockholm > .env & docker-compose -f docker-compose.yaml -f socat.yaml up -d
```

# Windows/Linux
```shell
# Set the proper TZ or skip to use the default (Asia/Shanghai)
echo TZ=Europe/Stockholm > .env & docker-compose -f docker-compose.yaml -d
```

`docker-compose` command for starting the `agent` and `portal` containers:
```shell
docker-compose -f portal.yaml -f agent.yaml up -d
```

**NOTE:** To run the above commands in a"attached mode, remove the `-d` flag from the commands.