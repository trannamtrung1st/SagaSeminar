FROM mcr.microsoft.com/dotnet/aspnet:6.0.18-bullseye-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.410-bullseye-slim AS build
WORKDIR /src
COPY ["SagaSeminar/SagaSeminar.csproj", "SagaSeminar/"]
RUN dotnet restore "SagaSeminar/SagaSeminar.csproj"
COPY . .
WORKDIR "/src/SagaSeminar"
RUN dotnet build "SagaSeminar.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SagaSeminar.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

CMD sleep 40s; dotnet SagaSeminar.dll