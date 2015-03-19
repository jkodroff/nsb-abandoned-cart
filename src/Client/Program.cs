using System;
using System.Linq;
using System.Text;
using Messages;
using NServiceBus;

namespace Client
{
    internal class Program
    {
        private static char[] ValidKeys = "1234".ToCharArray();
        private static string User1 = "Alice";
        private static string User2 = "Bob";

        private static void Main(string[] args)
        {
            var config = new BusConfiguration();
            config.UsePersistence<InMemoryPersistence>();

            var bus = Bus.CreateSendOnly(config);

            var sb = new StringBuilder();
            sb.AppendLine("1 - Send ItemAddedToCart for Alice");
            sb.AppendLine("2 - Send OrderSubmitted for Alice");
            sb.AppendLine("3 - Send ItemAddedToCart for Bob");
            sb.AppendLine("4 - Send OrderSubmitted for Bob");
            sb.AppendLine("Any other key to quit");

            Console.WriteLine(sb.ToString());


            var x = Console.ReadKey();
            do {
                switch (x.KeyChar) {
                    case '1':
                        bus.Send("Server", new ItemAddedToCart {
                            UserName = User1
                        });
                        break;
                    case '2':
                        bus.Send("Server", new OrderSubmitted {
                            UserName = User1
                        });
                        break;
                    case '3':
                        bus.Send("Server", new ItemAddedToCart {
                            UserName = User2
                        });
                        break;
                    case '4':
                        bus.Send("Server", new OrderSubmitted {
                            UserName = User2
                        });
                        break;
                }

                x = Console.ReadKey();
            } while (ValidKeys.Contains(x.KeyChar));
        }
    }
}
