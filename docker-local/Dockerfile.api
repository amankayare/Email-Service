FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EmailService.API/EmailService.API.csproj", "EmailService.API/"]
COPY ["EmailService.Core/EmailService.Core.csproj", "EmailService.Core/"]
COPY ["EmailService.Infrastructure/EmailService.Infrastructure.csproj", "EmailService.Infrastructure/"]
RUN dotnet restore "EmailService.API/EmailService.API.csproj"
COPY . .
WORKDIR "/src/EmailService.API"
RUN dotnet build "EmailService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmailService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailService.API.dll"]
