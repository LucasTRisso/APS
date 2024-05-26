using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    private static List<TcpClient> clients = new List<TcpClient>();
    private static Dictionary<TcpClient, string> clientUsernames = new Dictionary<TcpClient, string>();
    static void Main()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 11000;

        TcpListener server = new TcpListener(ipAddress, port);
        server.Start();
        Console.WriteLine("Servidor iniciado. Aguardando conexões...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            clients.Add(client);
            string username = ReceiveUsername(client);
            clientUsernames.Add(client, username);
            Console.WriteLine($"{username} se conectou.");
            BroadcastMessage($"{username} acabou de entrar!", client);


            Thread clientThread = new Thread(() => 
{
            try
            {
                HandleClient(client, username);
            }
            catch (Exception)
            {
                Console.WriteLine($"{username} se desconectou.");
                BroadcastMessage($"{username} saiu!", client);
            }});
            
            clientThread.Start();
        }
    }

    private static string ReceiveUsername(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        byte[] usernameBuffer = new byte[256];
        int bytesRead = stream.Read(usernameBuffer, 0, usernameBuffer.Length);
        string username = Encoding.UTF8.GetString(usernameBuffer, 0, bytesRead);
        return username;
    }

    private static void HandleClient(TcpClient client, string username)
    {

            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[256];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = $"{username}:" + $"{Encoding.UTF8.GetString(buffer, 0, bytesRead)}";
                Console.WriteLine($"{message}");
                BroadcastMessage(message, client);
            }

            client.Close();
            Console.WriteLine($"{username} se desconectou.");
            BroadcastMessage($"{username} saiu!", client);
            clients.Remove(client);
            clientUsernames.Remove(client);
    }

    private static void BroadcastMessage(string message, TcpClient excludeClient)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        foreach (var client in clients)
        {
            if (client != excludeClient)
            {
                NetworkStream stream = client.GetStream();
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
