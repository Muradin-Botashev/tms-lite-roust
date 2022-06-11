#Build image
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build

RUN mkdir -p /app

RUN chmod -R 777 /app

WORKDIR /app

COPY /src/backend/ .
COPY docker/nuget.config .

RUN dotnet  publish API/ -c Release -o /app/build/

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS runtime

WORKDIR /app

RUN chmod -R 777 /app

COPY --from=build /app/build .

ENTRYPOINT  dotnet API.dll --urls http://0.0.0.0:5000
