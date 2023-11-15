using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ThreadPool.SetMinThreads(16 * Environment.ProcessorCount, 16 * Environment.ProcessorCount);

var factory = new ConnectionFactory
{
	HostName = Environment.GetEnvironmentVariable("RMQ_HOST"),
	UserName = "guest",
	Password = "guest",
	VirtualHost = "/",
	DispatchConsumersAsync = true,
};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

Console.WriteLine(" [*] Waiting for messages.");

var previousDateTime = DateTime.Now;
var received = 0;

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.Received += ConsumerOnReceived;

channel.BasicConsume(queue: "rabbitmq-users-9_ohuUbX9NY", autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

Task ConsumerOnReceived(object sender, BasicDeliverEventArgs @event)
{
	received++;

	var currentTime = DateTime.Now;
	if (currentTime - previousDateTime > TimeSpan.FromSeconds(1))
	{
		Console.WriteLine($"Messages / sec: {received}");
		previousDateTime = currentTime;
		received = 0;
	}

	return Task.CompletedTask;
}
