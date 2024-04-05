FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /compiler
EXPOSE 3300

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CompilerService/CompilerService.csproj", "CompilerService/"]
RUN dotnet restore "CompilerService/CompilerService.csproj"
COPY . .
WORKDIR "/src/CompilerService"
RUN dotnet build "CompilerService.csproj" -c {MODE} -o /compiler/build

FROM build AS publish
RUN dotnet publish "CompilerService.csproj" -c {MODE} -o /compiler/publish /p:UseAppHost=false

FROM base AS final

#RUN apt-get update -y
#RUN apt install firejail -y --no-install-recommends --no-install-suggests

RUN adduser runner --disabled-password --gecos ""

WORKDIR /home/runner/exercise
COPY --from=build /src/CompilerService/Exercise .

RUN chmod go-r /
RUN chown -R runner /home/runner/


WORKDIR /compiler
COPY --from=publish /compiler/publish .

ENTRYPOINT ["dotnet", "CompilerService.dll"]