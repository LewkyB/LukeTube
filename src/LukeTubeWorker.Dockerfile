ARG RESOURCE_REAPER_SESSION_ID="00000000-0000-0000-0000-000000000002"
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LukeTubeWorkerService/LukeTubeWorkerService.csproj", "LukeTubeWorkerService/"]
COPY ["LukeTubeLib/LukeTubeLib.csproj", "LukeTubeLib/"]
COPY ["YoutubeExplode/YoutubeExplode.csproj", "YoutubeExplode/"]
RUN dotnet restore "LukeTubeWorkerService/LukeTubeWorkerService.csproj"
COPY . .
WORKDIR "/src/LukeTubeWorkerService"
RUN dotnet build "LukeTubeWorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LukeTubeWorkerService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LukeTubeWorkerService.dll"]