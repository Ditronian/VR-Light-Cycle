using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    
    public static void WelcomeReceived(int fromClient, Packet packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($"{Server.clients[fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}.");
        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player \"{username}\" (ID: {fromClient} has assumed the wrong client ID ({clientIdCheck})!");
        }
        // Send player into the game
        Server.clients[fromClient].SendIntoGame(username);
    }

    public static void PlayerTurn(int fromClient, Packet packet)
    {
        float roll = packet.ReadFloat();

        Server.clients[fromClient].player.SetRoll(roll);
    }
}
