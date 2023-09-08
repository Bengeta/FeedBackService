FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
COPY data .
EXPOSE 80
EXPOSE 443


RUN apt-get -y update
RUN apt  install -y -v libgdiplus
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src
COPY ["FeedBackService.csproj", "./"]
RUN dotnet restore "FeedBackService.csproj"
WORKDIR "/src"
COPY . .
RUN dotnet build "FeedBackService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FeedBackService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "FeedBackService.dll"]