using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    private static string username;

    static void Main()
    {
        do
        {
            Console.Write("Digite seu nome de usuário: ");
            username = Console.ReadLine();
        } while (string.IsNullOrEmpty(username));

        do
        {
            Console.Write("Digite o endereço IP do servidor: ");
            string ipAddress = Console.ReadLine();
        } while (string.IsNullOrEmpty(ipAddress));

        do
        {
            Console.Write("Digite a porta do servidor: ");
            int port = int.Parse(Console.ReadLine());
        } while (string.IsNullOrEmpty(port));

//        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
//        int port = 11000;

        TcpClient client = new TcpClient();
        try
        {
            client.Connect(ipAddress, port);
            Console.WriteLine($"Conectado ao servidor! Bem vindo(a), {username}!");

            NetworkStream stream = client.GetStream();
            byte[] usernameData = Encoding.UTF8.GetBytes(username);
            stream.Write(usernameData, 0, usernameData.Length);

            Thread readThread = new Thread(() => ReadMessages(stream));
            readThread.Start();

            while (true)
            {
                string message = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Conexão perdida!");
        }
        finally
        {
            client.Close();
        }
    }

    private static void ReadMessages(NetworkStream stream)
    {
        byte[] buffer = new byte[256];
        int bytesRead;

        try
        {
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(message);
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Você foi desconectado(a).");
        }
    }
}
