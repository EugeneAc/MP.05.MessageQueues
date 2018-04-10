namespace QueueServer
{
    using System;
    using System.IO;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            var server = new QueueServer(@"D:\");
            server.Start();
            server.SendBroadcastMessage("Update Status", null);
            while (true)
            {
                Console.WriteLine("Update status again? (y/n)");

                if (Console.ReadLine() == "y")
                    server.SendBroadcastMessage("Update Status", null);
                var messagefile = File.ReadAllLines("StatusMessages.txt");
                Thread.Sleep(2000);
                foreach (var s in messagefile)
                {
                    Console.WriteLine(s);
                }
            }
        }
    }
}
