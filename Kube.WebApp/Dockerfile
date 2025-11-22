FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Kube.WebApp/Kube.WebApp.csproj", "Kube.WebApp/"]
COPY ["Kube.Contracts/Kube.Contracts.csproj", "Kube.Contracts/"]


RUN dotnet restore "Kube.WebApp/Kube.WebApp.csproj"
COPY . .
WORKDIR "/src/Kube.WebApp"
RUN dotnet build "Kube.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Kube.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Kube.WebApp.dll"]