# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
# Copy project file and restore
COPY BackendApi.csproj .
RUN dotnet restore
# Copy the rest and publish
COPY . .
RUN dotnet publish -c Release -o /app/out

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./
# Render provides $PORT; bind to it
CMD ["sh","-c","dotnet BackendApi.dll --urls http://0.0.0.0:${PORT}"]
