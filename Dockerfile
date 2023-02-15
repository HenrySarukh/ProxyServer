# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

WORKDIR /src
COPY VMedia_Task/*.csproj .
RUN dotnet restore
COPY VMedia_Task .
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as runtime
WORKDIR /publish
COPY --from=build-env /publish .

ENV ASPNETCORE_URLS="http://+:80;https://+:443"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.crt 
ENV ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/https/localhost.key
COPY localhost.crt /https/
COPY localhost.key /https/

ENTRYPOINT ["dotnet", "VMedia_Task.dll"]