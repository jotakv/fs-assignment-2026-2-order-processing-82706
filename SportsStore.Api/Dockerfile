FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SportsStore.Api/SportsStore.Api.csproj", "SportsStore.Api/"]
COPY ["SportsStore.Application/SportsStore.Application.csproj", "SportsStore.Application/"]
COPY ["SportsStore.Infrastructure/SportsStore.Infrastructure.csproj", "SportsStore.Infrastructure/"]
COPY ["SportsStore.Domain/SportsStore.Domain.csproj", "SportsStore.Domain/"]
RUN dotnet restore "SportsStore.Api/SportsStore.Api.csproj"

COPY . .
WORKDIR /src/SportsStore.Api
RUN dotnet publish "SportsStore.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SportsStore.Api.dll"]
