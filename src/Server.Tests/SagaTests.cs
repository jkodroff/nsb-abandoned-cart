using System;
using FluentAssertions;
using Messages;
using NServiceBus.Testing;
using NUnit.Framework;

namespace Server.Tests
{
    [TestFixture]
    public class SagaTests
    {
        [Test]
        public void OrderSubmitted()
        {
            var userName = "sales@weblinc.com";

            Test
                .Saga<AbandonedCartSaga>()
                .ExpectTimeoutToBeSetIn<AbandonedCartTimeout>((msg, span) => {
                    msg.ItemCount.Should().Be(1);
                    span.Should().Be(TimeSpan.FromSeconds(5));
                })
                .When(saga => saga.Handle(new ItemAddedToCart {
                    UserName = userName
                }))
                .ExpectTimeoutToBeSetIn<AbandonedCartTimeout>((msg, span) => {
                    msg.ItemCount.Should().Be(2);
                    span.Should().Be(TimeSpan.FromSeconds(5));
                })
                .When(saga => saga.Handle(new ItemAddedToCart {
                    UserName = userName
                }))
                .ExpectNotSend<SendAbandonedCartEmail>(x => x != null) // just a dummy check.  no message should be sent
                .When(saga => {
                    // the first timeout comes back (which should be ignored):
                    saga.Timeout(new AbandonedCartTimeout {
                        ItemCount = 1
                    });

                    saga.Handle(new OrderSubmitted {
                        UserName = userName
                    });
                })
                .AssertSagaCompletionIs(true);
        }

        [Test]
        public void OrderNotSubmitted()
        {
            var userName = "sales@weblinc.com";

            Test
                .Saga<AbandonedCartSaga>()
                .ExpectTimeoutToBeSetIn<AbandonedCartTimeout>((msg, span) => {
                    msg.ItemCount.Should().Be(1);
                    span.Should().Be(TimeSpan.FromSeconds(5));
                })
                .When(saga => saga.Handle(new ItemAddedToCart {
                    UserName = userName
                }))
                .ExpectTimeoutToBeSetIn<AbandonedCartTimeout>((msg, span) => {
                    msg.ItemCount.Should().Be(2);
                    span.Should().Be(TimeSpan.FromSeconds(5));
                })
                .When(saga => {
                    saga.Handle(new ItemAddedToCart {
                        UserName = userName
                    });

                    // the first timeout comes back (which should be ignored):
                    saga.Timeout(new AbandonedCartTimeout {
                        ItemCount = 1
                    });
                })
                .ExpectSendLocal<SendAbandonedCartEmail>(msg => {
                    msg.UserName.Should().Be(userName);
                })
                .When(saga => saga.Timeout(new AbandonedCartTimeout {
                    ItemCount = 2
                }))
                .AssertSagaCompletionIs(true);
        }
    }
}
