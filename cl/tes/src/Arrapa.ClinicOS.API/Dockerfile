FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["global.json", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["Arrapa.ClinicOS.sln", "./"]

COPY ["src/Arrapa.ClinicOS.Shared/", "src/Arrapa.ClinicOS.Shared/"]
COPY ["src/Arrapa.ClinicOS.Contracts/", "src/Arrapa.ClinicOS.Contracts/"]
COPY ["src/Arrapa.ClinicOS.Domain/", "src/Arrapa.ClinicOS.Domain/"]
COPY ["src/Arrapa.ClinicOS.Application/", "src/Arrapa.ClinicOS.Application/"]
COPY ["src/Arrapa.ClinicOS.Persistence/", "src/Arrapa.ClinicOS.Persistence/"]
COPY ["src/Arrapa.ClinicOS.Infrastructure/", "src/Arrapa.ClinicOS.Infrastructure/"]
COPY ["src/Arrapa.ClinicOS.API/", "src/Arrapa.ClinicOS.API/"]

RUN dotnet restore "src/Arrapa.ClinicOS.API/Arrapa.ClinicOS.API.csproj"
RUN dotnet publish "src/Arrapa.ClinicOS.API/Arrapa.ClinicOS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Arrapa.ClinicOS.API.dll"]
