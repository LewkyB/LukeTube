FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["HackerNewsWorker/HackerNewsWorker.csproj", "HackerNewsWorker/"]
RUN dotnet restore "HackerNewsWorker/HackerNewsWorker.csproj"
COPY . .
WORKDIR "/src/HackerNewsWorker"
RUN dotnet build "HackerNewsWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HackerNewsWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HackerNewsWorker.dll"]
