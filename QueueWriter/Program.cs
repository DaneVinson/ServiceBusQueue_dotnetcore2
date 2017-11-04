using Domain.Model;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QueueWriter
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {
                Initialize();
                WriteToQueue(10).GetAwaiter().GetResult();
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

        private static void Initialize()
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

            QueueClient = new QueueClient(ServiceBusConnectionString, QueueName, QueueReceiveMode);
        }

        private static async Task WriteToQueue(int count)
        {
            var tasks = new Task[count];
            for (int i = 0; i < count; i++)
            {
                //var job = new ScheduledTask() { Description = $"Job{i}", ShardId = 2, TaskId = Guid.NewGuid().ToString() };
                var job = new JobStartOptions() { EndpointAddress = "http://charactersservice.azurewebsites.net/processorservice.svc", JobId = Guid.NewGuid().ToString(), ShardId = i };
                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(job)));
                Console.WriteLine($"Writing message {i}");
                tasks[i] = QueueClient.SendAsync(message);
            }
            await Task.WhenAll();
            Console.Write("All writes complete.");
        }

        private static IConfigurationRoot Configuration { get; set; }

        private static QueueClient QueueClient { get; set; }

        private static string QueueName { get; set; }

        private static ReceiveMode QueueReceiveMode { get; set; }

        private static string ServiceBusConnectionString { get; set; }
    }
}
