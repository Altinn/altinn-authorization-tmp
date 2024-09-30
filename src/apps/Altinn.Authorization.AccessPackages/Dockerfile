FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app
WORKDIR /src
COPY src/ .
WORKDIR /src/apps/Altinn.Authorization.AccessPackages/src/Altinn.Authorization.AccessPackages
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Altinn.Authorization.AccessPackages.dll"]