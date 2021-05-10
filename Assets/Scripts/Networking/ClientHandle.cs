//Code in this file was written whilst following along with Tom Weiland's C# Networking Tutorial.  Link below.
//https://www.youtube.com/watch?v=4uHTSknGJaY&list=PLXkn83W0QkfnqsK8I0RAz5AbUxfg3bOQ5&index=2

using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void welcome(Packet packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        Debug.Log($"Your ID: {myId}");
        Client.instance.myId = myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(Packet packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 position = packet.ReadVector3();
        Quaternion rotation = packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(id, username, position, rotation);
    }

    //Updates a player's position with the data received from the server
    public static void PlayerPosition(Packet packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        GameManager.players[id].transform.position = position;
    }

    //Updates a player's rotation with the data received from the server
    public static void PlayerRotation(Packet packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();

        Vector3 serverEuler = rotation.eulerAngles;
        if(id == Client.instance.myId) GameManager.players[id].transform.eulerAngles = new Vector3(serverEuler.x, serverEuler.y, -GameManager.players[id].player.barAngle);
        else GameManager.players[id].transform.eulerAngles = new Vector3(serverEuler.x, serverEuler.y, serverEuler.z);
    }

    //Triggers client crash code
    public static void PlayerCrashed(Packet packet)
    {
        bool hasCrashed = packet.ReadBool();

        GameManager.Crash();
    }

    public static void PreReset(Packet packet)
    {
        GameManager.PreReset();
    }

    public static void RoundReset(Packet packet)
    {
        GameManager.instance.RoundReset();
    }


}
