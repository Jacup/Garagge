FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
RUN dotnet restore "./src/Api/Api.csproj"
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

ARG INCLUDE_FRONTEND=true

FROM node:20 AS frontend-build
WORKDIR /src/web
COPY garagge-web/package*.json ./
RUN npm install
COPY garagge-web/ ./
RUN npm run build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG VERSION=0.0.0
RUN dotnet publish "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:Version=$VERSION

FROM base AS final-no-frontend
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]

FROM base AS final-with-frontend
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=frontend-build /src/web/dist ./wwwroot
ENTRYPOINT ["dotnet", "Api.dll"]