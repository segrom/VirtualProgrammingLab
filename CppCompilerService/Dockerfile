FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /cpp-compiler
EXPOSE 3300

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CppCompilerService/CppCompilerService.csproj", "CppCompilerService/"]
RUN dotnet restore "CppCompilerService/CppCompilerService.csproj"
COPY . .
WORKDIR "/src/CppCompilerService"
RUN dotnet build "CppCompilerService.csproj" -c "$MODE" -o /cpp-compiler/build

FROM build AS publish
RUN dotnet publish "CppCompilerService.csproj" -c "$MODE" -o /cpp-compiler/publish /p:UseAppHost=false

FROM base AS final

RUN apt-get update -y
RUN apt install build-essential -y
RUN apt-get install manpages-dev -y

RUN adduser runner --disabled-password --gecos ""

WORKDIR /home/runner/exercise
COPY --from=build /src/CppCompilerService/Exercise .

RUN chmod go-r /
RUN chown -R runner /home/runner/

WORKDIR /cpp-compiler
COPY --from=publish /cpp-compiler/publish .
ENTRYPOINT ["dotnet", "CppCompilerService.dll"]
