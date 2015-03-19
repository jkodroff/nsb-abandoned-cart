using NServiceBus;

namespace Messages
{
    // This is just a message because it's presumably published
    // from a website and calling Bus.Publish() from a website
    // just ain't cool, man:
    // http://www.make-awesome.com/2010/10/why-not-publish-nservicebus-messages-from-a-web-application/
    public class ItemAddedToCart : IMessage
    {
        public string UserName { get; set; }
    }

    public class OrderSubmitted : IMessage
    {
        public string UserName { get; set; }
    }
}