FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY Core.Application/Core.Application.csproj Core.Application/
COPY Core.Data/Core.Data.csproj Core.Data/
COPY Core.WebApi/Core.WebApi.csproj Core.WebApi/
RUN dotnet restore Core.WebApi/Core.WebApi.csproj
COPY . .
RUN dotnet publish Core.WebApi/Core.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Core.WebApi.dll"]
