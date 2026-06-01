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

- **v1.0.22**

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

`PropriedadeTipo(PropertyInfo)` mapeia propriedades de entidades para o enum `Tipo`:

| Tipo .NET | `Tipo` retornado |
|---|---|
| `string` | `STRING` |
| `int`, `long` | `INT` |
| `decimal` | `MONEY` |
| `double` | `DOUBLE` |
| `DateTime` | `DATA` |
| `DateOnly`, `DateOnly?` | `DATA` |
| `TimeSpan` | `HORA` |
| `TimeOnly`, `TimeOnly?` | `HORA` |
| `bool` | `BOOL` |
| `Guid` | `GUID` |
| `byte[]` | `BLOB` |
| `enum` | `ENUM` |

### Cripto
Criptografia simétrica com métodos de encriptação/desencriptação.

### Logger / LoggerYordi

`LoggerYordi` implementa `Microsoft.Extensions.Logging.ILogger` e oferece um conjunto completo de recursos de diagnóstico:

#### Método `Write()` — captura automática de origem
Prefira `Write()` ao `ILogger.Log<TState>()` quando o código chamar o logger diretamente.
A origem (método, linha e arquivo) é resolvida em **tempo de compilação** via `[CallerMemberName]`, sem custo de reflection:

```csharp
var log = LoggerYordi.LoggerInstance();
log.Write(LogLevel.Warning, "Algo suspeito aconteceu");
log.Write(LogLevel.Error, "Falha ao processar", exception);
// saída: [10/05/2026 12:06:44.327] [MeuMetodo:42] [WAR] Algo suspeito aconteceu
```

#### Logger tipado `LoggerYordi<T>` — equivalente ao `ILogger<T>`
Singleton por tipo com a categoria prefixada automaticamente em cada mensagem:

```csharp
// Obtém o logger tipado
var log = LoggerYordi.Instance<MinhaClasse>();
log.Write(LogLevel.Information, "Processando...");
// saída: [MinhaClasse.Processar:42] [INF] Processando...

// Compatível com ILogger<T> para injeção de dependência
ILogger<MinhaClasse> loggerDI = LoggerYordi.Instance<MinhaClasse>();
```

| | `LoggerYordi` | `LoggerYordi<T>` |
|---|---|---|
| Interface | `ILogger` | `ILogger<T>` |
| Instância | `LoggerYordi.LoggerInstance()` | `LoggerYordi.Instance<T>()` |
| Categoria na origem | não | sim — `NomeClasse.NomeMetodo` |
| Singleton | único global | um por tipo `T` |

#### Timestamp com milissegundos
Todas as mensagens incluem milissegundos no timestamp:
```
[10/05/2026 12:06:44.327] [MinhaClasse.MeuMetodo:42] [INF] Mensagem
```

#### Stack trace resumido em exceções
O caminho completo do arquivo é suprimido no log de exceções — apenas o nome do arquivo e a linha são exibidos:
```
// Antes
at Yordi.Tools.ConsoleApp.TesteLogger.TestarExcecao() in D:\...\TesteLogger.cs:line 72

// Depois
at TesteLogger.TestarExcecao() in TesteLogger.cs:line 72
```

#### Saída no depurador do Visual Studio
Todas as mensagens são enviadas via `Trace.WriteLine`/`Trace.Write`/`Trace.Fail`, garantindo visibilidade tanto em builds Debug quanto Release. Para exibir as mensagens na janela **Saída** do Visual Studio, adicione um listener no projeto consumidor:

```csharp
using System.Diagnostics;

Trace.Listeners.Add(new DefaultTraceListener());
```

#### `Logger` — API de arquivo
- `Logger.LogSync` / `Logger.LogAsync` para gravação em arquivo com rotação diária
- `Logger.MontaLinha` para formatar linhas sem gravação
- `Logger.Path` / `Logger.File` para configurar destino

### EventBaseClass
Classe base para publicação de eventos de mensagem, erro, exceção, quantidade de registros e progresso.
- Integra automaticamente com `LoggerYordi` ao ser instanciada
- Métodos protegidos: `Message()`, `Error(string)`, `Error(Exception)`, `Rows()`, `Progresso()`
- Eventos: `MessageEvent`, `ErroEvent`, `ExceptionEvent`, `ProgressValue`, `ProgressMax`
- `GetLogger()` / `SetLogger()` para substituição do logger em testes

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
- `UsarSQLiteWALMode` para habilitar WAL no SQLite.

### POCOs, atributos e índices
- Classes base: `CommonColumns`, `Basico`
- Atributos utilitários para metadados de entidade
- `IPOCOIndexes` para declarar índices de banco em entidades POCO
- `IPOCOIndexes.IndexInfo` para descrever nome, colunas, unicidade e chaves
- `Chave`/`IChave` para metadados de campos usados em cenários SQL

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

### v1.0.22 (atual)
- **Logger — serialização thread-safe via `Channel<T>`**: corrigido bug de mistura de entradas no arquivo de log quando múltiplas threads escreviam simultaneamente. Toda escrita (tanto `GraveSync` quanto `GraveAsync`) passa agora por um `Channel<string>` com consumidor único em background, eliminando interleaving de linhas sem bloquear as threads chamadoras.
- **`MontaNomeArquivoCompleto` thread-safe**: acesso ao nome do arquivo de log protegido por `lock` via método `ObterNomeArquivoCompleto()`, evitando condição de corrida na resolução do caminho entre threads concorrentes.

### v1.0.21
- **Logger — saída via `Trace`**: substituído `Debug.Write`/`Debug.WriteLine`/`Debug.Fail` por `Trace.*` e removida a guarda `if (Debugger.IsAttached)`. As mensagens agora são emitidas em builds Debug **e** Release, sem serem suprimidas pelo compilador ao publicar o pacote NuGet. Projetos consumidores devem registrar um `TraceListener` (ex.: `DefaultTraceListener`) para visualizar as mensagens.

### v1.0.20
- **`Conversores.PropriedadeTipo`**: adicionado suporte a `TimeOnly` e `TimeOnly?` (→ `Tipo.HORA`) e `DateOnly` e `DateOnly?` (→ `Tipo.DATA`).

### v1.0.19
- **Logger — método `Write()`**: captura automática de origem (método, linha, arquivo) via `[CallerMemberName]`/`[CallerLineNumber]`/`[CallerFilePath]` sem custo de reflection.
- **Logger tipado `LoggerYordi<T>`**: implementa `ILogger<T>`; singleton por tipo; categoria prefixada automaticamente na origem (`NomeClasse.NomeMetodo`); obtido via `LoggerYordi.Instance<T>()`.
- **Timestamp com milissegundos**: formato `dd/MM/yyyy HH:mm:ss.fff` em todas as mensagens.
- **Stack trace resumido**: exceções registradas exibem apenas `NomeArquivo.cs:line N` em vez do caminho completo, tanto no arquivo quanto no console/depurador.
- **Correção `[:0]`**: suprimido o bloco `[origem:linha]` quando origem e linha estão vazios/zero.
- **`ILogger.Log<TState>()` com origem via `StackFrame`**: quando chamado pelo framework ou via interface, a origem é capturada em runtime.

### v1.0.18.4
- Ajustes incrementais e estabilização da linha 1.0.18.
- Mensagens de log publicadas no console/debug para visibilidade imediata, além do arquivo.

### v1.0.18
- Adição: `UsarSQLiteWALMode` em `DBConfig`.
- Adição: novos recursos de identificação/classificação de IPs e adaptadores em `Rede`.

### v1.0.17
- Correção: excesso de nova linha ao escrever logs em console/debug.

### v1.0.16
- **Breaking change** no logging:
  - `LogAsync(Exception, ...)` → `Task<string?>`
  - `LogAsync(string, ...)` → `Task<string?>`
  - `LogSync(Exception, ...)` → `string?`
  - `LogSync(string, ...)` → `string?`

### v1.0.14
- Adição: `IPOCOIndexes`, `IPOCOIndexes.IndexInfo`, `Chave`.

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
