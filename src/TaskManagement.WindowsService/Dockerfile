FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/TaskManagement.WindowsService/TaskManagement.WindowsService.csproj", "src/TaskManagement.WindowsService/"]
COPY ["src/TaskManagement.Application/TaskManagement.Application.csproj", "src/TaskManagement.Application/"]
COPY ["src/TaskManagement.Domain/TaskManagement.Domain.csproj", "src/TaskManagement.Domain/"]
COPY ["src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "src/TaskManagement.Infrastructure/"]
RUN dotnet restore "src/TaskManagement.WindowsService/TaskManagement.WindowsService.csproj"
COPY . .
WORKDIR "/src/src/TaskManagement.WindowsService"
RUN dotnet build "TaskManagement.WindowsService.csproj" -c Release -o /app/build

FROM build AS publish
WORKDIR "/src/src/TaskManagement.WindowsService"
RUN dotnet publish "TaskManagement.WindowsService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.WindowsService.dll"]
