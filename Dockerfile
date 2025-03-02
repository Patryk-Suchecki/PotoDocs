FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY PotoDocs.Shared ./PotoDocs.Shared

COPY PotoDocs.Blazor ./PotoDocs.Blazor

WORKDIR /app/PotoDocs.Blazor

RUN dotnet restore

RUN dotnet publish -c Release -o /publish

FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html

COPY --from=build /publish/wwwroot .

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
