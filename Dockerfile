FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# Install coverlet
ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global coverlet.console

# Copy csproj and restore as distinct layers
COPY src/Bottlecap.Net.Notifications/*.csproj ./Bottlecap.Net.Notifications/
COPY src/Bottlecap.Net.Notifications.EF/*.csproj ./Bottlecap.Net.Notifications.EF/

COPY src/Examples/Bottlecap.Net.Notifications.ConsoleExample/*.csproj ./Examples/Bottlecap.Net.Notifications.ConsoleExample/
COPY src/Tests/UnitTests.Bottlecap.Net.Notifications/*.csproj ./Tests/UnitTests.Bottlecap.Net.Notifications/
COPY src/Tests/UnitTests.Bottlecap.Net.Notifications.EF/*.csproj ./Tests/UnitTests.Bottlecap.Net.Notifications.EF/

COPY src/Bottlecap.Net.Notifications.Transporters.SendGrid/*.csproj ./Bottlecap.Net.Notifications.Transporters.SendGrid/
COPY src/*.sln ./
RUN dotnet restore

# Define our environment variables so we can set our package information
ARG PACKAGE_VERSION
ARG PACKAGE_API

# Copy all notifications and build
COPY src/Bottlecap.Net.Notifications/. ./Bottlecap.Net.Notifications/

# Pack notifications
RUN dotnet pack ./Bottlecap.Net.Notifications/ -c Release -o out /p:Version=$PACKAGE_VERSION

# Copy all sendgrid and build
COPY src/Bottlecap.Net.Notifications.Transporters.SendGrid/. ./Bottlecap.Net.Notifications.Transporters.SendGrid/

# Pack sendgrid
RUN dotnet pack ./Bottlecap.Net.Notifications.Transporters.SendGrid/ -c Release -o out /p:Version=$PACKAGE_VERSION

# Copy all EF and build
COPY src/Bottlecap.Net.Notifications.EF/. ./Bottlecap.Net.Notifications.EF/

# Pack EF
RUN dotnet pack ./Bottlecap.Net.Notifications.EF/ -c Release -o out /p:Version=$PACKAGE_VERSION

# Run unit tests
COPY src/Tests/UnitTests.Bottlecap.Net.Notifications/. ./Tests/UnitTests.Bottlecap.Net.Notifications/
RUN dotnet test ./Tests/UnitTests.Bottlecap.Net.Notifications/ -c Release /p:CollectCoverage=true /p:CoverletOutput="../result/codecoverage/coverage.json" /p:Exclude="[xunit.*]*"

COPY src/Tests/UnitTests.Bottlecap.Net.Notifications.EF/. ./Tests/UnitTests.Bottlecap.Net.Notifications.EF/
RUN dotnet test ./Tests/UnitTests.Bottlecap.Net.Notifications.EF/ -c Release /p:CollectCoverage=true /p:CoverletOutput="../result/codecoverage/coverage.json" /p:MergeWith="../result/codecoverage/coverage.json" /p:Exclude="[xunit.*]*"

# Push all packages
RUN dotnet nuget push ./Bottlecap.Net.Notifications/out/Bottlecap.Net.Notifications.$PACKAGE_VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $PACKAGE_API
RUN dotnet nuget push ./Bottlecap.Net.Notifications.Transporters.SendGrid/out/Bottlecap.Net.Notifications.Transporters.SendGrid.$PACKAGE_VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $PACKAGE_API
RUN dotnet nuget push ./Bottlecap.Net.Notifications.EF/out/Bottlecap.Net.Notifications.EF.$PACKAGE_VERSION.nupkg -s https://api.nuget.org/v3/index.json -k $PACKAGE_API