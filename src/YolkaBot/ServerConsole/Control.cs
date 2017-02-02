using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace YolkaBot.Server.Console
{
    internal class Control
    {
        public readonly AsyncService server;
        public readonly ArduinoReliableTransport transport;
        public int activeClient;
        public ConcurrentDictionary<EndPoint, int> clients;


        public Control(int port)
        {
            clients = new ConcurrentDictionary<EndPoint, int>();
            server = new AsyncService(ProcessRequest, clients, port, true);
            transport = new ArduinoReliableTransport();
        }


        public async Task Run()
        {
            try
            {
                await transport.Call(Command.Status);
                await server.Run();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
                System.Console.ReadLine();
            }
        }


        public async Task<string> ProcessRequest(string request, EndPoint client, int clientNumber)
        {
            var data = request.Split(' ');
            var command = (Command) Enum.Parse(typeof(Command), data[0], true);
            var result = "";
            switch (command)
            {
                case Command.Left:
                case Command.Right:
                    if (IsActiveClient(client))
                        result = await transport.Call(command, int.Parse(data[1]));
                    break;
                case Command.Stop:
                case Command.Status:
                    if (IsActiveClient(client))
                        result = await transport.Call(command);
                    break;
                case Command.Active:
                    // nothing to send, need to edit server variable and use it in above stuff
                    //var value = int.Parse(data[1]);
                    int value;
                    if (clients.TryGetValue(client, out value))
                    {
                        activeClient = value;
                        System.Console.WriteLine(
                            $"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [{client} / {clientNumber}] is active now");
                    }
                    break;
                case Command.List:
                    result = string.Join(" ## ", clients.Select(kv => $"{kv.Key} / {kv.Value}"));
                    System.Console.WriteLine(
                        $"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [{client} / {clientNumber}] LIST: [{result}]");
                    break;
                default:
                    result = "unknown command";
                    break;
            }
            var response = result + $" | active: {IsActiveClient(client)}";
            return response;
        }

        private bool IsActiveClient(EndPoint client)
        {
            int x;
            clients.TryGetValue(client, out x);
            return x == activeClient;
        }
    }
}