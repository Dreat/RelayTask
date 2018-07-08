using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RelayTask.Abstract;
using RelayTask.Helpers;

namespace RelayTask.ConsoleApp
{
    class Program
    {
        public static event EventHandler<SystemEventArgs> SystemEventRaised;

        static void Main(string[] args)
        {
            var states = new List<bool>();
            var deadLetterQueue = new DeadLetterQueue();
            var invalidLetterQueue = new InvalidLetterQueue();
            var relay = new Relay(deadLetterQueue, invalidLetterQueue);
            var publisher = new Publisher(relay);
            SystemEventRaised += publisher.SystemMessageEmitted;

            // To check state changes - if backpressure was issued we will have "true" in that list
            relay.BackPressureNeeded += (sender, e) => states.Add(e);

            var subscriber = new Subscriber();
            var remoteService = new RemoteService();
            var complexSubscriber = new ComplexSubscriber();
            var failingSubscriber = new FailingSubscriber();

            relay.RegisterRemoteServices(new List<IRemoteService>
            {
                remoteService,
                complexSubscriber,
                failingSubscriber
            });
            relay.RegisterSubscribers(new List<ISubscriber> {subscriber, complexSubscriber, failingSubscriber});

            publisher.Run();

            // 200 is enough to have backpressure on
            foreach (var systemEvent in GenerateSystemEvents(200))
            {
                // To be honest I have no idea how to Invoke in static context
                // I'm not using envoker anyway
                SystemEventRaised?.Invoke(new object(), systemEvent);
            }

            StartReceiversPrinter(new IPrinter[]
            {
                deadLetterQueue,
                invalidLetterQueue,
                subscriber,
                remoteService,
                complexSubscriber
            });

            Console.ReadKey();
        }

        private static void StartReceiversPrinter(IPrinter[] printers)
        {
            new TaskFactory().StartNew(() =>
            {
                while (true)
                {
                    foreach (var printer in printers)
                    {
                        printer.Print();
                        Thread.Sleep(500);
                    }
                }
            });
        }

        public static IEnumerable<SystemEventArgs> GenerateSystemEvents(int numberOfEvents)
        {
            var result = new List<SystemEventArgs>();
            for (var i = 0; i < numberOfEvents; i++)
            {
                var messageType = (MessageType) (i % 2);
                if (i % 10 == 0)
                {
                    messageType = MessageType.NotHandled;
                }

                result.Add(new SystemEventArgs
                {
                    Command = $"Command number: {i}",
                    MessageType = messageType
                });
            }

            return result;
        }
    }
}