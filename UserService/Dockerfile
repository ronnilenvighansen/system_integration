FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY *.sln ./
COPY UserService/UserService.csproj ./UserService/
COPY Shared/Shared.csproj ./Shared/

RUN dotnet restore UserService/UserService.csproj
RUN dotnet restore Shared/Shared.csproj

COPY UserService/. ./UserService/
COPY Shared/. ./Shared/

RUN dotnet publish UserService/UserService.csproj -c Release -o /app/out/UserService
RUN dotnet publish Shared/Shared.csproj -c Release -o /app/out/Shared


FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

COPY --from=build /app/out/UserService ./UserService
COPY --from=build /app/out/Shared ./Shared

EXPOSE 80

ENTRYPOINT ["dotnet", "UserService/UserService.dll"]
