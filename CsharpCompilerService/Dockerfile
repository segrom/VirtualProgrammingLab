FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /compiler
EXPOSE 3300

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CsharpCompilerService/CsharpCompilerService.csproj", "CsharpCompilerService/"]
RUN dotnet restore "CsharpCompilerService/CsharpCompilerService.csproj"
COPY . .
WORKDIR "/src/CsharpCompilerService"
RUN dotnet build "CsharpCompilerService.csproj" -c "$MODE" -o /compiler/build

FROM build AS publish
RUN dotnet publish "CsharpCompilerService.csproj" -c "$MODE" -o /compiler/publish /p:UseAppHost=false

FROM base AS final

#RUN apt-get update -y
#RUN apt install firejail -y --no-install-recommends --no-install-suggests

RUN adduser runner --disabled-password --gecos ""

WORKDIR /home/runner/exercise
COPY --from=build /src/CsharpCompilerService/Exercise .

RUN chmod go-r /
RUN chown -R runner /home/runner/


WORKDIR /compiler
COPY --from=publish /compiler/publish .

ENTRYPOINT ["dotnet", "CsharpCompilerService.dll"]