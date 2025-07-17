FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine@sha256:a4eb48407ea8a1a4af92ee6630ec91af216365fdf45e7f08e1b5f4ce602407f4 AS build
WORKDIR /app
WORKDIR /src
COPY src/ .
WORKDIR /src/apps/Altinn.AccessManagement/src/Altinn.AccessManagement
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine@sha256:53867b9ebb86beab644de47226867d255d0360e38324d2afb3ff4d2f2933e33f AS final
EXPOSE 5110
WORKDIR /app
COPY --from=build /app .

# setup the user and group
# the user will have no password, using shell /bin/false and using the group dotnet
RUN addgroup -g 3000 dotnet && adduser -u 1000 -G dotnet -D -s /bin/false dotnet
# update permissions of files if neccessary before becoming dotnet user
USER dotnet
RUN mkdir /tmp/logtelemetry
ENTRYPOINT ["dotnet", "Altinn.AccessManagement.dll"]
