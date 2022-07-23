FROM mcr.microsoft.com/dotnet/aspnet:6.0.302-bullseye-slim-arm32v7 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.302-bullseye-slim-arm32v7 AS build
WORKDIR /src
COPY ["RuuviCore/RuuviCore.csproj", "RuuviCore/"]
COPY ["Common/Common.csproj", "Common/"]
COPY ["FileStorageProvider/FileStorageProvider.csproj", "FileStorageProvider/"]
COPY ["GrainInterfaces/GrainInterfaces.csproj", "GrainInterfaces/"]
COPY ["GrainServices/GrainServices.csproj", "GrainServices/"]
COPY ["Grains/Grains.csproj", "Grains/"]
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
