# --- Stage 1: Build da aplicação ---
# Usa a imagem do SDK do .NET para compilar a aplicação.
# Esta imagem é grande, mas é só usada para a etapa de build.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copia o arquivo .csproj e restaura as dependências.
# Isso aproveita o cache do Docker, acelerando builds subsequentes.
COPY RinhaBackend2025.csproj ./
RUN dotnet restore

# Copia o restante do código-fonte e publica a aplicação.
COPY . ./
RUN dotnet publish -c Release -o out

# --- Stage 2: Criação da imagem de runtime ---
# Usa uma imagem menor, que contém apenas o runtime necessário.
# A aplicação final será baseada nesta imagem.
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copia os arquivos da aplicação compilada do estágio de build.
COPY --from=build-env /app/out .

# Expondo a porta 8080, que é a porta padrão do ASP.NET Core.
EXPOSE 8080

# Comando para iniciar a aplicação quando o container é executado.
ENTRYPOINT ["dotnet", "RinhaBackend2025.dll"]
