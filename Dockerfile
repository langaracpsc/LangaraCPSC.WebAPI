FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY LangaraCPSC.WebAPI.csproj .
COPY DatabaseConfig.json .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app


FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app .
ENTRYPOINT ["dotnet", "LangaraCPSC.WebAPI.dll"]
