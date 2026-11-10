# Imagen de la API (REST + SOAP). Se construye desde la raíz del repositorio:
#   docker build -f src/ArcaSim.Api/Dockerfile -t arcasim-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj, así la restauración de paquetes queda en caché entre builds.
COPY src/ArcaSim.Dominio/ArcaSim.Dominio.csproj src/ArcaSim.Dominio/
COPY src/ArcaSim.Contratos/ArcaSim.Contratos.csproj src/ArcaSim.Contratos/
COPY src/ArcaSim.Aplicacion/ArcaSim.Aplicacion.csproj src/ArcaSim.Aplicacion/
COPY src/ArcaSim.Infraestructura/ArcaSim.Infraestructura.csproj src/ArcaSim.Infraestructura/
COPY src/ArcaSim.Api/ArcaSim.Api.csproj src/ArcaSim.Api/
RUN dotnet restore src/ArcaSim.Api/ArcaSim.Api.csproj

COPY src/ src/
RUN dotnet publish src/ArcaSim.Api/ArcaSim.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# La imagen de .NET escucha en el puerto 8080 y corre con un usuario sin privilegios.
EXPOSE 8080
ENTRYPOINT ["dotnet", "ArcaSim.Api.dll"]
