# OrderAPI Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SportsStore.OrderAPI/SportsStore.OrderAPI.csproj", "SportsStore.OrderAPI/"]
COPY ["SportsStore.Shared/SportsStore.Shared.csproj", "SportsStore.Shared/"]
RUN dotnet restore "SportsStore.OrderAPI/SportsStore.OrderAPI.csproj"
COPY . .
WORKDIR "/src/SportsStore.OrderAPI"
RUN dotnet build "SportsStore.OrderAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SportsStore.OrderAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "SportsStore.OrderAPI.dll"]
