# SimpleServerModule

 a simple server frame made with C#

一套通用C#服务端框架，该框架为单进程单线程架构，使用Select多路复用处理网络连接，用MySQL数据库保存玩家数据，具有粘包半包处理、心跳机制、消息分发等功能。一个单进程单线程服务器只能处理几百名玩家，大型服务器大多是分布式结构，协同工作，同时承载数十万玩家。

参考[Unity3D网络游戏实战](https://luopeiyu.github.io/unity_net_book/)实现的简单记事本服务端，对应客户端：
