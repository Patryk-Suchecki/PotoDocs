FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiujemy pliki projektów
COPY ["PotoDocs.API/PotoDocs.API.csproj", "PotoDocs.API/"]
COPY ["PotoDocs.Shared/PotoDocs.Shared.csproj", "PotoDocs.Shared/"]

# Przywracamy zależności (Restore)
RUN dotnet restore "PotoDocs.API/PotoDocs.API.csproj"

# Kopiujemy resztę plików
COPY . .

# Budujemy
WORKDIR "/src/PotoDocs.API"
RUN dotnet build "PotoDocs.API.csproj" -c Release -o /app/build

# Publikujemy
FROM build AS publish
RUN dotnet publish "PotoDocs.API.csproj" -c Release -o /app/publish

# Uruchamiamy
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "PotoDocs.API.dll"]