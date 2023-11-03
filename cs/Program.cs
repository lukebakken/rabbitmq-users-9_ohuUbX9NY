using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory
{
	HostName = Environment.GetEnvironmentVariable("RMQ_HOST"),
	UserName = "guest",
	Password = "guest",
	VirtualHost = "/"
};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

Console.WriteLine(" [*] Waiting for messages.");

var previousDateTime = DateTime.Now;
var received = 0;

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
	received++;

	var currentTime = DateTime.Now;
	if (currentTime - previousDateTime > TimeSpan.FromSeconds(1))
	{
		Console.WriteLine($"Messages / sec: {received}");
		previousDateTime = currentTime;
		received = 0;
	}
};

channel.BasicConsume(queue: "rabbitmq-users-9_ohuUbX9NY", autoAck: true, consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
