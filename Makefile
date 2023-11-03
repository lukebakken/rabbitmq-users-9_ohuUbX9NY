.PHONY: build publish run-java run-cs

RMQ_HOST ?= shostakovich

build:
	cd cs && dotnet build
	cd java && mvn compile

run-cs:
	cd cs && dotnet run

run-java:
	cd java && mvn exec:java '-Dexec.mainClass=com.rabbitmq.rabbitmq_users_9_ohuUbX9NY.App'

publish:
	cd rabbitmq-perf-test && make ARGS="--uri amqp://$(RMQ_HOST) --predeclared --queue rabbitmq-users-9_ohuUbX9NY --pmessages 655360 --producers 1 --consumers 0 --confirm 16 --size 4096" run
