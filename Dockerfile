# Sử dụng Runtime .NET 8.0 để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Sử dụng SDK để build source code
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["JTC Bot.csproj", "./"]
RUN dotnet restore "JTC Bot.csproj"
COPY . .
RUN dotnet build "JTC Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JTC Bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JTC Bot.dll"]