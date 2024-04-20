FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CompileResultsUpdaterService/CompileResultsUpdaterService.csproj", "CompileResultsUpdaterService/"]
RUN dotnet restore "CompileResultsUpdaterService/CompileResultsUpdaterService.csproj"
COPY . .
WORKDIR "/src/CompileResultsUpdaterService"
RUN dotnet build "CompileResultsUpdaterService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CompileResultsUpdaterService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CompileResultsUpdaterService.dll"]
