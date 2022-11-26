FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LukeTube.Benchmark/LukeTube.Benchmark.csproj", "LukeTube.Benchmark/"]
RUN dotnet restore "LukeTube.Benchmark/LukeTube.Benchmark.csproj"
COPY . .
WORKDIR "/src/LukeTube.Benchmark"
RUN dotnet build "LukeTube.Benchmark.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LukeTube.Benchmark.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LukeTube.Benchmark.dll"]
