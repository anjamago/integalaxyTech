FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY src/IntergalaxyTech.API/*.csproj src/IntergalaxyTech.API/
COPY src/IntergalaxyTech.Application/*.csproj src/IntergalaxyTech.Application/
COPY src/IntergalaxyTech.Domain/*.csproj src/IntergalaxyTech.Domain/
COPY src/IntergalaxyTech.Infrastructure/*.csproj src/IntergalaxyTech.Infrastructure/
COPY tests/IntergalaxyTech.Tests/*.csproj tests/IntergalaxyTech.Tests/
COPY IntergalaxyTech.sln .
RUN dotnet restore

COPY src/ src/
WORKDIR /source/src/IntergalaxyTech.API
RUN dotnet publish -c Release -o /app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080
ENTRYPOINT ["dotnet", "IntergalaxyTech.API.dll"]
