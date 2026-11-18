# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props GotFired.slnx ./
COPY GotFired.Model/GotFired.Model.csproj GotFired.Model/
COPY GotFired.DAL/GotFired.DAL.csproj GotFired.DAL/
COPY GotFired.Business/GotFired.Business.csproj GotFired.Business/
COPY GotFired.Api/GotFired.Api.csproj GotFired.Api/
RUN dotnet restore GotFired.Api/GotFired.Api.csproj
COPY . .
RUN dotnet publish GotFired.Api/GotFired.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
# The aspnet image listens on 8080 and provides the non-root "app" user.
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "GotFired.Api.dll"]
