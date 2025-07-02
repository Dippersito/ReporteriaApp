# --- Etapa 1: Construcción (Usa la imagen completa del SDK de .NET) ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia el archivo del proyecto y restaura las dependencias primero
COPY ["Reporteria.API/Reporteria.API.csproj", "Reporteria.API/"]
RUN dotnet restore "Reporteria.API/Reporteria.API.csproj"

# Copia el resto de los archivos del código fuente
COPY . .
WORKDIR "/src/Reporteria.API"

# Compila y publica la aplicación en modo Release
RUN dotnet publish "Reporteria.API.csproj" -c Release -o /app/publish


# --- Etapa 2: Ejecución (Usa la imagen más ligera del Runtime de ASP.NET) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia solo los archivos publicados de la etapa de construcción
COPY --from=build /app/publish .

# Define el punto de entrada para ejecutar la aplicación
ENTRYPOINT ["dotnet", "Reporteria.API.dll"]