# API Dockerfile (multi-stage)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore
COPY ["src/Ballastlane.Api/Ballastlane.Api.csproj", "src/Ballastlane.Api/"]
RUN dotnet restore "src/Ballastlane.Api/Ballastlane.Api.csproj"

# copy everything and publish
COPY . .
RUN dotnet publish "src/Ballastlane.Api/Ballastlane.Api.csproj" -c Release -o /app/publish

# runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Ballastlane.Api.dll"]
