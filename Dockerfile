FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/Bottlecap.Net.Notifications/*.csproj ./Bottlecap.Net.Notifications/
COPY src/Bottlecap.Net.Notifications.EF/*.csproj ./Bottlecap.Net.Notifications.EF/

COPY src/Examples/Bottlecap.Net.Notifications.ConsoleExample/*.csproj ./Examples/Bottlecap.Net.Notifications.ConsoleExample/
COPY src/Tests/UnitTests.Bottlecap.Net.Notifications/*.csproj ./Tests/UnitTests.Bottlecap.Net.Notifications/

COPY src/Bottlecap.Net.Notifications.Transporters.SendGrid/*.csproj ./Bottlecap.Net.Notifications.Transporters.SendGrid/
COPY src/*.sln ./
RUN dotnet restore

# Define our environment variables so we can set our package information
ARG PACKAGE_VERSION
ARG PACKAGE_API

FROM build AS notifications

# Copy all notifications and build
COPY src/Bottlecap.Net.Notifications/. ./Bottlecap.Net.Notifications/
RUN dotnet build ./Bottlecap.Net.Notifications/ -c Release -o out /p:Version=$PACKAGE_VERSION

# pack notifications
RUN dotnet pack ./Bottlecap.Net.Notifications/ -c Release -o out --no-build --no-restore /p:Version=$PACKAGE_VERSION
RUN dotnet nuget push ./Bottlecap.Net.Notifications/out/Bottlecap.Net.Notifications.$PACKAGE_VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $PACKAGE_API

FROM build AS transporters-sendgrid

# Copy all sendgrid and build
COPY src/Bottlecap.Net.Notifications.Transporters.SendGrid/. ./Bottlecap.Net.Notifications.Transporters.SendGrid/
RUN dotnet build ./Bottlecap.Net.Notifications.Transporters.SendGrid/ -c Release -o out /p:Version=$PACKAGE_VERSION

# pack sendgrid
RUN dotnet pack ./Bottlecap.Net.Notifications.Transporters.SendGrid/ -c Release -o out --no-build --no-restore /p:Version=$PACKAGE_VERSION
RUN dotnet nuget push ./Bottlecap.Net.Notifications.Transporters.SendGrid/out/Bottlecap.Net.Notifications.Transporters.SendGrid.$PACKAGE_VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $PACKAGE_API

FROM build AS notifications-ef

# Copy all EF and build
COPY src/Bottlecap.Net.Notifications.EF/. ./Bottlecap.Net.Notifications.EF/
RUN dotnet build ./Bottlecap.Net.Notifications.EF/ -c Release -o out /p:Version=$PACKAGE_VERSION

# pack EF
RUN dotnet pack ./Bottlecap.Net.Notifications.EF/ -c Release -o out --no-build --no-restore /p:Version=$PACKAGE_VERSION
RUN dotnet nuget push ./Bottlecap.Net.Notifications.EF/out/Bottlecap.Net.Notifications.EF.$PACKAGE_VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $PACKAGE_API