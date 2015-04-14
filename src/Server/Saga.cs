using System;
using Messages;
using NServiceBus;
using NServiceBus.Saga;

namespace Server
{
    public class AbandonedCartSaga :
        Saga<AbandonedCartSagaData>,
        IAmStartedByMessages<ItemAddedToCart>,
        IHandleMessages<OrderSubmitted>,
        IHandleTimeouts<AbandonedCartTimeout>
    {
        public void Handle(ItemAddedToCart message)
        {
            Console.WriteLine("Received ItemAddedToCart for " + message.UserName);

            Data.UserName = message.UserName;
            Data.ItemCount += 1;

            RequestTimeout(
                TimeSpan.FromSeconds(5),
                new AbandonedCartTimeout {
                    ItemCount = Data.ItemCount
                }
            );
        }

        public void Handle(OrderSubmitted message)
        {
            Console.WriteLine("Received OrderSubmitted for " + message.UserName);

            // If the order is submitted, we can cancel this saga.
            MarkAsComplete();
        }

        public void Timeout(AbandonedCartTimeout state)
        {
            if (Data.ItemCount != state.ItemCount) {
                // This is not the last timeout issued, so ignore it.
                return;
            }

            Console.WriteLine("Timeout reached for: " + Data.UserName);

            Bus.SendLocal(new SendAbandonedCartEmail {
                UserName = Data.UserName
            });

            MarkAsComplete();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<AbandonedCartSagaData> mapper)
        {
            mapper
                .ConfigureMapping<ItemAddedToCart>(x => x.UserName)
                .ToSaga(x => x.UserName);

            mapper
                .ConfigureMapping<OrderSubmitted>(x => x.UserName)
                .ToSaga(x => x.UserName);
        }
    }

    public class AbandonedCartTimeout
    {
        public int ItemCount { get; set; }
    }

    public class AbandonedCartSagaData : IContainSagaData
    {
        // Built-in saga properties:
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }

        // Our properties, specific to this saga:
        
        // This needs to be unique so that we can scale out, or 
        // we might get 2 Sagas with the same username, 
        // per https://twitter.com/UdiDahan/status/587896128688951297
        [Unique]
        public string UserName { get; set; }
        
        public int ItemCount { get; set; }
    }
}
