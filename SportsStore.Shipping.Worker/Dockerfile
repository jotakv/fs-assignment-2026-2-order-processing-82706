FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SportsStore.Shipping.Worker/SportsStore.Shipping.Worker.csproj", "SportsStore.Shipping.Worker/"]
COPY ["SportsStore.Application/SportsStore.Application.csproj", "SportsStore.Application/"]
COPY ["SportsStore.Infrastructure/SportsStore.Infrastructure.csproj", "SportsStore.Infrastructure/"]
COPY ["SportsStore.Domain/SportsStore.Domain.csproj", "SportsStore.Domain/"]
RUN dotnet restore "SportsStore.Shipping.Worker/SportsStore.Shipping.Worker.csproj"

COPY . .
WORKDIR /src/SportsStore.Shipping.Worker
RUN dotnet publish "SportsStore.Shipping.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SportsStore.Shipping.Worker.dll"]
