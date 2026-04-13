# API Dockerfile (multi-stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy ALL csproj files first — NuGet restore becomes a cached layer that only
# invalidates when a dependency changes, not when source files change.
COPY ["src/Ballastlane.Api/Ballastlane.Api.csproj",                 "src/Ballastlane.Api/"]
COPY ["src/Ballastlane.Application/Ballastlane.Application.csproj", "src/Ballastlane.Application/"]
COPY ["src/Ballastlane.Domain/Ballastlane.Domain.csproj",           "src/Ballastlane.Domain/"]
COPY ["src/Ballastlane.Infrastructure/Ballastlane.Infrastructure.csproj", "src/Ballastlane.Infrastructure/"]
RUN dotnet restore "src/Ballastlane.Api/Ballastlane.Api.csproj"

# copy everything and publish (packages already restored above)
COPY . .
RUN dotnet publish "src/Ballastlane.Api/Ballastlane.Api.csproj" -c Release -o /app/publish --no-restore

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Ballastlane.Api.dll"]
