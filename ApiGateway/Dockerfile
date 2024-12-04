FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
RUN dotnet restore "ApiGateway/ApiGateway.csproj"

COPY ["ApiGateway/", "ApiGateway/"]
WORKDIR "/src/ApiGateway"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

COPY ["ApiGateway/ocelot.json", "/app/publish"]

EXPOSE 80

ENTRYPOINT ["dotnet", "ApiGateway.dll"]
