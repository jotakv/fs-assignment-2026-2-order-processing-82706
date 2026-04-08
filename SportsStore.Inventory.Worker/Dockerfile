FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SportsStore.Inventory.Worker/SportsStore.Inventory.Worker.csproj", "SportsStore.Inventory.Worker/"]
COPY ["SportsStore.Application/SportsStore.Application.csproj", "SportsStore.Application/"]
COPY ["SportsStore.Infrastructure/SportsStore.Infrastructure.csproj", "SportsStore.Infrastructure/"]
COPY ["SportsStore.Domain/SportsStore.Domain.csproj", "SportsStore.Domain/"]
RUN dotnet restore "SportsStore.Inventory.Worker/SportsStore.Inventory.Worker.csproj"

COPY . .
WORKDIR /src/SportsStore.Inventory.Worker
RUN dotnet publish "SportsStore.Inventory.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SportsStore.Inventory.Worker.dll"]
