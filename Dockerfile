# Use the ASP.NET base image for .NET 7.0
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install libgdiplus and copy your data (appsettings.json?)
RUN apt-get update && apt-get install -y -v libgdiplus && apt-get clean
COPY data/ /app/

# Use the .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FeedbackService.csproj", "./"]
RUN dotnet restore "FeedbackService.csproj"
COPY . .
RUN dotnet build "FeedbackService.csproj" -c Release -o /app/build

# Publish your application
FROM build AS publish
RUN dotnet publish "FeedbackService.csproj" -c Release -o /app/publish

# Create the final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set your application's entry point
ENTRYPOINT ["dotnet", "FeedbackService.dll"]
