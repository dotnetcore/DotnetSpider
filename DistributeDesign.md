## Design

```
                                                               +----------------+
                                                        +----->| Console Node 1 |
                                                        |      +----------------+
                                                        |
+--------------+                  +---------+           |      +----------------+
| Scheduler.NET|<------HTTP------>| Broker  |<--SignalR-|----->| Web Node 1     |
+--------------+                  +----^----+           |      +----------------+
                                       |                |
                                     SignalR            |      +----------------+
                                       |                +----->| Web Node 2     |
                               +-------+---------+             +----------------+
                               |                 |
                         +-----v----+     +------v---+
                         | Worker 1 |     | Worker 2 |
                         +----------+     +----------+
```



## Modules

### Node

Node send heartbeat to broker and receive message from broker via websocket.

**NodeType: Console, Web**

```
+--------------------------------------------------------+ 
|                          Node                          |
+------------------------+--------------+----------------+
| + Id                   | GUID         | PRIMARY        |
+------------------------+--------------+----------------+
| + NodeType             | STRING(50)   |                |
+------------------------+--------------+----------------+
| + IPAddress            | STRING(50)   |                |
+------------------------+--------------+----------------+
| + ProcessorCount       | INT          |                |
+------------------------+--------------+----------------+
| + Group                | STRING(50)   |                |
+------------------------+--------------+----------------+
| + OperatingSystem      | STRING(50)   |                |
+------------------------+--------------+----------------+
| + Memory               | INT          |                |
+------------------------+--------------+----------------+
| + IsEnabled            | BOOL         |                |
+------------------------+--------------+----------------+
| + CreationTime         | DATETIME     |                |
+------------------------+--------------+----------------+

+---------------------------------------------------+ 
|                    NodeStatus                     |
+-------------------+--------------+----------------+
| + Id              | INT          | PRIMARY, AI    |
+-------------------+--------------+----------------+
| + NodeId          | GUID         |                |
+-------------------+--------------+----------------+
| + ProcessCount    | INT          |                |
+-------------------+--------------+----------------+
| + Cpu             | INT          |                |
+-------------------+--------------+----------------+
| + FreeMemory      | INT          |                |
+-------------------+--------------+----------------+
| + CreationTime    | DATETIME     |                |
+-------------------+--------------+----------------+
```

### Broker

```
+---------------------------------------------------+ 
|                          Job                      |
+-------------------+--------------+----------------+
| + Id              | GUID         | PRIMARY        |
+-------------------+--------------+----------------+
| + Name            | STRING(50)   |                |
+-------------------+--------------+----------------+
| + JobType         | ENUM         |                |
+-------------------+--------------+----------------+
| + Cron            | STRING(50)   |                |
+-------------------+--------------+----------------+
| + Description     | STRING(500)  |                |
+-------------------+--------------+----------------+
| + IsEnabled       | BOOL         |                |
+-------------------+--------------+----------------+
| + IsDeleted       | BOOL         |                |
+-------------------+--------------+----------------+

JobType：Block | Application

Node is used as distributed downloader for block job, start request builder & processor & pipeline is a worker, 
used as a agent for application job.

---------------------------------------------------------------------------------------------------------------

+---------------------------------------------------+ 
|                      JobProperty                  |
+-------------------+--------------+----------------+
| + Id              | INT          | PRIMARY, AI    |
+-------------------+--------------+----------------+
| + JobId           | GUID         |                |
+-------------------+--------------+----------------+
| + Key             | STRING(50)   |                |
+-------------------+--------------+----------------+
| + Value           | STRING(500)  |                |
+-------------------+--------------+----------------+


Block job: FullClassName, Arguments
Application job: Pacakge, Application, Arguments, NodeCount, NodeGroup, Os

---------------------------------------------------------------------------------------------------------------

+---------------------------------------------------+ 
|                      Worker                       |
+-------------------+--------------+----------------+
| + Id              | INT          | PRIMARY, AI    |
+-------------------+--------------+----------------+
| + FullClassName   | STRING(100)  |                |
+-------------------+--------------+----------------+
| + ConnectionId    | STRING(50)   |                |
+-------------------+--------------+----------------+
| + ConnectTime     | DATETIME     |                |
+-------------------+--------------+----------------+
| + DisconnectTime  | DATETIME     |                |
+-------------------+--------------+----------------+

---------------------------------------------------------------------------------------------------------------

+---------------------------------------------------+ 
|                      Block                        |
+-------------------+--------------+----------------+
| + Id              | GUID         | PRIMARY        |
+-------------------+--------------+----------------+
| + Identity        | GUID         | INDEX_IDENTITY |
+-------------------+--------------+----------------+
| + State           | ENUM         |                |
+-------------------+--------------+----------------+
| + CreationTime    | DATETIME     |                |
+-------------------+--------------+----------------+

Block state: ready (1), using (2), success(4), failed(8)

---------------------------------------------------------------------------------------------------------------

+---------------------------------------------------+ 
|                      Running                      |
+-------------------+--------------+----------------+
| + JobId           | GUID         | PRIMARY        |
+-------------------+--------------+----------------+
| + Identity        | GUID         | INDEX_IDENTITY |
+-------------------+--------------+----------------+
| + Consuming       | INT          |                |
+-------------------+--------------+----------------+
| + Priority        | INT          |                |
+-------------------+--------------+----------------+
| + CreationTime    | DATETIME     |                |
+-------------------+--------------+----------------+

---------------------------------------------------------------------------------------------------------------

+---------------------------------------------------+ 
|                      JobStatus                    |
+-------------------+--------------+----------------+
| + JobId           | GUID         | PRIMARY        |
+-------------------+--------------+----------------+
| + Identity        | GUID         | PRIMARY        |
+-------------------+--------------+----------------+
| + NodeId          | GUID         |                |
+-------------------+--------------+----------------+
| + Status          | ENUM         |                |
+-------------------+--------------+----------------+
| + Left            | INT          |                |
+-------------------+--------------+----------------+
| + Success         | INT          |                |
+-------------------+--------------+----------------+
| + Error           | INT          |                |
+-------------------+--------------+----------------+
| + Total           | INT          |                |
+-------------------+--------------+----------------+
| + UpdateTime      | DATETIME     |                |
+-------------------+--------------+----------------+

```

### Application Job

#### Create job

1. Add to Scheduler.NET
2. Save to database

#### Delete job

1. Set job's IsDeleted true

#### Update job

1. Update cron to Scheduler.NET if need
2. Update to database

#### Trigger job

1. Scheduler.NET trigger a callback, argument is job id
2. Check job exists by job id
3. Check job is enabled
4. Get available agent
5. Call agent to run job via signalr 

### Block job

#### Create & Delete & Update are same with application job

#### Register worker

1. Worker connected to broker via signalr
2. Save worker to database

#### Trigger job

1. Scheduler.NET trigger a callback, argument is job id
2. Check job exists by job id
3. Check job is enabled
4. Check if some worker watch this job
5. Call worker via signalr

### Broker

#### OnConnected

1. Add to node table if not exists, update it if exists

#### Heartbeat: BlockDto Heartbeat(AddNodeStatusDto dto)

```
+----------------------------------+ 
|       AddNodeStatusDto           |
+-------------------+--------------+
| + ProcessCount    | INT          |
+-------------------+--------------+
| + Cpu             | INT          |
+-------------------+--------------+
| + FreeMemory      | INT          |
+-------------------+--------------+

```

1. Add to nodeheartbeat table 

#### Queue

1. Push(string identity, Request[] requests)
2. IEnumable<Request> Poll(string identity)

### Agent 流程设计

#### Agent Configuration

+ maxProcessCount: 5
+ broker: http://localhost:55626/
+ token: 1234
+ heartbeat: 30
+ retryDownload: 3

#### Connect to broker

1. Connect to broker by url  http://localhost:55626/?nodeid={guid}&operationsystem=windows&proccessorcount=4&memory=1600&nodetype=console&group=default&sign={guid}

#### Heartbeat

1. Send heartbeat every 5 seconds and return requests: Heartbeat(HeartbeatDto dto)
2. Download all requests and send responses to broker: SaveBlockResponse(BlockResonse response), every site & identity use a httpclient, and set cookies if in need(force use same cookies or set cookies before request first url), save download bytes to broker.

```
+--------------------------------------+ 
|          BlockDto                    |
+-------------------+------------------+
| + BlockId         | STRING           |
+-------------------+------------------+
| + Identity        | STRING           |
+-------------------+------------------+
| + ThreadNum       | INT              |
+-------------------+------------------+
| + ChangeIpPattern | STRING           |
+-------------------+------------------+
| + Content         | Site             |
+-------------------+------------------+
| + Cookies         | STRING           |
+-------------------+------------------+

```

#### Run application job

1. Broker call agent to run a application job: RunApplication(string package, string application, string arguments)

#### Control application job

+ Exit(string identity)







