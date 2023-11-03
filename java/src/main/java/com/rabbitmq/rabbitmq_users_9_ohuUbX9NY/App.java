package com.rabbitmq.rabbitmq_users_9_ohuUbX9NY;

import com.rabbitmq.client.Channel;
import com.rabbitmq.client.Connection;
import com.rabbitmq.client.ConnectionFactory;
import com.rabbitmq.client.DeliverCallback;
import java.nio.charset.StandardCharsets;
import java.time.LocalDateTime;
import java.time.Duration;

public class App 
{
    private final static String QUEUE_NAME = "rabbitmq-users-9_ohuUbX9NY";
	private static int received;
	private static LocalDateTime previousDateTime;

    public static void main(String[] argv) throws Exception {
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(System.getenv("RMQ_HOST"));
		factory.setUsername("guest");
        factory.setPassword("guest");
        factory.setVirtualHost("/");
        Connection connection = factory.newConnection();
        Channel channel = connection.createChannel();

        System.out.println(" [*] Waiting for messages. To exit press CTRL+C");

		received = 0;
		previousDateTime = LocalDateTime.now();

        DeliverCallback deliverCallback = (consumerTag, delivery) -> {

			received++;

			LocalDateTime currentTime = LocalDateTime.now();

            String message = new String(delivery.getBody(), StandardCharsets.UTF_8);
			
			Duration duration = Duration.between(currentTime, previousDateTime);
			long diff = Math.abs(duration.toSeconds());
			if (diff > 1)
			{
				System.out.println("Messages / sec: " + received);
				previousDateTime = currentTime;
				received = 0;
			}
        };

        channel.basicConsume(QUEUE_NAME, true, deliverCallback, consumerTag -> { });
    }
}
