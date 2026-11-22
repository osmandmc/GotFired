# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props GotFired.slnx ./
COPY GotFired.Model/GotFired.Model.csproj GotFired.Model/
COPY GotFired.DAL/GotFired.DAL.csproj GotFired.DAL/
COPY GotFired.Api/GotFired.Api.csproj GotFired.Api/
RUN dotnet restore GotFired.Api/GotFired.Api.csproj
COPY . .
RUN dotnet publish GotFired.Api/GotFired.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
# tini as PID 1: the kernel ignores SIGABRT for PID 1, so without an init a crashing .NET process
# (e.g. a startup configuration error) hangs instead of exiting and the container is never restarted.
RUN apt-get update && apt-get install -y --no-install-recommends tini && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app .
# The aspnet image listens on 8080 and provides the non-root "app" user.
USER app
EXPOSE 8080
ENTRYPOINT ["/usr/bin/tini", "--", "dotnet", "GotFired.Api.dll"]
