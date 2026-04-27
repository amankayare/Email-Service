FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EmailService.Worker/EmailService.Worker.csproj", "EmailService.Worker/"]
COPY ["EmailService.Core/EmailService.Core.csproj", "EmailService.Core/"]
COPY ["EmailService.Infrastructure/EmailService.Infrastructure.csproj", "EmailService.Infrastructure/"]
RUN dotnet restore "EmailService.Worker/EmailService.Worker.csproj"
COPY . .
WORKDIR "/src/EmailService.Worker"
RUN dotnet build "EmailService.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EmailService.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmailService.Worker.dll"]
