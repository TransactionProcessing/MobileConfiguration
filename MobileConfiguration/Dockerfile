FROM stuartferguson/txnprocbase AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["NuGet.Config", "."]
COPY ["MobileConfiguration.csproj", "."]
RUN dotnet restore "./MobileConfiguration.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MobileConfiguration.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MobileConfiguration.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MobileConfiguration.dll"]