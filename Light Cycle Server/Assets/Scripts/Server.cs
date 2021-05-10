using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class Server
{

    private static int maxPlayers;
    private static int port;
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int fromClient, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;
    public static int livingPlayers;
    public static GameObject gameManager;
    public static GameObject networkManager;
    public static bool hasStarted = false;


    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static void Start(int _maxPlayers, int _port)
    {
        maxPlayers = _maxPlayers;
        port = _port;

        Debug.Log($"Starting Server...");
        initializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

        udpListener = new UdpClient(port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Debug.Log($"Server started on {port}.");
    }

    public static void Restart()
    {
        //Tell players to reset their views
        //ServerSend.PreReset();

        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        for (var i = 0; i < walls.Length; i++) GameObject.Destroy(walls[i]);

        //Reset Server Clients to spawn fresh players
        foreach(Client client in clients.Values)
        {
            if(client.player != null)
            {
                Player player = client.player;
                player.transform.position = NetworkManager.instance.spawnLocation[player.id - 1];
                player.transform.rotation = Quaternion.identity;
                player.isDead = false;
            }
        }

        //Tell players to reset their views
        ServerSend.RoundReset();
    }

    public static void StartGame()
    {
        foreach (Client client in clients.Values)
        {
            if (client.player != null)
            {
                Player player = client.player;
                player.isDead = false;
            }
        }
    }

    private static void TCPConnectCallback(IAsyncResult result)
    {
        TcpClient client = tcpListener.EndAcceptTcpClient(result);
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        Debug.Log($"Incoming connection from {client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= maxPlayers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(client);
                return;
            }
        }

        Debug.Log($"{client.Client.RemoteEndPoint} failed to connect:  Server is full!");
    }

    private static void UDPReceiveCallback(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (data.Length < 4)
            {
                return;
            }

            using (Packet packet = new Packet(data))
            {
                int clientId = packet.ReadInt();

                if (clientId == 0) return;

                if (clients[clientId].udp.endPoint == null)
                {
                    clients[clientId].udp.Connect(clientEndPoint);
                    return;
                }

                if (clients[clientId].udp.endPoint.ToString() == clientEndPoint.ToString())
                {
                    clients[clientId].udp.HandleData(packet);
                }
            }


        }
        catch (Exception ex)
        {
            Debug.Log($"Error received UDP data: {ex}");
        }
    }

    public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
    {
        try
        {
            if (clientEndPoint != null)
            {
                udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            }
        }
        catch (Exception ex)
        {
            Debug.Log($"Error sending data to {clientEndPoint} via UDP: {ex}");
        }
    }

    private static void initializeServerData()
    {
        for (int i = 1; i <= maxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                {(int)ClientPackets.playerTurn, ServerHandle.PlayerTurn },
            };

        Debug.Log("Initialized packets.");
    }


    public static int MaxPlayers { get => maxPlayers; set => maxPlayers = value; }
    public static int Port1 { get => port; set => port = value; }
}

