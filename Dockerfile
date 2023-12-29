FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY LangaraCPSC.WebAPI.csproj .
COPY DatabaseConfig.json .
COPY . .

RUN git submodule update --init KeyMan
RUN git submodule update --init opendatabaseapi
RUN rm -rf /KeyMan/OpenDatabaseAPI

RUN mkdir /app
COPY Images/ /app/Images
COPY keyfile.json /app
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app .
ENTRYPOINT ["dotnet", "LangaraCPSC.WebAPI.dll"]
