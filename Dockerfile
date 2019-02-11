FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./
COPY ./ServiceBus/appsettings.docker.json ./ServiceBus/appsettings.json
RUN dotnet restore

# Copy everything else and build
RUN dotnet publish ./ServiceBus/ServiceBus.csproj -c Release -o ../out

# Build runtime image
FROM microsoft/dotnet:aspnetcore-runtime
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "ServiceBus.dll"]