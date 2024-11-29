FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /app
WORKDIR /src
COPY src/ .
WORKDIR /src/apps/Altinn.AccessManagement/src/Altinn.AccessManagement
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "Altinn.AccessManagement.dll"]
