FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["ConcordCloud.API/ConcordCloud.API.csproj", "ConcordCloud.API/"]
COPY ["ConcordCloud.Core/ConcordCloud.Core.csproj", "ConcordCloud.Core/"]
COPY ["ConcordCloud.Infrastructure/ConcordCloud.Infrastructure.csproj", "ConcordCloud.Infrastructure/"]
RUN dotnet restore "ConcordCloud.API/ConcordCloud.API.csproj"
COPY . .
WORKDIR "/src/ConcordCloud.API"
RUN dotnet build "ConcordCloud.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConcordCloud.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConcordCloud.API.dll"]
