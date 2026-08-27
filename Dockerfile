# ==========================================
# Etapa 1: Build y Publicación (.NET 10)
# ==========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY ["TicketsApp.csproj", "./"]
RUN dotnet restore "./TicketsApp.csproj"

# Copiar el resto del código y compilar en modo Release
COPY . .
RUN dotnet publish "TicketsApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ==========================================
# Etapa 2: Runtime en Debian Linux (.NET 10)
# ==========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Crear directorio de uploads con permisos adecuados
RUN mkdir -p /app/wwwroot/uploads

COPY --from=build /app/publish .

# Exponer el puerto estándar
ENV ASPNETCORE_HTTP_PORTS=80
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 80

ENTRYPOINT ["dotnet", "TicketsApp.dll"]
