FROM mcr.microsoft.com/dotnet/runtime:5.0

LABEL org.opencontainers.image.source=https://github.com/ValentinLevitov/bender

# Add crontab file in the cron directory
ADD bender-cron /app/

# Create the log file to be able to run tail
RUN touch /var/log/cron.log

#Install Cron
RUN apt-get update
RUN apt-get -y install cron

RUN chmod +x /usr/bin/crontab\
	&& chmod 775 -R /var/spool/cron\
	&& chmod 777 /var/run -R
	
COPY bin/Release/net5.0/publish/ app/
WORKDIR /app

RUN crontab /app/bender-cron

CMD cron && tail -f /var/log/cron.log