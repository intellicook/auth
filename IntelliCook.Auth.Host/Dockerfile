FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["IntelliCook.Auth.Host/IntelliCook.Auth.Host.csproj", "IntelliCook.Auth.Host/"]
COPY ["IntelliCook.Auth.Infrastructure/IntelliCook.Auth.Infrastructure.csproj", "IntelliCook.Auth.Infrastructure/"]
COPY ["IntelliCook.Auth.Contract/IntelliCook.Auth.Contract.csproj", "IntelliCook.Auth.Contract/"]
RUN dotnet restore "IntelliCook.Auth.Host/IntelliCook.Auth.Host.csproj"
COPY . .
WORKDIR "/src/IntelliCook.Auth.Host"
RUN dotnet build "IntelliCook.Auth.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IntelliCook.Auth.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IntelliCook.Auth.Host.dll"]
