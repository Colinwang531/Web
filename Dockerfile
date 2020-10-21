#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
#ENV LANG=zh_CN.utf8
ADD ./simsun.ttc /usr/share/fonts/simsun.ttc
#ADD ./simsun.ttc /usr/share/fonts/Fonts/simsun.ttc

WORKDIR /app
COPY . .
RUN mv /etc/apt/sources.list /etc/apt/sources.list.bak && mv sources.list /etc/apt/ && apt-get update -y && apt-get install -y libgdiplus && apt-get clean && apt-get install sox -y && ln -s /usr/lib/libgdiplus.so /usr/lib/gdiplus.dll 

EXPOSE 80
EXPOSE 443
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build

WORKDIR /src
COPY ["SmartWeb.csproj", ""]
COPY . .
WORKDIR "/src/."
RUN dotnet build "SmartWeb.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SmartWeb.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SmartWeb.dll"]
