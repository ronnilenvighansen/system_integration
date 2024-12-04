FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY *.sln ./
COPY PostService/PostService.csproj ./PostService/
COPY UserService/UserService.csproj ./UserService/

RUN dotnet restore UserService/UserService.csproj
RUN dotnet restore PostService/PostService.csproj

COPY PostService/. ./PostService/
COPY UserService/. ./UserService/

RUN dotnet publish UserService/UserService.csproj -c Release -o /app/out/UserService
RUN dotnet publish PostService/PostService.csproj -c Release -o /app/out/PostService


FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

COPY --from=build /app/out/UserService ./UserService
COPY --from=build /app/out/PostService ./PostService

EXPOSE 80

ENTRYPOINT ["dotnet", "UserService/UserService.dll"]
