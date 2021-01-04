#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Annaki.Web/Annaki.Web.csproj", "Annaki.Web/"]
COPY ["Annaki/Annaki.csproj", "Annaki/"]
COPY ["SplatnetMonitor/SplatNet2.Net.Monitor.csproj", "SplatnetMonitor/"]
COPY ["SplatNet2.Net.Api/SplatNet2.Net.Api.csproj", "SplatNet2.Net.Api/"]
RUN dotnet restore "Annaki.Web/Annaki.Web.csproj"
COPY . .
WORKDIR "/src/Annaki.Web"
RUN dotnet build "Annaki.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Annaki.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Annaki.Web.dll"]