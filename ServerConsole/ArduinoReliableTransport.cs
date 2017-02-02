using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandMessenger;
using CommandMessenger.Transport.Serial;
using System.Management;
using System.Threading;


namespace YolkaBot.Server.Console
{
    class ArduinoReliableTransport
    {
        public SerialTransport transport;
        public CmdMessenger messenger;
        TimeSpan cmdTimeout;
        TimeSpan updateTimeout;

        private static SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);


        public ArduinoReliableTransport(int timeout=100)
        {
            cmdTimeout = TimeSpan.FromMilliseconds(timeout);
            updateTimeout = TimeSpan.FromSeconds(1);
            Update();
        }


        public async Task<string> Call(Command command, params dynamic[] args)
        {
            var cmd = CreateCommand(command);
            foreach(var i in args)
            {
                cmd.AddArgument(i);
            }
            var response = await Interact(cmd);
            var left = response.ReadInt32Arg();
            var right = response.ReadInt32Arg();
            return $"[left {left} || right {right}]";
        }

        private async Task<ReceivedCommand> Interact(SendCommand command)
        {
            await semaphore.WaitAsync();
            try
            {
                return await Task.Run(() => InteractInternal(command));
            }
            finally
            {
                semaphore.Release();
            }
        }

        private ReceivedCommand InteractInternal(SendCommand command)
        {
            while(true)
            {
                var resultCommand = messenger.SendCommand(command);
                if (resultCommand.Ok)
                {
                    return resultCommand;
                }
                else
                {
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [SERIAL] !! NO RESPONSE!");
                    Update();
                }
            }
        }

        private SendCommand CreateCommand(Command command)
        {
            return new SendCommand((int)command, (int)Command.Status, (int)cmdTimeout.TotalMilliseconds);
        }

        private static void OnUnknownCommand(ReceivedCommand arguments)
        {
            System.Console.WriteLine($"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [SERIAL] >> UNKNOWN [{arguments.CommandString()}]");
        }

        private static void OnStatus(ReceivedCommand arguments)
        {
            var left = arguments.ReadInt32Arg();
            var right = arguments.ReadInt32Arg();
            System.Console.WriteLine($"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [SERIAL] >> STATUS [left {left} || right {right}]");
        }

        private static void NewLineReceived(object sender, CommandEventArgs e)
        {
            System.Console.WriteLine($"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [SERIAL] >> {e.Command.CommandString()}");
        }

        private static void NewLineSent(object sender, CommandEventArgs e)
        {
            System.Console.WriteLine($"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [SERIAL] << {e.Command.CommandString()}");
        }

        private void Update()
        {
            bool success = false;
            string port = null;
            int attempt = 0;
            while (true)
            {
                attempt++;
                Thread.Sleep(updateTimeout);
                System.Console.WriteLine($"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [UPDATER] !! attempt {attempt}...");
                try
                {
                    if (transport == null || messenger == null || !transport.IsConnected())
                    {
                        //messenger?.Disconnect();
                        messenger?.Dispose();
                        messenger = null;
                        //transport?.Disconnect();
                        transport?.Dispose();
                        transport = null;

                        port = AutodetectArduinoPort();
                        transport = GetTransport(port);
                        messenger = GetMessenger(transport);
                    }
                    if (transport.IsConnected())
                    {
                        success = true;
                    }
                }
                catch (Exception e)
                {
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [UPDATER] !! {e}");
                    success = false;
                }
                if(success)
                {
                    System.Console.WriteLine($"[{DateTime.Now.ToString("HH':'mm':'ss'.'fffffff")}] [UPDATER] !! connected at port {port} at {attempt} attempt");
                    break;
                }
            }
        }

        private static SerialTransport GetTransport(string port)
        {
            return new SerialTransport
            {
                CurrentSerialSettings = { PortName = port, BaudRate = 115200, DtrEnable = true }
            };
        }

        private static CmdMessenger GetMessenger(SerialTransport transport)
        {
            var messenger = new CmdMessenger(transport, BoardType.Bit16);
            messenger.Attach(OnUnknownCommand);
            messenger.Attach((int)Command.Status, OnStatus);
            messenger.NewLineReceived += NewLineReceived;
            messenger.NewLineSent += NewLineSent;
            messenger.Connect();

            return messenger;
        }

        private static string AutodetectArduinoPort()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                return "/dev/ttyACM0";  // can fail too if more than one USBSERIAL connected
            }

            var connectionScope = new ManagementScope();
            var serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            var searcher = new ManagementObjectSearcher(connectionScope, serialQuery);
            foreach (ManagementObject item in searcher.Get())
            {
                string desc = item["Description"].ToString();
                string deviceId = item["DeviceID"].ToString();

                if (desc.Contains("Arduino"))
                {
                    return deviceId;
                }
            }
            return null;
        }

    }
}
