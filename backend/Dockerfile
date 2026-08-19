FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY BezorgBaas.sln ./
COPY src/BezorgBaas.Domain/BezorgBaas.Domain.csproj src/BezorgBaas.Domain/
COPY src/BezorgBaas.Application/BezorgBaas.Application.csproj src/BezorgBaas.Application/
COPY src/BezorgBaas.Infrastructure/BezorgBaas.Infrastructure.csproj src/BezorgBaas.Infrastructure/
COPY src/BezorgBaas.Api/BezorgBaas.Api.csproj src/BezorgBaas.Api/
RUN dotnet restore

COPY src ./src
RUN dotnet publish src/BezorgBaas.Api/BezorgBaas.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BezorgBaas.Api.dll"]
