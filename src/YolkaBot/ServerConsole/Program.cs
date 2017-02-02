namespace YolkaBot.Server.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var port = 42038;
            if (args.Length != 0)
                port = int.Parse(args[0]);
            var control = new Control(port);
            control.Run().Wait();
        }
    }
}