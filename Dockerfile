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

# SonarCloud docker:S6471 fix: don't run as root - the base image ships a pre-created non-root "app" user
# (exposed via the APP_UID build-time env var), so we just chown the published output to it and switch.
COPY --from=build --chown=$APP_UID:$APP_UID /app/publish .
USER $APP_UID

EXPOSE 8080

# Render injects $PORT at runtime and requires the app to bind to it on 0.0.0.0; falls back to 8080 for any
# other host/local `docker run` where $PORT isn't set.
#
# SonarCloud docker:S5332 ("clear-text protocol") flags this http:// binding - accepted/won't-fix, not a real
# vulnerability here: Render terminates HTTPS at its own edge/load balancer with an auto-managed certificate
# and forwards plain HTTP to the container over its private network, which is the standard, documented
# pattern for this and effectively every PaaS (Heroku, Azure App Service, AWS behind an ALB, etc.) -
# terminating TLS again inside the container would be redundant and isn't what Render expects.
ENTRYPOINT ["/bin/sh", "-c", "exec dotnet Kiwbi.Web.dll --urls http://+:${PORT:-8080}"]
