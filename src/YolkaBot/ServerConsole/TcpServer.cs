using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace YolkaBot.Server.Console
{
    public class AsyncService
    {
        private readonly ConcurrentDictionary<EndPoint, int> clients;
        private readonly IPAddress ipAddress;
        private readonly int port;
        private readonly Func<string, EndPoint, int, Task<string>> processingFunc;
        private int clientCounter;

        public AsyncService(Func<string, EndPoint, int, Task<string>> processingFunc,
            ConcurrentDictionary<EndPoint, int> clients, int port, bool anyAddress)
        {
            this.processingFunc = processingFunc;
            this.clients = clients;
            this.port = port;

            var hostName = Dns.GetHostName();
            var ipHostInfo = Dns.GetHostEntry(hostName);

            if (anyAddress)
            {
                ipAddress = IPAddress.Any;
            }
            else
            {
                ipAddress = null;
                for (var i = 0; i < ipHostInfo.AddressList.Length; ++i)
                    if (ipHostInfo.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = ipHostInfo.AddressList[i];
                        break;
                    }
            }
            if (ipAddress == null)
                throw new Exception("No IPv4 address for server");
        }

        public async Task Run()
        {
            while (true)
                try
                {
                    var listener = new TcpListener(ipAddress, port);
                    listener.Start();
                    System.Console.WriteLine("\nArduinoControlServer running on port " + port);
                    await RunInternal(listener);
                }
                catch (Exception e)
                {
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [TCPSERVER] !! {e}...");
                    await Task.Delay(1000);
                }
        }

        private async Task RunInternal(TcpListener listener)
        {
            while (true)
                try
                {
                    var tcpClient = await listener.AcceptTcpClientAsync();
                    await Process(tcpClient); // don't care about results
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
        }

        private async Task Process(TcpClient tcpClient)
        {
            var clientEndPoint = tcpClient.Client.RemoteEndPoint;

            var clientNumber = clients.GetOrAdd(clientEndPoint, _ => clientCounter++);
            System.Console.WriteLine(
                $"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [{clientEndPoint} / {clientNumber}] connected");
            try
            {
                var networkStream = tcpClient.GetStream();
                var reader = new StreamReader(networkStream);
                var writer = new StreamWriter(networkStream);
                writer.AutoFlush = true;
                while (true)
                {
                    var request = (await reader.ReadLineAsync())?.Trim();
                    if (request != null)
                    {
                        System.Console.WriteLine(
                            $"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [{clientEndPoint} / {clientNumber}] >> [{request}]");
                        try
                        {
                            var response = await processingFunc(request, clientEndPoint, clientNumber);
                            if (response != null)
                            {
                                System.Console.WriteLine(
                                    $"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [{clientEndPoint} / {clientNumber}] << [{response}]");
                                await writer.WriteLineAsync(response);
                            }
                        }
                        catch (Exception e)
                        {
                            System.Console.WriteLine(
                                $"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [{clientEndPoint} / {clientNumber}] EXCEPTION {e}");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine(
                            $"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [{clientEndPoint} / {clientNumber}] disconnected");
                        int x;
                        clients.TryRemove(clientEndPoint, out x);
                        break; // client closed connection
                    }
                }
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                if (tcpClient.Connected)
                    tcpClient.Close();
            }
        }
    }
}