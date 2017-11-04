using Domain.Model;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueueReader
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                InitializeConfiguration();
                InitializeQueueReader();
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} - {1}", ex.GetType(), ex.Message);
                Console.WriteLine(ex.StackTrace ?? String.Empty);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("...");
                Console.ReadKey();
            }
        }

        private static void InitializeConfiguration()
        {
            Configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("config.json", false, true)
                                .Build();
            QueueName = Configuration.GetSection("ServiceBus:QueueName")?.Value;
            ServiceBusConnectionString = Configuration.GetSection("ServiceBus:ConnectionString")?.Value;
            ReceiveMode receiveMode;
            if (Enum.TryParse<ReceiveMode>(Configuration.GetSection("ServiceBus:ReceiveMode")?.Value, out receiveMode)) { QueueReceiveMode = receiveMode; }
            else { QueueReceiveMode = ReceiveMode.PeekLock; }
            int maxReads = 0;
            if (Int32.TryParse(Configuration.GetSection("ServiceBus:MaxConcurrentReads")?.Value, out maxReads)) { MaxConcurrentReads = maxReads; }
            else { MaxConcurrentReads = 1; }
        }

        private static void InitializeQueueReader()
        {
            var messageHandlerOptions = new MessageHandlerOptions(HandleReceivedException)
            {
                AutoComplete = false,
                MaxConcurrentCalls = MaxConcurrentReads
            };
            QueueClient = new QueueClient(ServiceBusConnectionString, QueueName, QueueReceiveMode);
            QueueClient.RegisterMessageHandler(HandleMessage, messageHandlerOptions);
        }

        private static async Task HandleMessage(Message message, CancellationToken cancellationToken)
        {
            var job = JsonConvert.DeserializeObject<ScheduledTask>(Encoding.UTF8.GetString(message.Body));
            Console.WriteLine(job);

            // For PeekLock complete the message.
            if (QueueReceiveMode == ReceiveMode.PeekLock) { await QueueClient.CompleteAsync(message.SystemProperties.LockToken); }
        }

        private static async Task HandleReceivedException(ExceptionReceivedEventArgs args)
        {
            Console.WriteLine($"{args.Exception.GetType()} duing recevie: {args.Exception.Message}");
        }


        private static IConfigurationRoot Configuration { get; set; }

        private static int MaxConcurrentReads { get; set; }

        private static QueueClient QueueClient { get; set; }

        private static string QueueName { get; set; }

        private static ReceiveMode QueueReceiveMode { get; set; }

        private static string ServiceBusConnectionString { get; set; }
    }
}
