## DotnetSpider Full Product Solution

### Spider Node Agent

##### 1. Keep only one instance in a server
##### 2. Auto register node info to redis server

key: DOTNETSPIDER_NODES

field: {Guid}

value: {Node status}


##### 3. Guid generate logic

- In windows, create file c:/program data/dotnetspider/agent.id if it is not exists. Write a new guid to it after creation.
- In non-windows, create file /opt/dotnetspider/agent.id if it is not exists. Write a new guid to it after creation.
 

##### 4. Subscribe DOTNETSPIDER_NODE_{Guid} to accept message from manager center

- EXECUTE: Execute command via process.

##### 5. Publish message consume result to DOTNETSPIDER_MESSAGE_{Guid} 

Every message from manager center has a guid identity, publish the consume result info to manager center.

##### 6. Check node enviroment

- Git
- .NET Core, .NET 4.5 or later

### Portal

Supply a portal to publish, schedule task and check task's status and log.

##### 1. Config git source url( allow multi sources).

- Save new git source url, project name from git url, md5 to database.
- When add new git source url, notice every spider node try to clone codes to /opt/dotnetspider/solutions/
- Git source url can be only deleted, and will every tasks, log, status records will be removed.
- Save entry project path. For example, your bussiness solution contain 2 project, 1 named spiders contains all your spider codes, another named Entry which support to accept command arguments and find which spider to be run by reflection.

##### 2. Publish new schedule task

- Set task name(Required).
- Set task arguments(Required).
- Set how manage nodes to run this task. Get count of nodes from redis.
- Set running plan via schedule calendar(Required).
- After task info saved to database. Send message to every spider node to publish the entry project to /opt/dotnetspider/projects/{task name}(c:/program data/dotnetspider/projects/{task name}) via: dotnet publish {path of entry project}

##### 3. Qutarz.net server to schedule all tasks

- When a task triggered, caculate load balance and then send message to spider node to run this task. Command: dotnet run /opt/dotnetspider/projects/{task name}/{entry project name}.dll {arguments}