FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine@sha256:496914451b05ac5a24592024362ec370c3a2d3c3fe817f864e4aa55c4c194a56 AS build
WORKDIR /src
COPY src/ .
WORKDIR /src/apps/Altinn.Authentication/src/Altinn.Platform.Authentication
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:ecf8dcbaab8ebc4bdfb2322d6fe4217bbb118894310ebb6d53c580874ab4b27d AS final
EXPOSE 5040
WORKDIR /app
COPY --from=build /app .

# setup the user and group
# the user will have no password, using shell /bin/false and using the group dotnet
RUN addgroup -g 3000 dotnet && adduser -u 1000 -G dotnet -D -s /bin/false dotnet
# update permissions of files if neccessary before becoming dotnet user
USER dotnet
RUN mkdir /tmp/logtelemetry

ENTRYPOINT ["dotnet", "Altinn.Platform.Authentication.dll"]
