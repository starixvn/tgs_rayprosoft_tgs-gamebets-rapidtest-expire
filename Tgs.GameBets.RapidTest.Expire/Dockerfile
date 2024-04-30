#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["nuget.config", "."]
COPY ["Tgs.GameBets.RapidTest.Expire/Tgs.GameBets.RapidTest.Expire.csproj", "Tgs.GameBets.RapidTest.Expire/"]
COPY ["Tgs.GameBets.RapidTest.Expire.Common/Tgs.GameBets.RapidTest.Expire.Common.csproj", "Tgs.GameBets.RapidTest.Expire.Common/"]
COPY ["Tgs.GameBets.RapidTest.Expire.Core/Tgs.GameBets.RapidTest.Expire.Core.csproj", "Tgs.GameBets.RapidTest.Expire.Core/"]
COPY ["Tgs.GameBets.RapidTest.Expire.Repositories/Tgs.GameBets.RapidTest.Expire.Repositories.csproj", "Tgs.GameBets.RapidTest.Expire.Repositories/"]
RUN dotnet restore "./Tgs.GameBets.RapidTest.Expire/./Tgs.GameBets.RapidTest.Expire.csproj"
COPY . .
WORKDIR "/src/Tgs.GameBets.RapidTest.Expire"
RUN dotnet build "./Tgs.GameBets.RapidTest.Expire.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tgs.GameBets.RapidTest.Expire.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tgs.GameBets.RapidTest.Expire.dll"]