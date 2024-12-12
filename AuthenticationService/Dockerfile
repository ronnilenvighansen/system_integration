FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY ["AuthenticationService/AuthenticationService.csproj", "AuthenticationService/"]
RUN dotnet restore "AuthenticationService/AuthenticationService.csproj"

COPY ["AuthenticationService/", "AuthenticationService/"]
WORKDIR "/src/AuthenticationService"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "AuthenticationService.dll"]
