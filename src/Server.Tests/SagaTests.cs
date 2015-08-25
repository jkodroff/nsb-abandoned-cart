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
            var userName = "sales@joshkodroff.com";
            var timeoutId1 = Guid.Empty;

            Test
                .Saga<AbandonedCartSaga>()
                .ExpectTimeoutToBeSetIn<AbandonedCartTimeout>((msg, span) => {
                    timeoutId1 = msg.Id;
                    span.Should().Be(TimeSpan.FromSeconds(5));
                })
                .When(saga => saga.Handle(new ItemAddedToCart {
                    UserName = userName
                }))
                .ExpectTimeoutToBeSetIn<AbandonedCartTimeout>((msg, span) => {
                    msg.Id.Should().NotBe(timeoutId1); // just to make sure we're generating new ids
                    span.Should().Be(TimeSpan.FromSeconds(5));
                })
                .When(saga => saga.Handle(new ItemAddedToCart {
                    UserName = userName
                }))
                .ExpectNotSend<SendAbandonedCartEmail>(x => x != null) // just a dummy check.  no message should be sent
                .When(saga => {
                    // the first timeout comes back (which should be ignored):
                    saga.Timeout(new AbandonedCartTimeout {
                        Id = timeoutId1
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
            var userName = "sales@joshkodroff.com";
            var timeoutId1 = Guid.Empty;
            var timeoutId2 = Guid.Empty;

            Test
                .Saga<AbandonedCartSaga>()
                .ExpectTimeoutToBeSetIn<AbandonedCartTimeout>((msg, span) => {
                    timeoutId1 = msg.Id;
                    span.Should().Be(TimeSpan.FromSeconds(5));
                })
                .When(saga => saga.Handle(new ItemAddedToCart {
                    UserName = userName
                }))
                .ExpectTimeoutToBeSetIn<AbandonedCartTimeout>((msg, span) => {
                    timeoutId2 = msg.Id;
                    span.Should().Be(TimeSpan.FromSeconds(5));
                })
                .When(saga => {
                    saga.Handle(new ItemAddedToCart {
                        UserName = userName
                    });

                    // the first timeout comes back (which should be ignored):
                    saga.Timeout(new AbandonedCartTimeout {
                        Id = timeoutId1
                    });
                })
                .ExpectSendLocal<SendAbandonedCartEmail>(msg => {
                    msg.UserName.Should().Be(userName);
                })
                .When(saga => saga.Timeout(new AbandonedCartTimeout {
                    Id = timeoutId2
                }))
                .AssertSagaCompletionIs(true);
        }
    }
}
