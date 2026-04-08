FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SportsStore.Blazor/SportsStore.Blazor.csproj", "SportsStore.Blazor/"]
COPY ["SportsStore.Application/SportsStore.Application.csproj", "SportsStore.Application/"]
COPY ["SportsStore.Domain/SportsStore.Domain.csproj", "SportsStore.Domain/"]
RUN dotnet restore "SportsStore.Blazor/SportsStore.Blazor.csproj"

COPY . .
WORKDIR /src/SportsStore.Blazor
RUN dotnet publish "SportsStore.Blazor.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SportsStore.Blazor.dll"]
