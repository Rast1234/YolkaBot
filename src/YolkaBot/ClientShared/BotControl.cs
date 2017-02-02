using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace YolkaBot.Client
{
    public class BotControl
    {
        ActionRequest current;
        ActionRequest previous;
        string host;
        int port;
        TimeSpan timeout;
        TcpClient tcpClient;
        StreamReader reader;
        StreamWriter writer;

        public string Status {
            get
            {
                var connected = tcpClient?.Connected == true ? "connected" : "no connection";
                return $"{connected} / {response}";
            }
        }

        private string response;

        private static SemaphoreSlim connectSemaphore = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim sendSemaphore = new SemaphoreSlim(1, 1);

        public BotControl(string host, int port, TimeSpan timeout)
        {
            this.host = host;
            this.port = port;
            this.timeout = timeout;
        }

        public async Task Run()
        {
            await Connect();
            await Receive();
        }

        private async Task Connect()
        {
            await connectSemaphore.WaitAsync();
            try
            {
                await ConnectInternal();
            }
            finally
            {
                connectSemaphore.Release();
            }
        }

        private async Task ConnectInternal()
        {
            int i = 0;
            while (true)
            {
                i++;
                System.Diagnostics.Debug.WriteLine($"ConnectInternal / iteration {i}");
                try
                {
                    if (tcpClient == null || !tcpClient.Connected)
                    {
                        tcpClient = new TcpClient();
                        await tcpClient.ConnectAsync(host, port);
                        var stream = tcpClient.GetStream();
                        reader = new StreamReader(stream);
                        writer = new StreamWriter(stream);
                        System.Diagnostics.Debug.WriteLine($"connection ok!");
                        return;
                    } else
                    {
                        System.Diagnostics.Debug.WriteLine($"connection already established!");
                        return;
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception at {nameof(ConnectInternal)}: {e}");
                    tcpClient = null;
                    reader = null;
                    writer = null;
                    await Task.Delay(timeout);
                }
            }
        }

        private async Task Send()
        {
            if (await sendSemaphore.WaitAsync(0))
            {
                try
                {
                    await SendInternal();
                }
                finally
                {
                    sendSemaphore.Release();
                }
            }
        }

        private async Task SendInternal()
        {
            int i = 0;
            while (true)
            {
                i++;
                System.Diagnostics.Debug.WriteLine($"Send / iteration {i}");
                try
                {
                    if (current.Activate && previous?.Activate != true)
                    {
                        await writer.WriteLineAsync($"active");
                    }
                    if (current.Stop && previous?.Stop != true)
                    {
                        await writer.WriteLineAsync($"stop");
                    }
                    if (current.Left != previous?.Left)
                    {
                        await writer.WriteLineAsync($"left {current.Left}");
                    }
                    if (current.Right != previous?.Right)
                    {
                        await writer.WriteLineAsync($"right {current.Right}");
                    }
                    await writer.FlushAsync();
                    return;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception at {nameof(Send)}: {e}");
                    await Connect();
                    return; // do not queue requests, discard failed
                }
            }
            
        }

        

        private async Task Receive()
        {
            int i = 0;
            while (true)
            {
                i++;
                System.Diagnostics.Debug.WriteLine($"Receive / iteration {i}");
                try
                {
                    response = await reader.ReadLineAsync();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception at {nameof(Receive)}: {e}");
                    await Connect();
                }
            }
        }

        public async Task Update(ActionRequest action)
        {
            previous = current;
            current = action;

            await Send();
        }
    }
}
