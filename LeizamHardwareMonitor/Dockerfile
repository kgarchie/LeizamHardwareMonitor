FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["LeizamHardwareMonitor/LeizamHardwareMonitor.csproj", "LeizamHardwareMonitor/"]
RUN dotnet restore "LeizamHardwareMonitor/LeizamHardwareMonitor.csproj"
COPY . .
WORKDIR "/src/LeizamHardwareMonitor"
RUN dotnet build "LeizamHardwareMonitor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LeizamHardwareMonitor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LeizamHardwareMonitor.dll"]
