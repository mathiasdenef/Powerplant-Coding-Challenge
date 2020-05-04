#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Powerplant Coding Challenge/Powerplant Coding Challenge.csproj", "Powerplant Coding Challenge/"]
COPY ["BusinessLayer/Business.csproj", "BusinessLayer/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "Powerplant Coding Challenge/Powerplant Coding Challenge.csproj"

# Setup NodeJs
RUN apt-get update && \
    apt-get install -y wget && \
    apt-get install -y gnupg2 && \
    wget -qO- https://deb.nodesource.com/setup_10.x | bash - && \
    apt-get install -y build-essential nodejs


COPY . .
WORKDIR "/src/Powerplant Coding Challenge"
RUN dotnet build "Powerplant Coding Challenge.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Powerplant Coding Challenge.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Powerplant Coding Challenge.dll"]