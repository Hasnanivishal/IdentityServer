FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MyIdentityServer.csproj", "."]
RUN dotnet restore "./MyIdentityServer.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./MyIdentityServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./MyIdentityServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyIdentityServer.dll"]