using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Reflection;

// 一套通用C#服务端框架，该框架为单进程单线程架构，使用Select多路复用处理网络连接，
// 用MySQL数据库保存玩家数据，具有粘包半包处理、心跳机制、消息分发等功能。
// 一个单进程单线程服务器只能处理几百名玩家，大型服务器大多是分布式结构，协同工作，同时承载数十万玩家。

class SelectServer
{
    //public static void Main(string[] args)
    //{
    //    //Socket
    //    listenfd = new Socket(AddressFamily.InterNetwork,
    //        SocketType.Stream, ProtocolType.Tcp);
    //    //Bind
    //    IPAddress ipAdr = IPAddress.Parse("127.0.0.1");
    //    IPEndPoint ipEp = new IPEndPoint(ipAdr, 8888);
    //    listenfd.Bind(ipEp);
    //    //Listen
    //    listenfd.Listen(0);
    //    Console.WriteLine("[服务器]启动成功");
    //    //checkRead
    //    List<Socket> checkRead = new List<Socket>();
    //    //主循环
    //    while (true)
    //    {
    //        //填充checkRead列表
    //        checkRead.Clear();
    //        checkRead.Add(listenfd);
    //        foreach (ClientState s in clients.Values)
    //        {
    //            checkRead.Add(s.socket);
    //        }
    //        //select
    //        Socket.Select(checkRead, null, null, 1000);
    //        //检查可读对象
    //        foreach (Socket s in checkRead)
    //        {
    //            if (s == listenfd)
    //            {
    //                ReadListenfd(s);
    //            }
    //            else
    //            {
    //                ReadClientfd(s);
    //            }
    //        }
    //    }
    //}

    public static void Main(string[] args)
    {
        if (!DbManager.Connect("game", "127.0.0.1", 3306, "root", "Zilisen1123")) return;

        //if (DbManager.Register("zilisen", "123456"))
        //{
        //    Console.WriteLine("注册成功{0}", 1);
        //}
        ////测试
        //DbManager.CreatePlayer("aglab");
        //PlayerData pd = DbManager.GetPlayerData("aglab");
        //pd.coin = 256;
        //DbManager.UpdatePlayerData("aglab", pd);

        NetManager.StartLoop(8888);
    }
    

}