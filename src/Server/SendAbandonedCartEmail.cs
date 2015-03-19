using System;
using NServiceBus;

namespace Server
{
    public class SendAbandonedCartEmail : ICommand
    {
        public string UserName { get; set; }
    }

    public class SendAbandonedCartEmailHandler : IHandleMessages<SendAbandonedCartEmail>
    {
        public void Handle(SendAbandonedCartEmail message)
        {
            Console.WriteLine("Sending abandoned cart email for: " + message.UserName);
        }
    }
}
