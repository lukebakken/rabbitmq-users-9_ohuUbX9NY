using System.Net;
using System.Net.Sockets;
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

factory.SocketFactory = (AddressFamily af) =>
{
    var socket = new Socket(af, SocketType.Stream, ProtocolType.Tcp)
    {
        NoDelay = true,
    };
    return new LocalTcpClientAdapter(socket);
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

class LocalTcpClientAdapter : ITcpClient
{
    private Socket _sock;

    public LocalTcpClientAdapter(Socket socket)
    {
        _sock = socket ?? throw new InvalidOperationException("socket must not be null");
    }

    public virtual Task ConnectAsync(string host, int port)
    {
        AssertSocket();
        return _sock.ConnectAsync(host, port);
    }

    public virtual Task ConnectAsync(IPAddress ep, int port)
    {
        AssertSocket();
        return _sock.ConnectAsync(ep, port);
    }

    public virtual void Close()
    {
        _sock.Dispose();
    }

    public virtual void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Close();
        }
    }

    public virtual NetworkStream GetStream()
    {
        AssertSocket();
        return new NetworkStream(_sock);
    }

    public virtual Socket Client
    {
        get
        {
            return _sock;
        }
    }

    public virtual bool Connected
    {
        get
        {
            if (_sock is null)
            {
                return false;
            }
            return _sock.Connected;
        }
    }

    public virtual TimeSpan ReceiveTimeout
    {
        get
        {
            AssertSocket();
            return TimeSpan.FromMilliseconds(_sock.ReceiveTimeout);
        }
        set
        {
            AssertSocket();
            _sock.ReceiveTimeout = (int)value.TotalMilliseconds;
        }
    }

    private void AssertSocket()
    {
        if (_sock is null)
        {
            throw new InvalidOperationException("Cannot perform operation as socket is null");
        }
    }
}
