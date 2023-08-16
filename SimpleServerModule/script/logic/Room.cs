using System;
using System.Collections.Generic;

public class Room
{
    //id
    public int id = 0;
    //最大玩家数
    public int maxPlayer = 6;
    //玩家列表
    public Dictionary<string, bool> playerIds = new Dictionary<string, bool>();
    //房主id
    public string ownerId = "";

    //状态
    public enum Status
    {
        PREPARE = 0,
        FIGHT = 1,
    }
    public Status status = Status.PREPARE;

    //出生点位置配置
    static float[,,] birthConfig = new float[2, 3, 6] {
        //阵营1出生点
        {
            {-85.8f, 3.8f, -33.8f, 0, 24.9f, 0f}, //出生点1
            {-49.9f, 3.8f, -61.4f, 0, 21.4f, 0f}, //出生点2
            {-6.2f,  3.8f, -70.7f, 0, 21.9f, 0f}, //出生点3
        },
        //阵营2出生点
        {
            {150f, 0f, 178.9f, 0, -156.8f, 0f}, //出生点1
            {105f, 0f, 216.5f, 0, -156.8f, 0f}, //出生点2
            {52.0f,0f, 239.2f, 0, -156.8f, 0f}, //出生点3
        },
    };

    //添加玩家
    public bool AddPlayer(string id)
    {
        //获取玩家
        Player player = PlayerManager.GetPlayer(id);
        if (player == null)
        {
            Console.WriteLine("room.AddPlayer fail, player is null");
            return false;
        }
        //房间人数
        if (playerIds.Count >= maxPlayer)
        {
            Console.WriteLine("room.AddPlayer fail, reach maxPlayer");
            return false;
        }
        //准备状态才能加入
        if (status != Status.PREPARE)
        {
            Console.WriteLine("room.AddPlayer fail, not PREPARE");
            return false;
        }
        //已经在房间里
        if (playerIds.ContainsKey(id))
        {
            Console.WriteLine("room.AddPlayer fail, already in this room");
            return false;
        }
        //加入列表
        playerIds[id] = true;
        //设置玩家数据
        player.camp = SwitchCamp();
        player.roomId = this.id;
        //设置房主
        if (ownerId == "")
        {
            ownerId = player.id;
        }
        //广播
        Broadcast(ToMsg());
        return true;
    }

    //分配阵营
    public int SwitchCamp()
    {
        //计数
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            if (player.camp == 1) { count1++; }
            if (player.camp == 2) { count2++; }
        }
        //选择
        if (count1 <= count2)
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    //删除玩家
    public bool RemovePlayer(string id)
    {
        //获取玩家
        Player player = PlayerManager.GetPlayer(id);
        if (player == null)
        {
            Console.WriteLine("room.RemovePlayer fail, player is null");
            return false;
        }
        //没有在房间里
        if (!playerIds.ContainsKey(id))
        {
            Console.WriteLine("room.RemovePlayer fail, not in this room");
            return false;
        }
        //删除列表
        playerIds.Remove(id);
        //设置玩家数据
        player.camp = 0;
        player.roomId = -1;
        //设置房主
        if (isOwner(player))
        {
            ownerId = SwitchOwner();
        }
        //战斗状态退出
        if (status == Status.FIGHT)
        {
            player.data.lost++;
            MsgLeaveBattle msg = new MsgLeaveBattle();
            msg.id = player.id;
            Broadcast(msg);
        }
        //房间为空
        if (playerIds.Count == 0)
        {
            RoomManager.RemoveRoom(this.id);
        }
        //广播
        Broadcast(ToMsg());
        return true;
    }

    //是不是房主
    public bool isOwner(Player player)
    {
        return player.id == ownerId;
    }

    //选择房主
    public string SwitchOwner()
    {
        //选择第一个玩家
        foreach (string id in playerIds.Keys)
        {
            return id;
        }
        //房间没人
        return "";
    }

    //广播消息
    public void Broadcast(MsgBase msg)
    {
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            player.Send(msg);
        }
    }

    //生成MsgGetRoomInfo协议
    public MsgBase ToMsg()
    {
        MsgGetRoomInfo msg = new MsgGetRoomInfo();
        int count = playerIds.Count;
        msg.players = new PlayerInfo[count];
        //players
        int i = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            PlayerInfo playerInfo = new PlayerInfo();
            //赋值
            playerInfo.id = player.id;
            playerInfo.camp = player.camp;
            playerInfo.win = player.data.win;
            playerInfo.lost = player.data.lost;
            playerInfo.isOwner = 0;
            if (isOwner(player))
            {
                playerInfo.isOwner = 1;
            }

            msg.players[i] = playerInfo;
            i++;
        }
        return msg;
    }

    //能否开战
    public bool CanStartBattle()
    {
        //已经是战斗状态
        if (status != Status.PREPARE)
        {
            return false;
        }
        //统计每个阵营的玩家数
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            if (player.camp == 1) { count1++; }
            else { count2++; }
        }
        //每个阵营至少要有1名玩家
        if (count1 < 1 || count2 < 1)
        {
            return false;
        }

        return true;
    }


    //初始化位置
    private void SetBirthPos(Player player, int index)
    {
        int camp = player.camp;

        player.x = birthConfig[camp - 1, index, 0];
        player.y = birthConfig[camp - 1, index, 1];
        player.z = birthConfig[camp - 1, index, 2];
        player.ex = birthConfig[camp - 1, index, 3];
        player.ey = birthConfig[camp - 1, index, 4];
        player.ez = birthConfig[camp - 1, index, 5];
    }

    //重置玩家战斗属性
    private void ResetPlayers()
    {
        //位置和旋转
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            if (player.camp == 1)
            {
                SetBirthPos(player, count1);
                count1++;
            }
            else
            {
                SetBirthPos(player, count2);
                count2++;
            }
        }
        //生命值
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            player.hp = 100;
        }
    }

    //玩家数据转成TankInfo
    public TankInfo PlayerToTankInfo(Player player)
    {
        TankInfo tankInfo = new TankInfo();
        tankInfo.camp = player.camp;
        tankInfo.id = player.id;
        tankInfo.hp = player.hp;

        tankInfo.x = player.x;
        tankInfo.y = player.y;
        tankInfo.z = player.z;
        tankInfo.ex = player.ex;
        tankInfo.ey = player.ey;
        tankInfo.ez = player.ez;

        return tankInfo;
    }

    //开战
    public bool StartBattle()
    {
        if (!CanStartBattle())
        {
            return false;
        }
        //状态
        status = Status.FIGHT;
        //玩家战斗属性
        ResetPlayers();
        //返回数据
        MsgEnterBattle msg = new MsgEnterBattle();
        msg.mapId = 1;
        msg.tanks = new TankInfo[playerIds.Count];

        int i = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            msg.tanks[i] = PlayerToTankInfo(player);
            i++;
        }
        Broadcast(msg);
        return true;
    }

    //是否死亡
    public bool IsDie(Player player)
    {
        return player.hp <= 0;
    }

    //胜负判断
    public int Judgment()
    {
        //存活人数
        int count1 = 0;
        int count2 = 0;
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            if (!IsDie(player))
            {
                if (player.camp == 1) { count1++; };
                if (player.camp == 2) { count2++; };
            }
        }
        //判断
        if (count1 <= 0)
        {
            return 2;
        }
        else if (count2 <= 0)
        {
            return 1;
        }
        return 0;
    }

    //上一次判断结果的时间
    private long lastJudgeTime = 0;

    //定时更新
    public void Update()
    {
        //状态判断
        if (status != Status.FIGHT)
        {
            return;
        }
        //时间判断
        if (NetManager.GetTimeStamp() - lastJudgeTime < 10f)
        {
            return;
        }
        lastJudgeTime = NetManager.GetTimeStamp();
        //胜负判断
        int winCamp = Judgment();
        //尚未分出胜负
        if (winCamp == 0)
        {
            return;
        }
        //某一方胜利，结束战斗
        status = Status.PREPARE;
        //统计信息
        foreach (string id in playerIds.Keys)
        {
            Player player = PlayerManager.GetPlayer(id);
            if (player.camp == winCamp) { player.data.win++; }
            else { player.data.lost++; }
        }
        //发送Result
        MsgBattleResult msg = new MsgBattleResult();
        msg.winCamp = winCamp;
        Broadcast(msg);
    }
}