# CloudExplorer API

Este projeto é uma API Rest ASP.NET Core para manipulação de arquivos e diretórios utilizando FluentStorage. Os endpoints implementam operações compatíveis com a interface TypeScript IDataService, facilitando integração com aplicações Angular ou similares.

## Funcionalidades
- Listagem de arquivos e diretórios
- Criação, renomeação e exclusão de diretórios
- Upload e download de arquivos
- Retorno de árvore de diretórios

## Como executar

1. Instale o .NET 9 SDK
2. Execute:
   ```powershell
   dotnet run
   ```
3. Acesse a documentação Swagger em `https://localhost:5001/swagger`

## Dependências
- ASP.NET Core
- FluentStorage

## Estrutura inicial
- Controllers
- Models
- Integração básica com FluentStorage

## Personalização
Adapte os endpoints conforme sua necessidade para atender a interface do frontend.
