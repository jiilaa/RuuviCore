#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["RuuviCore/RuuviCore.csproj", "RuuviCore/"]
COPY ["FileStorageProvider/FileStorageProvider.csproj", "FileStorageProvider/"]
COPY ["GrainInterfaces/GrainInterfaces.csproj", "GrainInterfaces/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["Grains/Grains.csproj", "Grains/"]
COPY ["GrainServices/GrainServices.csproj", "GrainServices/"]
RUN dotnet restore "RuuviCore/RuuviCore.csproj"
COPY . .
WORKDIR "/src/RuuviCore"
RUN dotnet build "RuuviCore.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RuuviCore.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ruuvi-core.dll"]
