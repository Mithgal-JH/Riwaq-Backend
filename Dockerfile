# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["Team3.Backend/Team3.Backend.csproj", "Team3.Backend/"]

RUN dotnet restore "Team3.Backend/Team3.Backend.csproj"

COPY . .

WORKDIR "/src/Team3.Backend"

RUN dotnet publish "Team3.Backend.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["sh", "-c", "dotnet Team3.Backend.dll --urls http://0.0.0.0:${PORT:-8080}"]