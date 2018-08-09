### 角色

**Broker**

+ 分发请求块（Blockoutput）： POST http://broker/node/heartbeat
+ 接收分布式下载器的回传数据 (Blockinput): POST http://broker/node/block
+ 接收 Request 打包成 Block, 插入 Request 队列: POST http://broker/requestqueue/{identity}
+ 通过 Identity 查询已经完成的 Request 以 Block 为单位 : GET http://broker/requestqueue/{identity}

Entities
----------------------------

#### Job

| Column | DataType | Value| Key |
|:---|:---|:---|:---|
|Id| VARCHAR(32)| GUID | Primary |
| JobType | INT | Block (0) \| Application (1) |  |
| Name | VARCHAR(50) | TASK1 |  |
| Cron | VARCHAR(50) | */5 * * * * |  |
| Application | VARCHAR(100) | Console.SampleProcessor \| dotnet |  |
| Arguments | VARCHAR(500) | Crawler.dll -s:TestSpider \| null |  |
| Description | VARCHAR(500) | This is a test task. |  |
| IsEnabled | BOOL | true |  |
| IsDeleted | BOOL | false |  |

There are two job types: Block | Application, block job use node as a distributed downloader, startrequestbuilder & processor & pipeline is a worker will be called by Scheduler.NET. Application use node as a agent, run a full application in each node.

#### JobProperty

| Column | DataType | Value| Key |
|:---|:---|:---|:---|
|Id| INT | 1 | Primary |
|JobId| VARCHAR(32)| GUID | INDEX_JOB_ID |
| Property | VARCHAR(32) | NODE_COUNT |  |
| Value | VARCHAR(256)  | 1 |  |


ApplicationJob's properties

		NodeCount : 1
		NodeGroup : Vps | InHouse | Vps_Static_Ip
		Package : Http://a.com/app1.zip
		OS : Windows


#### Node

| Column | DataType | Value| Key |
|:---|:---|:---|:---|
|NodeId| VARCHAR(32)| GUID | Primary |
|Ip| VARCHAR(32)| 192.168.90.100 |  |
| CpuCount | INT | 8 |  |
| Group | VARCHAR(32)  | vps_redial \| inhouse \| vps_static_ip |  |
| Os  | VARCHAR(32) | windows \| linux | |
| TotalMemory | INT | 2000 | 
| IsEnable | BOOL |  | true

#### NodeHeartbeat

| Column | DataType | Value| Key |
|:---|:---|:---|:---|
|Id| LONG | 1 | Primary |
|NodeId| VARCHAR(32)| GUID | INDEX_NODE_ID |
| ProcessCount | INT | 2| |
|Cpu|INT|20||
|FreeMemory|INT| 2000||

#### Block

| Column | DataType | Value| Key |
|:---|:---|:---|:---|
|BlockId| VARCHAR(32)| GUID | Primary |
|Identity| VARCHAR(32)| GUID | INDEX_IDENTITY |
|ChangeIpPattern|VARCHAR(50)||
|Exception|VARCHAR(MAX)||
|Status|INT| Ready (1)\|Using (2)\|Success (3)\|Failed (4)\|Retry (5)||

#### Running

| Column | DataType | Value|	 Key |
|:---|:---|:---|:---|
|JobId|VARCHAR(32)| GUID | Primary |
|Identity| VARCHAR(32)| GUID | Primary |
|ThreadNum| INT | 2 |  |
|Site| VARCHAR(MAX)|  |  |
|Priority| INT| 0 |  |
|BlockTimes| INT| 0 |  |

##### 任务管理

1. Portal 通过 Broker API 添加一个任务
		
		NAME: TASK1
		CRON: */5 * * * *
		PROCESSOR: Console.SampleProcessor
		ARGUMENTS:
		DESCRIPTION: This is a test task.

+ 通过 Scheduler.NET API 添加任务并获得返回的 SchedulerNetId, 如果成功则执行下一步操作，失败则抛异常
+ 添加 Job 信息到数据库中

2. Portal 通过 Broker API 修改一个任务

+ 通过 Scheduler.NET API 修改任务， 如果修改成功则执行下一步操作，失败则抛异常
+ 修改数据库中的 Job 信息

3. Portal 通过 Broker API 删除一个任务

+ 通过 Scheduler.NET API 删除任务， 如果删除成功则执行下一步操作，失败则抛异常
+ 从数据库中删除 Job 信息

##### 任务执行

1. 启动 Wroker 实例





