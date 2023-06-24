FROM mcr.microsoft.com/dotnet/aspnet:6.0.18-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.410-bullseye-slim AS build
WORKDIR /src
COPY ["SagaSeminar.Services.ShippingService/SagaSeminar.Services.ShippingService.csproj", "SagaSeminar.Services.ShippingService/"]
RUN dotnet restore "SagaSeminar.Services.ShippingService/SagaSeminar.Services.ShippingService.csproj"
COPY . .
WORKDIR "/src/SagaSeminar.Services.ShippingService"
RUN dotnet build "SagaSeminar.Services.ShippingService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SagaSeminar.Services.ShippingService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

CMD sleep 50s; dotnet SagaSeminar.Services.ShippingService.dll