ARG RESOURCE_REAPER_SESSION_ID="00000000-0000-0000-0000-000000000000"

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LukeTube/LukeTube.csproj", "LukeTube/"]
COPY ["LukeTubeLib/LukeTubeLib.csproj", "LukeTubeLib/"]
RUN dotnet restore "LukeTube/LukeTube.csproj"
COPY . .
WORKDIR "/src/LukeTube"
RUN dotnet build "LukeTube.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LukeTube.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LukeTube.dll"]