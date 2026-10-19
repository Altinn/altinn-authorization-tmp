FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine@sha256:3cc3bbbbf93d82104892f42aa9106b6be4d120346dea0649643a97c801525256 AS build
WORKDIR /app
WORKDIR /src
COPY src/ .
WORKDIR /src/apps/Altinn.AccessManagement/src/Altinn.AccessManagement
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:f62a272ac1b46e83f56b8ed0416572f31cd1128e2c4a5e63eb34d348e4a36095 AS final
EXPOSE 5110
WORKDIR /app
COPY --from=build /app .

# Install tzdata for timezone support, which is needed for correct handling of dates and times in the application.
# Do not add krb5-libs: once libgssapi_krb5 is present, Npgsql's default GssEncryptionMode=Prefer runs native
# Kerberos code on every new physical connection. We authenticate to Azure PostgreSQL with Entra tokens, not Kerberos.
RUN apk add --no-cache tzdata

# Never attempt GSS encryption towards PostgreSQL, whatever the image contains. Npgsql reads PGGSSENCMODE.
ENV PGGSSENCMODE=disable

# setup the user and group
# the user will have no password, using shell /bin/false and using the group dotnet
RUN addgroup -g 3000 dotnet && adduser -u 1000 -G dotnet -D -s /bin/false dotnet
# update permissions of files if neccessary before becoming dotnet user
USER dotnet
RUN mkdir /tmp/logtelemetry
ENTRYPOINT ["dotnet", "Altinn.AccessManagement.dll"]
