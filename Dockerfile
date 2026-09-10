FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props global.json ./
COPY src/StayHub.Api/StayHub.Api.csproj src/StayHub.Api/
COPY src/StayHub.Business/StayHub.Business.csproj src/StayHub.Business/
COPY src/StayHub.Contracts/StayHub.Contracts.csproj src/StayHub.Contracts/
COPY src/StayHub.Dal/StayHub.Dal.csproj src/StayHub.Dal/
COPY src/StayHub.Domain/StayHub.Domain.csproj src/StayHub.Domain/
COPY src/StayHub.Mapping/StayHub.Mapping.csproj src/StayHub.Mapping/
RUN dotnet restore src/StayHub.Api/StayHub.Api.csproj

COPY src/ ./src/
RUN dotnet publish src/StayHub.Api/StayHub.Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "StayHub.Api.dll"]
