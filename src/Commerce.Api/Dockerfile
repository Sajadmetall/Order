FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/Commerce.Api/Commerce.Api.csproj src/Commerce.Api/
COPY src/Application/Commerce.Application.csproj src/Application/
COPY src/Domain/Commerce.Domain.csproj src/Domain/
COPY src/Infrastructure/Commerce.Infrastructure.csproj src/Infrastructure/
RUN dotnet restore src/Commerce.Api/Commerce.Api.csproj

COPY src/ src/
RUN dotnet publish src/Commerce.Api/Commerce.Api.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "Commerce.Api.dll"]
