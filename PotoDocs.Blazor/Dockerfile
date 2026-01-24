FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PotoDocs.Blazor/PotoDocs.Blazor.csproj ./PotoDocs.Blazor/
COPY PotoDocs.Shared/PotoDocs.Shared.csproj ./PotoDocs.Shared/

COPY . .

RUN rm -rf /src/PotoDocs.Blazor/bin /src/PotoDocs.Blazor/obj

RUN dotnet restore PotoDocs.Blazor/PotoDocs.Blazor.csproj

WORKDIR /src/PotoDocs.Blazor
RUN dotnet publish -c Release -o /app/publish

FROM nginx:alpine AS runtime

COPY --from=build /app/publish/wwwroot /usr/share/nginx/html

COPY PotoDocs.Blazor/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]