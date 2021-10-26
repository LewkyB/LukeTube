#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["luke_site_mvc.csproj", "."]
RUN dotnet restore "luke_site_mvc.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "luke_site_mvc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "luke_site_mvc.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "luke_site_mvc.dll"]
