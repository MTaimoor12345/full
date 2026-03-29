# InventoryService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SportsStore.InventoryService/SportsStore.InventoryService.csproj", "SportsStore.InventoryService/"]
COPY ["SportsStore.Shared/SportsStore.Shared.csproj", "SportsStore.Shared/"]
RUN dotnet restore "SportsStore.InventoryService/SportsStore.InventoryService.csproj"
COPY . .
WORKDIR "/src/SportsStore.InventoryService"
RUN dotnet build "SportsStore.InventoryService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SportsStore.InventoryService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "SportsStore.InventoryService.dll"]
