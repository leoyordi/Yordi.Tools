# Yordi.Tools

![NuGet](https://img.shields.io/nuget/v/Yordi.Tools)
![License](https://img.shields.io/github/license/leoyordi/Yordi.Tools)

Biblioteca utilitária para .NET 8 com foco em produtividade para aplicações de negócio: arquivos, conversões, validações, eventos, logging, criptografia, rede e suporte a POCOs.

## Instalação

```bash
dotnet add package Yordi.Tools
```

Ou:

```powershell
Install-Package Yordi.Tools
```

## Versão atual

- **v1.0.18.3**

## Projetos da solução

- **Yordi.Tools**: biblioteca principal (publicada no NuGet)
- **Yordi.Tools.ConsoleApp**: app de apoio para testes/exemplos (não empacotado no NuGet)

## Requisitos

- .NET 8
- Microsoft.Extensions.Logging 8.0.1
- System.Management 8.0.0

---

## Principais componentes

### ValidaObjetos
Validações de e-mail, CPF, CNPJ, telefone, IP/URL, MAC, inteiros, doubles e enums.

### FileTools
Operações com arquivos e diretórios:
- leitura/escrita síncrona e assíncrona
- detecção de encoding
- utilitários de caminho, extensão e existência
- listagem, exclusão e movimentação de arquivos

### FileRepository<T>
Persistência JSON em arquivo com suporte a leitura/escrita assíncrona e integração com `EventBaseClass`.

### Conversores
Conversões de tipos primitivos, datas, strings, JSON (`ToJson`/`FromJson`) e helpers de texto.

### Cripto
Criptografia simétrica com métodos de encriptação/desencriptação.

### Logger / LoggerYordi
- `LoggerYordi` implementa `Microsoft.Extensions.Logging.ILogger`
- `Logger` oferece API direta de gravação em arquivo
- desde a linha **1.0.16**, métodos de log retornam `string?`/`Task<string?>` para permitir tratamento de falha

### EventBaseClass
Classe base para publicação de eventos de mensagem, erro, exceção, quantidade de registros e progresso.

### NewGuid (GuidSequence)
Geração de GUID sequencial para cenários MySQL, Oracle e SQL Server.

### DataPadrao
Datas utilitárias com foco em horário de Brasília e formato para SQL.

### Rede
Recursos de rede clássicos e avançados:
- IP externo e MAC
- resolução de host para IP
- inventário de interfaces e IPs ativos
- classificação de rede (`TipoRede`: Loopback, LinkLocal, RedeLocal, VPN, Web)
- seleção de melhor IP para publicação de serviço
- resumo textual de rede

### DBConfig
Configuração para conexão de banco (MySQL/SQLite), com montagem de connection string e opções de reconexão.

**Novo na linha 1.0.18:**
- `UsarSQLiteWALMode` para habilitar WAL no SQLite.

### POCOs, atributos e índices
- Classes base: `CommonColumns`, `Basico`
- Atributos utilitários para metadados de entidade
- `IPOCOIndexes` para declarar índices de banco em entidades POCO
- `IPOCOIndexes.IndexInfo` para descrever nome, colunas, unicidade e chaves
- `Chave`/`IChave` para metadados de campos usados em cenários SQL

Exemplo de assinatura:

```csharp
public class Produto : Basico, IPOCOIndexes
{
    public IEnumerable<IPOCOIndexes.IndexInfo> GetIndexes()
    {
        return new[]
        {
            new IPOCOIndexes.IndexInfo
            {
                IndexName = "IX_Produto_Codigo",
                Columns = new List<string> { "Codigo" },
                IsUnique = true
            }
        };
    }
}
```

---

## Interfaces importantes

- `ICommonColumns`
- `IAuto`
- `IDescricao`
- `IObjectStringIndexer`
- `IPropertyType`
- `IPOCOtoDB`
- `IPOCOIndexes`
- `IChave`

---

## Changelog

### v1.0.18.3 (atual)
- Ajustes incrementais e estabilização da linha 1.0.18.

### v1.0.18
- Adição: `UsarSQLiteWALMode` em `DBConfig`.
- Adição: novos recursos de identificação/classificação de IPs e adaptadores em `Rede`.

### v1.0.17
- Correção: excesso de nova linha ao escrever logs em console/debug.

### v1.0.16
- **Breaking change** no logging:
  - `LogAsync(Exception, ...)` -> `Task<string?>`
  - `LogAsync(string, ...)` -> `Task<string?>`
  - `LogSync(Exception, ...)` -> `string?`
  - `LogSync(string, ...)` -> `string?`
- Recomendado informar `origem`, `line` e `file` para contexto completo de log.

### v1.0.15 (preterida)
- Versão preterida por erro de projeto. Substituída pela v1.0.16.

### v1.0.14
- Adição: `IPOCOIndexes`.
- Adição: `IPOCOIndexes.IndexInfo`.
- Adição: `Chave` para cenários SQL/índices.

### v1.0.13 (preterida)
- Versão preterida por ajustes posteriores de design/documentação.

### v1.0.11
- Correção em `Conversores.FromJson` com `AssemblyQualifiedName` string.

### v1.0.10
- Correção na classe `Cripto` (reuso de instância/chave).
- Inclusão do projeto console para testes.

### v1.0.9
- Adição de `FileRepository<T>`.

---

## Contribuição

1. Fork do repositório
2. Crie sua branch (`git checkout -b feature/minha-feature`)
3. Commit (`git commit -m "Minha alteração"`)
4. Push (`git push origin feature/minha-feature`)
5. Abra um Pull Request

---

## Licença

Licença **MIT**.

---

## Autor

**Leopoldo Yordi**

- GitHub: [@leoyordi](https://github.com/leoyordi)
- Repositório: [Yordi.Tools](https://github.com/leoyordi/Yordi.Tools)

---

## Links

- NuGet: https://www.nuget.org/packages/Yordi.Tools
- GitHub: https://github.com/leoyordi/Yordi.Tools
