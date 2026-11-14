# Imagen del portal Blazor. Se construye desde la raíz del repositorio:
#   docker build -f src/ArcaSim.Web/Dockerfile -t arcasim-web .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj, así la restauración de paquetes queda en caché entre builds.
COPY src/ArcaSim.Contratos/ArcaSim.Contratos.csproj src/ArcaSim.Contratos/
COPY src/ArcaSim.Web/ArcaSim.Web.csproj src/ArcaSim.Web/
RUN dotnet restore src/ArcaSim.Web/ArcaSim.Web.csproj

COPY src/ArcaSim.Contratos/ src/ArcaSim.Contratos/
COPY src/ArcaSim.Web/ src/ArcaSim.Web/
RUN dotnet publish src/ArcaSim.Web/ArcaSim.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# La imagen de .NET escucha en el puerto 8080 y corre con un usuario sin privilegios.
EXPOSE 8080
ENTRYPOINT ["dotnet", "ArcaSim.Web.dll"]
