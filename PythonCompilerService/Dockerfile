﻿FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /python-compiler
EXPOSE 3300

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["PythonCompilerService/PythonCompilerService.csproj", "PythonCompilerService/"]
RUN dotnet restore "PythonCompilerService/PythonCompilerService.csproj"
COPY . .
WORKDIR "/src/PythonCompilerService"
RUN dotnet build "PythonCompilerService.csproj" -c "$MODE" -o /python-compiler/build

FROM build AS publish
RUN dotnet publish "PythonCompilerService.csproj" -c "$MODE" -o /python-compiler/publish /p:UseAppHost=false

FROM base AS final

RUN apt-get update -y
RUN apt show python3
RUN apt-get install -y python3

RUN adduser runner --disabled-password --gecos ""

WORKDIR /home/runner/exercise
COPY --from=build /src/PythonCompilerService/Exercise .

RUN chmod go-r /
RUN chown -R runner /home/runner/

WORKDIR /python-compiler
COPY --from=publish /python-compiler/publish .
ENTRYPOINT ["dotnet", "PythonCompilerService.dll"]
