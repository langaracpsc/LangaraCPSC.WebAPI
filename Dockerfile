FROM  --platform=$BUILDPLATFORM  mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY LangaraCPSC.WebAPI/LangaraCPSC.WebAPI.csproj .
COPY LangaraCPSC.WebAPI/KeyMan KeyMan
COPY . .

RUN git submodule update --init LangaraCPSC.WebAPI/KeyMan

RUN mkdir /app
RUN mkdir /app/Images
RUN dotnet publish -c release -o /app

FROM  --platform=$BUILDPLATFORM  mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
RUN dotnet test

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 80
EXPOSE 443
COPY --from=build /app .

ENTRYPOINT ["dotnet", "LangaraCPSC.WebAPI.dll"]
