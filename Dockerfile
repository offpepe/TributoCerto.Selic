FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY TributoCerto.Selic.sln .
COPY ./TributoCerto.SelicJob/TributoCerto.SelicJob.csproj ./TributoCerto.SelicJob/TributoCerto.SelicJob.csproj
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o publish

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS release
#RUN dotnet dev-certs https --clean
#RUN dotnet dev-certs https --trust
COPY . .
COPY --from=build ./app/publish ./
ENV TZ="America/Sao_Paulo"
#ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 9000
ENTRYPOINT ["dotnet", "TributoCerto.SelicJob.dll"]
