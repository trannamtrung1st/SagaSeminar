FROM mcr.microsoft.com/dotnet/aspnet:6.0.18-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.410-bullseye-slim AS build
WORKDIR /src
COPY ["SagaSeminar.Services.OrderService/SagaSeminar.Services.OrderService.csproj", "SagaSeminar.Services.OrderService/"]
RUN dotnet restore "SagaSeminar.Services.OrderService/SagaSeminar.Services.OrderService.csproj"
COPY . .
WORKDIR "/src/SagaSeminar.Services.OrderService"
RUN dotnet build "SagaSeminar.Services.OrderService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SagaSeminar.Services.OrderService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

CMD sleep 50s; dotnet SagaSeminar.Services.OrderService.dll