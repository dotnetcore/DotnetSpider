### 角色

**Broker**

+ 分发请求块（Blockoutput）： POST http://broker/node/heartbeat
+ 接收分布式下载器的回传数据 (Blockinput): POST http://broker/node/block
+ 接收 Request 打包成 Block, 插入 Request 队列: POST http://broker/requestqueue/{identity}
+ 通过 Identity 查询已经完成的 Request 以 Block 为单位 : GET http://broker/requestqueue/{identity}

#### JOB

| Column | DataType | Value| Key |
|:---|:---|:---|:---|
|Id| VARCHAR(32)| GUID | Primary |
| JobType | INT | Block \| Application |  |
| Name | VARCHAR(50) | TASK1 |  |  |

#### JobProperty

		JobId VARCHAR(32): GUID
		NodeCount INT: 1
		NodeGroup VARCHAR(50): Vps | InHouse | Vps_Static_Ip
		Package VARCHAR(256): Http://a.com/app1.zip
		OS VARCHAR(50): Windows



+ 任务类型有两种：Block | Application, Block 是使用分布式下载器下载内容, 解析器和数据管道都在 Worker 里

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





