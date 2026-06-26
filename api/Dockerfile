FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY api/Api.csproj api/
RUN dotnet restore api/Api.csproj

COPY api/ api/
WORKDIR /src/api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 8080

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
