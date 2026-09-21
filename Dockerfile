# syntax=docker/dockerfile:1
# Epic 11, Feature 11.7: builds Kiwbi.Web for a container-based host (Render). Build context is the repo root
# (Kiwbi.slnx lives there) since Kiwbi.Web references the other 3 projects via ProjectReference.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy just the project files first so `dotnet restore` is cached across builds that only change source code.
COPY src/Kiwbi.Domain/Kiwbi.Domain.csproj src/Kiwbi.Domain/
COPY src/Kiwbi.Application/Kiwbi.Application.csproj src/Kiwbi.Application/
COPY src/Kiwbi.Infrastructure/Kiwbi.Infrastructure.csproj src/Kiwbi.Infrastructure/
COPY src/Kiwbi.Web/Kiwbi.Web.csproj src/Kiwbi.Web/
COPY Directory.Build.props .
RUN dotnet restore src/Kiwbi.Web/Kiwbi.Web.csproj

COPY src/ src/
RUN dotnet publish src/Kiwbi.Web/Kiwbi.Web.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

# Render injects $PORT at runtime and requires the app to bind to it on 0.0.0.0; falls back to 8080 for any
# other host/local `docker run` where $PORT isn't set.
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet Kiwbi.Web.dll --urls http://+:${PORT:-8080}"]
