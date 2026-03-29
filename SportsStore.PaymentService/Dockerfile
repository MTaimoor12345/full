# PaymentService Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5002

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SportsStore.PaymentService/SportsStore.PaymentService.csproj", "SportsStore.PaymentService/"]
COPY ["SportsStore.Shared/SportsStore.Shared.csproj", "SportsStore.Shared/"]
RUN dotnet restore "SportsStore.PaymentService/SportsStore.PaymentService.csproj"
COPY . .
WORKDIR "/src/SportsStore.PaymentService"
RUN dotnet build "SportsStore.PaymentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SportsStore.PaymentService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5002
ENTRYPOINT ["dotnet", "SportsStore.PaymentService.dll"]
