FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS publish
WORKDIR /src
COPY . .
RUN dotnet publish "./Bender/Bender.csproj" -c Release -o /app/publish \
  --runtime alpine-x64 \
  --self-contained true \
  /p:PublishTrimmed=true \
  /p:PublishSingleFile=true

# ---------------------------------

FROM curlimages/curl AS get-rootless-cron
WORKDIR /tmp
RUN curl -fsSLO https://github.com/aptible/supercronic/releases/download/v0.1.12/supercronic-linux-amd64 \
 && echo "048b95b48b708983effb2e5c935a1ef8483d9e3e  supercronic-linux-amd64" | sha1sum -c -

# ---------------------------------

FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS final
LABEL org.opencontainers.image.source=https://github.com/ValentinLevitov/bender
COPY --from=get-rootless-cron /tmp/supercronic-linux-amd64 /usr/local/bin/supercronic
RUN chmod +x /usr/local/bin/supercronic
WORKDIR /app
ADD bender-cron .
COPY --from=publish /app/publish/ .
CMD supercronic -passthrough-logs /app/bender-cron