FROM openjdk:8-jdk
WORKDIR /
EXPOSE 9092

RUN wget -O kafka-bin.tgz https://downloads.apache.org/kafka/3.4.1/kafka_2.13-3.4.1.tgz &&\
	tar -xzf kafka-bin.tgz &&\
	mv kafka_2.13-3.4.1 kafka &&\
	rm kafka-bin.tgz

WORKDIR /kafka

CMD ./scripts/start.sh

