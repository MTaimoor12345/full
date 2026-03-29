FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SportsStore.Blazor/SportsStore.Blazor.csproj", "SportsStore.Blazor/"]
COPY ["SportsStore.Shared/SportsStore.Shared.csproj", "SportsStore.Shared/"]
RUN dotnet restore "SportsStore.Blazor/SportsStore.Blazor.csproj"
COPY . .
WORKDIR "/src/SportsStore.Blazor"
RUN dotnet build "SportsStore.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SportsStore.Blazor.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY SportsStore.Blazor/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
