FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
RUN ln -s /lib/x86_64-linux-gnu/libdl.so.2 /lib/x86_64-linux-gnu/libdl.so \
  && apt update \
  && apt install -y libgdiplus \
  && ln -s /usr/lib/libgdiplus.so /lib/x86_64-linux-gnu/libgdiplus.so

WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG CONFIGURATION
WORKDIR /src
COPY ["WebHost/WebHost.csproj", "WebHost/"]
COPY ["Keycloak/Keycloak.csproj", "Keycloak/"]
COPY ["DAL/DAL.csproj", "DAL/"]
COPY ["Shared/Shared.csproj", "Shared/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["BLL/BLL.csproj", "BLL/"]
RUN dotnet restore "WebHost/WebHost.csproj"
COPY . .
WORKDIR "/src/WebHost"
RUN dotnet build "WebHost.csproj" -c $CONFIGURATION -o /app

FROM build AS publish
ARG CONFIGURATION
RUN dotnet publish "WebHost.csproj" -c $CONFIGURATION -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

RUN useradd -d /home/houguildbotuser -m -s /bin/bash houguildbotuser
USER houguildbotuser

ENTRYPOINT ["dotnet", "guildbot.dll"]