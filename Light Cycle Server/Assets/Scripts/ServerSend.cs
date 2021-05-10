using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    private static void sendTCPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].tcp.sendData(packet);
    }

    private static void SendUDPData(int toClient, Packet packet)
    {
        packet.WriteLength();
        Server.clients[toClient].udp.SendData(packet);
    }

    private static void sendTCPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient) Server.clients[i].tcp.sendData(packet);
        }
    }

    private static void sendTCPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].tcp.sendData(packet);
        }
    }

    private static void sendUDPDataToAll(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != exceptClient) Server.clients[i].udp.SendData(packet);
        }
    }

    private static void sendUDPDataToAll(Packet packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.clients[i].udp.SendData(packet);
        }
    }

    #region Packets
    public static void welcome(int toClient, string msg)
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome))
        {
            packet.Write(msg);
            packet.Write(toClient);

            sendTCPData(toClient, packet);
        };
    }

    public static void SpawnPlayer(int toClient, Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
        {
            packet.Write(player.id);
            packet.Write(player.username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            sendTCPData(toClient, packet);
        };
    }

    //Send player a crash notification
    public static void PlayerCrashed(int toClient, Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerCrashed))
        {
            packet.Write(true);

            sendTCPData(toClient, packet);
        };
    }

    //Send player a notification to do pre-reset work
    public static void PreReset()
    {
        using (Packet packet = new Packet((int)ServerPackets.PreReset))
        {
            packet.Write(true);

            sendTCPDataToAll(packet);
        };
    }

    //Send player a notification to reset the grid
    public static void RoundReset()
    {
        using (Packet packet = new Packet((int)ServerPackets.RoundReset))
        {
            packet.Write(true);

            sendTCPDataToAll(packet);
        };
    }

    //Send the player's position information to ALL clients via UDP
    public static void PlayerPosition(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerPosition))
        {
            packet.Write(player.id);
            packet.Write(player.transform.position);

            sendUDPDataToAll(packet);
        };
    }

    //Send the player's rotation information to ALL clients except that player, via UDP
    //We do not send rotation information to the owning player because we dont want to overwrite his rotation.
    //JK we actually do, because I have decided rotation is the cycle's rotation, and I want server authority over it.
    //This prevents rapid cheat turns.
    public static void PlayerRotation(Player player)
    {
        using (Packet packet = new Packet((int)ServerPackets.playerRotation))
        {
            packet.Write(player.id);
            packet.Write(player.transform.rotation);

            sendUDPDataToAll(packet);
        };
    }
    #endregion

}
    

