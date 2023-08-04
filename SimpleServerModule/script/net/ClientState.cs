using System.Net.Sockets;

public class ClientState
{
    public Socket socket;
    public ByteArray readBuff = new ByteArray();
    //玩家数据后面添加
    public long lastPingTime = 0;

    // 玩家
    public Player player;
}