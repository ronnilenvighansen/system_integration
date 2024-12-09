FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY *.sln ./
COPY PostService/PostService.csproj ./PostService/
COPY Shared/Shared.csproj ./Shared/

RUN dotnet restore PostService/PostService.csproj
RUN dotnet restore Shared/Shared.csproj

COPY PostService/. ./PostService/
COPY Shared/. ./Shared/

RUN dotnet publish PostService/PostService.csproj -c Release -o /app/out/PostService
RUN dotnet publish Shared/Shared.csproj -c Release -o /app/out/Shared

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

COPY --from=build /app/out/PostService ./PostService
COPY --from=build /app/out/Shared ./Shared

EXPOSE 80

ENTRYPOINT ["dotnet", "PostService/PostService.dll"]
