FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["YoutubeWorkerService/YoutubeWorkerService.csproj", "YoutubeWorkerService/"]
COPY ["LukeTubeLib/LukeTubeLib.csproj", "LukeTubeLib/"]
RUN dotnet restore "YoutubeWorkerService/YoutubeWorkerService.csproj"
COPY . .
WORKDIR "/src/YoutubeWorkerService"
RUN dotnet build "YoutubeWorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YoutubeWorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YoutubeWorkerService.dll"]
