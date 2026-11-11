
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY src/DmsQueueMonitor.Worker/DmsQueueMonitor.Worker.csproj ./Worker/

RUN dotnet restore ./Worker/DmsQueueMonitor.Worker.csproj

COPY src/DmsQueueMonitor.Worker/ ./Worker/

RUN dotnet publish ./Worker/DmsQueueMonitor.Worker.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app/data

ENV DOTNET_ENVIRONMENT=Production
ENV TZ=Europe/Kyiv

USER $APP_UID

ENTRYPOINT ["dotnet", "DmsQueueMonitor.Worker.dll"]
