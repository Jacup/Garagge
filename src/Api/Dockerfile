# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
# USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Api/Api.csproj", "src/Api/"]
COPY ["src/Infrastructure/Infrastructure.csproj", "src/Infrastructure/"]
COPY ["src/Application/Application.csproj", "src/Application/"]
COPY ["src/Domain/Domain.csproj", "src/Domain/"]
COPY ["src/SharedKernel/SharedKernel.csproj", "src/SharedKernel/"]
RUN dotnet restore "./src/Api/Api.csproj"
COPY . .
WORKDIR "/src/src/Api"
RUN dotnet build "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

ARG INCLUDE_FRONTEND=true

# Etap budowania frontu tylko jeśli INCLUDE_FRONTEND=true
FROM node:20 AS frontend-build
WORKDIR /src/web
COPY garagge-web/package*.json ./
RUN npm install
COPY garagge-web/ ./
RUN npm run build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Finalny obraz bez frontu (dev)
FROM base AS final-no-frontend
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]

# Finalny obraz z frontem (prod, domyślny)
FROM base AS final-with-frontend
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=frontend-build /src/web/dist ./wwwroot
ENTRYPOINT ["dotnet", "Api.dll"]