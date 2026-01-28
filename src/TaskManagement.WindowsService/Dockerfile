FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TaskManagement.WindowsService/TaskManagement.WindowsService.csproj", "TaskManagement.WindowsService/"]
COPY ["TaskManagement.Application/TaskManagement.Application.csproj", "TaskManagement.Application/"]
COPY ["TaskManagement.Domain/TaskManagement.Domain.csproj", "TaskManagement.Domain/"]
COPY ["TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "TaskManagement.Infrastructure/"]
RUN dotnet restore "TaskManagement.WindowsService/TaskManagement.WindowsService.csproj"
COPY . .
WORKDIR "/src/TaskManagement.WindowsService"
RUN dotnet build "TaskManagement.WindowsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagement.WindowsService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagement.WindowsService.dll"]
