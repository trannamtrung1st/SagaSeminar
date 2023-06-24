FROM mcr.microsoft.com/dotnet/sdk:6.0.410-bullseye-slim AS build

WORKDIR /src
COPY ["SagaSeminar.Clients.WebClient/SagaSeminar.Clients.WebClient.csproj", "SagaSeminar.Clients.WebClient/"]
RUN dotnet restore "SagaSeminar.Clients.WebClient/SagaSeminar.Clients.WebClient.csproj"
COPY . .
WORKDIR "/src/SagaSeminar.Clients.WebClient"
RUN dotnet build "SagaSeminar.Clients.WebClient.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SagaSeminar.Clients.WebClient.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY ../External/Nginx/nginx.conf /etc/nginx/nginx.conf
COPY ../External/Docker/WebClient/appsettings.json ./appsettings.json
