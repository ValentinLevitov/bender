using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Threading;
using Messaging;
using Moq;
using NUnit.Framework;
using SmtpClient = Messaging.SmtpClient;

namespace Tests.MailSending
{
    [TestFixture]
    public class MailSendingTests
    {
        private Message[] Messages { get; }

        public MailSendingTests()
        {
            var m1 = new Message { Subject = "m1" };
            var m2 = new Message { Subject = "m2" };
            var m3 = new Message { Subject = "m3" };

            Messages = new[] { m1, m2, m3 };

        }

        [Test]
        public void TestEachMessageIsSent()
        {
            var messenger = new Mock<IMessenger>();

            SmtpClient.Send(messenger.Object, Messages, TimeSpan.FromSeconds(3), 2);

            messenger.Verify(m => m.Send(It.Is<Message>(s => s.Subject == "m1")), Times.Once);
            messenger.Verify(m => m.Send(It.Is<Message>(s => s.Subject == "m2")), Times.Once);
            messenger.Verify(m => m.Send(It.Is<Message>(s => s.Subject == "m3")), Times.Once);
        }

        [Test]
        public void TestRetryWorks()
        {
            var count = 0;
            var messenger = new Mock<IMessenger>();
            messenger.Setup(a => a.Send(It.IsAny<Message>()))
                
                .Callback(() =>
                {
                    Debug.WriteLine(Thread.CurrentThread.ManagedThreadId);
                    count++;
                    if (count == 2)
                        throw new SmtpException();
                });

            SmtpClient.Send(messenger.Object, Messages, TimeSpan.FromSeconds(1), 3);

            messenger.Verify(m => m.Send(It.IsAny<Message>()), Times.Exactly(4));

            messenger.Verify(m => m.Send(It.Is<Message>(s => s.Subject == "m1")), Times.AtLeastOnce());
            messenger.Verify(m => m.Send(It.Is<Message>(s => s.Subject == "m2")), Times.AtLeastOnce());
            messenger.Verify(m => m.Send(It.Is<Message>(s => s.Subject == "m3")), Times.AtLeastOnce());

        }

    }
}