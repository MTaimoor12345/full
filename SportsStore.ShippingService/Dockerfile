# ShippingService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SportsStore.ShippingService/SportsStore.ShippingService.csproj", "SportsStore.ShippingService/"]
COPY ["SportsStore.Shared/SportsStore.Shared.csproj", "SportsStore.Shared/"]
RUN dotnet restore "SportsStore.ShippingService/SportsStore.ShippingService.csproj"
COPY . .
WORKDIR "/src/SportsStore.ShippingService"
RUN dotnet build "SportsStore.ShippingService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SportsStore.ShippingService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5003
ENTRYPOINT ["dotnet", "SportsStore.ShippingService.dll"]
