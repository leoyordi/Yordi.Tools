# Yordi.Tools

![NuGet](https://img.shields.io/nuget/v/Yordi.Tools) ![License](https://img.shields.io/github/license/leoyordi/Yordi.Tools)

**Yordi.Tools** é uma biblioteca .NET 8 com ferramentas essenciais para desenvolvimento de sistemas, oferecendo funcionalidades prontas para trabalhar com arquivos, validações, conversões, criptografia, logging, rede e muito mais.

## 📦 Instalação

Instale o pacote via NuGet Package Manager:

```bash
dotnet add package Yordi.Tools
```

Ou via Package Manager Console:

```powershell
Install-Package Yordi.Tools
```

## 🎯 Sobre a Solução

Esta solução é composta por:

- **Yordi.Tools**: Biblioteca principal com todas as ferramentas e utilitários
- **Yordi.Tools.ConsoleApp**: Aplicação console para testes e exemplos (não incluída no pacote NuGet)

### Requisitos

- .NET 8.0 ou superior
- Microsoft.Extensions.Logging 8.0.1
- System.Management 8.0.0 (para funcionalidades de rede no Windows)

---

## 🛠️ Componentes Principais

### 1. ValidaObjetos
Classe para validação de diferentes tipos de dados comuns no Brasil e no mundo.

**Métodos disponíveis:**
- `IsEmail(string email)`: Valida endereço de e-mail
- `IsCPF(string cpf)`: Valida CPF brasileiro
- `IsCNPJ(string cnpj)`: Valida CNPJ brasileiro
- `IsFone(string fone)`: Valida número de telefone
- `IsCircuito(string circuito)`: Valida circuito de rede
- `IsMacAddress(string macAddress)`: Valida endereço MAC
- `IsIPv4(string value)`: Valida endereço IPv4
- `IsIP(string value, out IPAddress? address)`: Valida endereço IP e retorna o objeto IPAddress
- `IsURL(string value)`: Valida URL
- `IsIPorURL(string value)`: Valida IP ou URL
- `IsInt(string value, out int valor)`: Valida e converte para inteiro
- `IsDouble(string value, out double valor)`: Valida e converte para double
- `IsEnumValid(Enum enumValue, int value, int minValue, int maxValue)`: Valida valor de enum

**Exemplo:**
```csharp
using Yordi.Tools;

bool emailValido = ValidaObjetos.IsEmail("usuario@example.com");
bool cpfValido = ValidaObjetos.IsCPF("123.456.789-09");

if (ValidaObjetos.IsInt("123", out int numero))
{
    Console.WriteLine($"Número válido: {numero}");
}
```

---

### 2. FileTools
Classe com operações avançadas para trabalhar com arquivos e diretórios.

**Principais métodos:**

**Leitura:**
- `ReadAllText(string? filePath)`: Lê todo o conteúdo de um arquivo
- `ReadAllTextAsync(string? filePath, Encoding? encoding = null)`: Leitura assíncrona
- `ReadAllLines(string? filePath, Encoding? encoding = null)`: Lê todas as linhas
- `DetectFileEncoding(string? filename, Encoding? defaultEncoding = null)`: Detecta codificação

**Escrita:**
- `WriteText(string? filePath, string text, Encoding? encoding = null, bool replace = false)`: Escreve texto
- `WriteTextAsync(string? filePath, string text, Encoding? encoding = null)`: Escrita assíncrona
- `WriteAllBytes(string? filePath, byte[] bytes, bool replace = false)`: Escreve bytes

**Manipulação:**
- `NomeArquivo(string? filePath)`: Retorna nome do arquivo
- `PastaSomente(string? nomeArquivoCompleto)`: Retorna apenas o caminho da pasta
- `NomeArquivoSemExtensao(string? filePath)`: Retorna nome sem extensão
- `Extensao(string? filePath)`: Retorna a extensão
- `Combina(string? pasta, string? arquivo)`: Combina pasta e arquivo
- `CriaDiretorio(string? path)`: Cria diretório
- `Excluir(string? arquivo)`: Exclui arquivo
- `Mover(string? arquivoOrigem, string? arquivoDestino)`: Move arquivo

**Utilitários:**
- `ArquivoExiste(string? arquivo)`: Verifica existência
- `PastaExiste(string? pasta)`: Verifica existência de pasta
- `PastaTemporaria()`: Retorna caminho da pasta temporária
- `DataCriacao(string? arq)`: Data de criação
- `DataAtualizacao(string? arq)`: Data de modificação
- `Arquivos(string? pasta, string? criterio = "*.log", bool? topDirectoryOnly = true)`: Lista arquivos
- `Excluir(string pasta, string extensao, DateTime olderThan)`: Exclui arquivos antigos

**Exemplo:**
```csharp
using Yordi.Tools;

// Ler arquivo
string conteudo = FileTools.ReadAllText("config.txt");

// Escrever arquivo
FileTools.WriteText("log.txt", "Mensagem de log", replace: false);

// Listar arquivos
var arquivos = FileTools.Arquivos(@"C:\Logs", "*.log");
```

---

### 3. FileRepository<T>
Repositório genérico para persistência de objetos em arquivos JSON.

**Características:**
- Serialização/deserializaçã automática para JSON
- Suporte a leitura e escrita assíncrona
- Herda de `EventBaseClass` para notificações de eventos
- Suporte a encoding personalizável
- Substituição automática de placeholders (%TEMP%, DATA)

**Métodos:**
- `Salvar(T objeto)`: Salva objeto no arquivo
- `Ler()`: Lê e deserializa o objeto
- `LerAsync()`: Leitura assíncrona
- `LerComoTexto()`: Lê como texto puro
- `LerLinhas(string arquivo)`: Lê em formato de linhas
- `Escrever(string texto)`: Escreve texto no arquivo
- `Escrever(string texto, string arquivo)`: Escrita assíncrona

**Exemplo 1 - Configurações simples:**
```csharp
using System.Text; 
using Yordi.Tools;

public class Configuracao
{
    public string Usuario { get; set; }
    public string Servidor { get; set; }
    public int Porta { get; set; }
}

public class Program 
{ 
    public static void Main() 
    { 
        var repo = new FileRepository<Configuracao>("config.json", Encoding.UTF8);
        
        // Salvar
        var config = new Configuracao 
        { 
            Usuario = "admin", 
            Servidor = "localhost",
            Porta = 8080
        };
        repo.Salvar(config);
        
        // Ler
        var configLida = repo.Ler();
        Console.WriteLine($"Servidor: {configLida.Servidor}");
    }
}
```

**Exemplo 2 - Lista de objetos com singleton:**
```csharp
using System.Text;
using Yordi.Tools;

public class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
}

public class ClienteRepository : FileRepository<List<Cliente>>
{
    private static ClienteRepository? _instance;
    
    private ClienteRepository(string path) : base(path, Encoding.UTF8) { }
    
    public static ClienteRepository Instance => 
        _instance ??= new ClienteRepository("clientes.json");

    public bool AdicionarCliente(Cliente cliente)
    {
        var lista = Ler() ?? new List<Cliente>();
        lista.Add(cliente);
        return base.Salvar(lista);
    }
    
    public Cliente? BuscarPorId(int id)
    {
        var lista = Ler();
        return lista?.FirstOrDefault(c => c.Id == id);
    }
}
```

---

### 4. Conversores
Classe com métodos de conversão entre tipos de dados e formatos.

**Conversão de Tipos:**
- `ToDataHora(string/object)`: Converte para DateTime
- `DateToString(DateTime)`: Converte DateTime para string (formato: yyyy-MM-dd HH:mm:ss)
- `ToDecimal(string/object)`: Converte para decimal
- `ToDouble(string/object)`: Converte para double
- `ToInt(string/object/bool)`: Converte para int
- `ToBool(int/string/object)`: Converte para bool
- `ToVersion(string)`: Converte para Version

**Manipulação de Strings:**
- `Right(string value, int size)`: Retorna últimos caracteres
- `SubstituiDiacritico(string)`: Remove acentos
- `RemoveCaracteresEspeciais(string)`: Remove caracteres especiais
- `RemovePontos(string)`: Remove pontos
- `RetornaNumeros(string)`: Retorna apenas números

**JSON:**
- `ToJson<T>(T obj, bool writeIndent = false)`: Serializa para JSON
- `ToJsonUtf8(object)`: Serializa para JSON em bytes UTF-8
- `FromJson<T>(string/byte[])`: Deserializa de JSON
- `FromJson(string/byte[], Type)`: Deserializa para tipo específico

**Exemplo:**
```csharp
using Yordi.Tools;

// Conversões
int numero = Conversores.ToInt("123");
bool ativo = Conversores.ToBool("true");
DateTime data = Conversores.ToDataHora("2024-01-15");

// Manipulação de strings
string texto = "João José";
string semAcento = texto.SubstituiDiacritico(); // "Joao Jose"
string apenasNumeros = Conversores.RetornaNumeros("ABC123XYZ"); // "123"

// JSON
var objeto = new { Nome = "João", Idade = 30 };
string json = Conversores.ToJson(objeto, writeIndent: true);
var objetoRecuperado = Conversores.FromJson<dynamic>(json);
```

---

### 5. Cripto
Classe para criptografia simétrica de textos.

**Métodos:**
- `GetKey()`: Gera chave de criptografia válida
- `Encrypt(string texto)`: Criptografa texto
- `Decrypt(string textoCriptografado)`: Descriptografa texto

**Exemplo:**
```csharp
using Yordi.Tools;

var cripto = new Cripto("minhaChaveSecreta123");

string textoOriginal = "Senha123";
string textoCriptografado = cripto.Encrypt(textoOriginal);
string textoDescriptografado = cripto.Decrypt(textoCriptografado);

Console.WriteLine($"Original: {textoOriginal}");
Console.WriteLine($"Criptografado: {textoCriptografado}");
Console.WriteLine($"Descriptografado: {textoDescriptografado}");
```

---

### 6. LoggerYordi
Sistema de logging integrado com Microsoft.Extensions.Logging.

**Observação importante (alteração de comportamento / breaking change)**

Nas versões recentes houve mudanças na implementação dos logs:

- As APIs de log em `Yordi.Tools.Logger` foram atualizadas para retornar valores informativos em vez de serem apenas `void`/fire-and-forget. Em particular:
  - `LogAsync(Exception filterContext, string origem = "", int line = 0, string file = "")` agora retorna `Task<string?>` com a linha de log escrita quando a gravação for bem-sucedida, ou `null` em caso de falha.
  - `LogAsync(string texto, string origem = "", int line = 0, string file = "")` agora retorna `Task<string?>` com a linha de log escrita quando a gravação for bem-sucedida, ou `null` em caso de falha.
  - `LogSync(Exception filterContext, string origem = "", int line = 0, string file = "")` agora retorna `string?` com a linha de log escrita ou `null` em caso de falha.
  - `LogSync(string texto, string origem = "", int line = 0, string file = "")` agora retorna `string?` com a linha de log escrita ou `null` em caso de falha.

- A classe `LoggerYordi` (implementação de `Microsoft.Extensions.Logging.ILogger`) continua a implementar `void Log<TState>(...)` conforme a interface, porém internamente passa as entradas para `Logger.LogSync`. Como `Logger.LogSync` agora possui retorno (`string?`), houve alteração de comportamento interno — consumidores que dependiam exclusivamente de efeitos colaterais sem tratamento de retorno devem considerar verificar os novos retornos quando usarem diretamente `Yordi.Tools.Logger`.

- Além disso, agora é recomendado que chamados às APIs de log sejam explícitos quanto aos parâmetros de origem, linha e arquivo (`origem`, `line`, `file`) para garantir que o log contenha contexto útil. Apesar de existirem valores padrão (cadeia vazia / zero), a nova prática é sempre informar explicitamente essas informações para evitar logs sem contexto.

Possível quebra de código (breaking change)

- Se o seu código dependia do comportamento anterior em que as APIs de log eram `void` e nunca retornavam valor, pode haver impacto: agora métodos públicos retornam `string?` ou `Task<string?>`. Atualize chamadas para tratar os retornos (verificar `null` em caso de falha) ou continue a ignorar o retorno conscientemente.

- Exemplo de atualização necessária:

```csharp
// Antes (antigo behavior fire-and-forget)
Yordi.Tools.Logger.LogSync("Aplicação iniciada");

// Agora (tratar retorno)
var resultado = Yordi.Tools.Logger.LogSync("Aplicação iniciada", origem: "Program", line: 42, file: nameof(Program));
if (resultado == null)
{
    // Tratar falha ao gravar log
}
```

- Exemplo assíncrono:

```csharp
var resultadoAsync = await Yordi.Tools.Logger.LogAsync(new Exception("Erro X"), origem: "Servico", line: 123, file: "Servico.cs");
if (resultadoAsync == null)
{
    // Tratar falha
}
else
{
    Console.WriteLine(resultadoAsync); // linha completa do log gerada
}
```

Recomenda-se revisar chamadas que façam logging em massa ou que assumam sucesso silencioso, pois agora é possível detectar falhas de gravação retornadas pelas novas APIs.

---

### 7. EventBaseClass
Classe base para implementação de eventos customizados em suas classes.

**Eventos disponíveis:**
- `OnMessage`: Evento de mensagem informativa
- `OnError`: Evento de erro
- `OnException`: Evento de exceção
- `OnRows`: Evento de quantidade de registros
- `OnProgresso`: Evento de progresso

**Métodos:**
- `Message(string mensagem, string origem = "", int line = 0, string path = "")`: Dispara evento de mensagem
- `Error(string mensagem, string origem = "", int line = 0, string path = "")`: Dispara evento de erro
- `Error(Exception e, string origem = "", int line = 0, string path = "")`: Dispara evento de exceção
- `Rows(float registros)`: Dispara evento de registros
- `Progresso(float progresso)`: Dispara evento de progresso

**Exemplo:**
```csharp
using Yordi.Tools;

public class MeuProcessador : EventBaseClass
{
    public void ProcessarDados()
    {
        Message("Iniciando processamento...");
        
        try
        {
            for (int i = 0; i < 100; i++)
            {
                // Processar...
                Progresso(i / 100f);
            }
            
            Rows(100);
            Message("Processamento concluído");
        }
        catch (Exception ex)
        {
            Error(ex);
        }
    }
}

// Uso
var processador = new MeuProcessador();
processador.OnMessage += (msg, origem, line, path) => 
    Console.WriteLine($"[INFO] {msg}");
processador.OnProgresso += (prog) => 
    Console.WriteLine($"Progresso: {prog:P0}");
processador.ProcessarDados();
```

---

### 8. NewGuid (GuidSequence)
Gerador de GUIDs sequenciais otimizados para diferentes bancos de dados.

**Características:**
- GUIDs sequenciais melhoram performance de índices
- Suporte para MySQL, Oracle e SQL Server
- Baseado em timestamp para ordenação

**Tipos:**
- `TipoGuid.MySQL`: Ordem alfabética
- `TipoGuid.Oracle`: Ordem binária
- `TipoGuid.MSSQL`: Ordem baseada nos últimos 6 bytes (padrão)

**Exemplo:**
```csharp
using Yordi.Tools;

// Configurar para SQL Server (padrão)
NewGuid.TipoGuid = TipoGuid.MSSQL;
Guid guid1 = NewGuid.NewSequentialGuid();

// Configurar para MySQL
NewGuid.TipoGuid = TipoGuid.MySQL;
Guid guid2 = NewGuid.NewSequentialGuid();

Console.WriteLine($"GUID SQL Server: {guid1}");
Console.WriteLine($"GUID MySQL: {guid2}");
```

---

### 9. DataPadrao
Classe para manipulação de datas com timezone de Brasília.

**Propriedades:**
- `Brasilia`: Data/hora atual de Brasília
- `Maquina`: Data/hora atual da máquina
- `DataBrasiliaToMSSQL`: Data/hora de Brasília formatada para SQL Server
- `MinValue`: Data mínima compatível com MySQL

**Exemplo:**
```csharp
using Yordi.Tools;

DateTime agora = DataPadrao.Brasilia;
DateTime agoraMaquina = DataPadrao.Maquina;
string dataSQL = DataPadrao.DataBrasiliaToMSSQL;

Console.WriteLine($"Brasília: {agora}");
Console.WriteLine($"SQL Server: {dataSQL}");
```

---

### 10. Rede
Classe com utilitários para trabalhar com redes e endereços.

**Métodos:**
- `IP()`: Retorna IP externo (async)
- `MeuMACAddress(IPAddress ip)`: Retorna endereço MAC
- `MeuMacAddressForWindows()`: Retorna MACs físicos (Windows)
- `MeuMacAddressForWindows(IPAddress ip)`: Retorna MAC específico (Windows)
- `GetIP(string host)`: Resolve hostname para IP

**Exemplo:**
```csharp
using Yordi.Tools;
using System.Net;

// IP externo
string? ipExterno = await Rede.IP();
Console.WriteLine($"Meu IP: {ipExterno}");

// MAC Address
var ip = IPAddress.Parse("192.168.1.100");
string? mac = Rede.MeuMACAddress(ip);

// Resolver hostname
IPAddress? ipResolvido = Rede.GetIP("www.google.com");
```

---

### 11. POCObase e Atributos
Classes base e atributos para trabalhar com entidades e mapeamento objeto-relacional.

**Classes Base:**
- `CommonColumns`: Colunas comuns (DataInclusao, DataAlteracao, Usuario, Origem)
- `Basico`: Entidade básica com Auto (ID) e Descricao

**Atributos disponíveis:**
- `[AutoIncrement]`: Campo auto-incremento
- `[BDIgnorar]`: Ignorar no banco de dados
- `[Exibir]`: DisplayMember para controles
- `[Valor]`: ValueMember para controles
- `[AutoUpdateDate]`: Atualização automática de data
- `[AutoInsertDate]`: Data de inserção automática
- `[OnlyInsert]`: Apenas para inserção
- `[OnlyUpdate]`: Apenas para atualização
- `[Tamanho("(100)")]`: Define tamanho de campo
- `[POCOtoDB(POCOType.CADASTRO)]`: Marca tipo de entidade

**Exemplo:**
```csharp
using Yordi.Tools;

[POCOtoDB(POCOType.CADASTRO)]
public class Produto : Basico
{
    [Tamanho("(100)")]
    public string? CodigoBarras { get; set; }
    
    public decimal Preco { get; set; }
    
    public int Estoque { get; set; }
    
    [BDIgnorar]
    public decimal PrecoComDesconto => Preco * 0.9m;
}

// Uso
var produto = new Produto 
{ 
    Auto = 1,
    Descricao = "Notebook",
    CodigoBarras = "7891234567890",
    Preco = 3000.00m,
    Estoque = 10
};

Console.WriteLine(produto.ToString()); // [1] Notebook
```

---

### 12. IPOCOIndexes - Definição de Índices para Banco de Dados

Interface que permite classes POCO especificarem índices que devem ser criados no banco de dados.

**Interface:**
```csharp
public interface IPOCOIndexes
{
    IEnumerable<IndexInfo> GetIndexes();
}
```

**Classe IndexInfo:**
```csharp
public class IndexInfo
{
    public string IndexName { get; set; }        // Nome do índice
    public List<string> Columns { get; set; }    // Colunas do índice
    public bool IsUnique { get; set; }           // Se é UNIQUE INDEX
    public IEnumerable<Chave> Chaves { get; set; } // Chaves detalhadas para cláusulas WHERE se necessárias
}
```

**Quando usar:**
- Quando sua entidade precisa de índices personalizados no banco de dados
- Para otimização de consultas frequentes
- Para garantir unicidade de combinações de campos
- Para índices compostos (múltiplas colunas)

**Exemplo:**
```csharp
using Yordi.Tools;

[POCOtoDB(POCOType.CADASTRO)]
public class Produto : Basico, IPOCOIndexes
{
    public string? CodigoBarras { get; set; }
    public string? Fabricante { get; set; }
    public string? Categoria { get; set; }
    public decimal Preco { get; set; }

    public IEnumerable<IPOCOIndexes.IndexInfo> GetIndexes()
    {
        return new List<IPOCOIndexes.IndexInfo>
        {
            // Índice único para código de barras
            new IPOCOIndexes.IndexInfo
            {
                IndexName = "IX_Produto_CodigoBarras",
                Columns = new List<string> { "CodigoBarras" },
                IsUnique = true
            },
            
            // Índice composto para consultas por fabricante e categoria
            new IPOCOIndexes.IndexInfo
            {
                IndexName = "IX_Produto_Fabricante_Categoria",
                Columns = new List<string> { "Fabricante", "Categoria" },
                IsUnique = false
            },
            
            // Índice com chaves detalhadas. Define cláusula WHERE para otimização
            new IPOCOIndexes.IndexInfo
            {
                IndexName = "IX_Produto_Preco",
                Columns = new List<string> { "Preco" },
                IsUnique = false,
                Chaves = new List<Chave>
                {
                    new Chave
                    {
                        Campo = "Preco",
                        Tipo = Tipo.MONEY,
                        Operador = Operador.MAIORque
                    }
                }
            }
        };
    }
}
```

**Cenários práticos:**

1. **E-commerce - Busca rápida de produtos:**
```csharp
public class Produto : Basico, IPOCOIndexes
{
    public string? SKU { get; set; }
    public string? Nome { get; set; }
    public decimal Preco { get; set; }
    
    public IEnumerable<IPOCOIndexes.IndexInfo> GetIndexes()
    {
        return new[]
        {
            new IPOCOIndexes.IndexInfo
            {
                IndexName = "IX_Produto_SKU",
                Columns = new List<string> { "SKU" },
                IsUnique = true
            },
            new IPOCOIndexes.IndexInfo
            {
                IndexName = "IX_Produto_Nome_Preco",
                Columns = new List<string> { "Nome", "Preco" }
            }
        };
    }
}
```

2. **Sistema de usuários - Email único:**
```csharp
public class Usuario : Basico, IPOCOIndexes
{
    public string? Email { get; set; }
    public string? CPF { get; set; }
    
    public IEnumerable<IPOCOIndexes.IndexInfo> GetIndexes()
    {
        return new[]
        {
            new IPOCOIndexes.IndexInfo
            {
                IndexName = "IX_Usuario_Email",
                Columns = new List<string> { "Email" },
                IsUnique = true
            },
            new IPOCOIndexes.IndexInfo
            {
                IndexName = "IX_Usuario_CPF",
                Columns = new List<string> { "CPF" },
                IsUnique = true
            }
        };
    }
}
```

3. **Retornar null quando não há índices:**
```csharp
public class SimplesEntity : Basico, IPOCOIndexes
{
    public string? Nome { get; set; }
    
    public IEnumerable<IPOCOIndexes.IndexInfo> GetIndexes()
    {
        // Sem índices personalizados
        return Enumerable.Empty<IPOCOIndexes.IndexInfo>();
    }
}
```

---

### 13. Chave - Classe para Instruções SQL

Classe usada para montar instruções SQL, especialmente relacionadas à cláusula WHERE.

**Propriedades:**
- `Campo`: Nome do campo no banco de dados
- `Valor`: Valor do campo
- `Tipo`: Tipo do campo (usando enum `Tipo`)
- `Operador`: Operador de comparação (usando enum `Operador`)
- `Parametro`: Nome do parâmetro SQL
- `Tabela`: Nome da tabela

**Exemplo:**
```csharp
using Yordi.Tools;

var chave = new Chave
{
    Campo = "Preco",
    Valor = 100.00m,
    Tipo = Tipo.MONEY,
    Operador = Operador.MAIORque,
    Parametro = "@preco",
    Tabela = "Produtos"
};

// Usado em conjunto com IPOCOIndexes.IndexInfo
var indexInfo = new IPOCOIndexes.IndexInfo
{
    IndexName = "IX_Produtos_Preco",
    Columns = new List<string> { "Preco" },
    Chaves = new[] { chave }
};
```

---

### 14. Extensions
Extensões úteis para tipos do .NET.

**ReflectionExtensions:**
- `GetMethodsWithAttribute(Type, Type)`: Métodos com atributo específico
- `GetMembersWithAttribute(Type, Type)`: Membros com atributo específico
- `GetDefaultValue(PropertyInfo)`: Valor padrão de propriedade

**SocketExtensions:**
- Extensões para trabalhar com Sockets

**Exemplo:**
```csharp
using System;
using System.Reflection;

var tipo = typeof(MinhaClasse);
var metodosAPI = tipo.GetMethodsWithAttribute(typeof(APIAttribute));

foreach (var metodo in metodosAPI)
{
    Console.WriteLine($"Método API: {metodo.Name}");
}
```

---

## 📋 Interfaces

- `ICommonColumns`: Define colunas comuns de auditoria
- `IAuto`: Define propriedade Auto (ID)
- `IDescricao`: Define propriedade Descricao
- `IObjectStringIndexer`: Permite acesso a propriedades por string
- `IPropertyType`: Permite obter tipo de propriedade
- `IPOCOtoDB`: Marca classes que interagem com banco de dados
- `IPOCOIndexes`: Define índices de banco de dados para entidades POCO
- `IChave`: Interface para a classe Chave

---

## 📝 Changelog

### v1.0.15 (atual)
- **Alteração importante**: Atualização no sistema de logging.
  - As APIs de log em `Yordi.Tools.Logger` agora retornam valores informativos em vez de serem apenas `void`/fire-and-forget:
    - `LogAsync(Exception filterContext, string origem = "", int line = 0, string file = "")` passa a retornar `Task<string?>` (linha do log ou `null` em caso de falha).
    - `LogAsync(string texto, string origem = "", int line = 0, string file = "")` passa a retornar `Task<string?>`.
    - `LogSync(Exception filterContext, string origem = "", int line = 0, string file = "")` passa a retornar `string?`.
    - `LogSync(string texto, string origem = "", int line = 0, string file = "")` passa a retornar `string?`.
  - A implementação `LoggerYordi` (implementação de `Microsoft.Extensions.Logging.ILogger`) continua a implementar `void Log<TState>(...)` conforme a interface, mas passa internamente as entradas para `Logger.LogSync`, que agora tem retorno — isto pode alterar o comportamento observado por consumidores que dependiam apenas dos efeitos colaterais.
  - Recomenda-se informar explicitamente os parâmetros `origem`, `line` e `file` ao chamar as APIs de log para garantir contexto útil nas mensagens gravadas.

- **Breaking change**: métodos públicos de logging agora retornam `string?` ou `Task<string?>`. Código que usava as APIs de log como `void` deve ser revisto para tratar (ou conscientemente ignorar) os novos retornos.

- Atualização da documentação `README.md` com exemplos e instruções de migração.

### v1.0.14
- **Adição**: Interface `IPOCOIndexes` para definição de índices de banco de dados
- **Adição**: Classe `IPOCOIndexes.IndexInfo` para especificar detalhes de índices
- **Adição**: Classe `Chave` para construção de instruções SQL e cláusulas WHERE
- **Melhoria**: Documentação XML completa para todas as novas interfaces e classes

### v1.0.13 (preterida)
- Versão estável para .NET 8.0
- Todas as funcionalidades testadas e validadas

### v1.0.11
- **Correção**: Bug em `Conversores.FromJson` quando AssemblyQualifiedName era string

### v1.0.10
- **Correção**: Classe `Cripto` - palavra-chave convertida poderia ter dois valores se a mesma instância fosse usada mais de uma vez
- **Adição**: Projeto console para testes (não incluído no NuGet)
- **Nota**: Versionamento seguirá baseado na DLL principal

### v1.0.9
- **Adição**: Classe `FileRepository<T>` para trabalhar com arquivos JSON

### Versões anteriores
- Implementação das classes principais: ValidaObjetos, FileTools, Conversores, LoggerYordi
- Sistema de eventos com EventBaseClass
- Utilitários de criptografia, data e rede

---

## 🤝 Contribuição

Contribuições são bem-vindas! Para contribuir:

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

---

## 📄 Licença

Este projeto está licenciado sob a **Licença MIT**. Veja o arquivo LICENSE para mais detalhes.

---

## 👤 Autor

**Leopoldo Yordi**  
Yordi Sistemas Inteligentes

- GitHub: [@leoyordi](https://github.com/leoyordi)
- Repository: [Yordi.Tools](https://github.com/leoyordi/Yordi.Tools)

---

## 🔗 Links Úteis

- [NuGet Package](https://www.nuget.org/packages/Yordi.Tools)
- [GitHub Repository](https://github.com/leoyordi/Yordi.Tools)
- [Documentação .NET 8](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-8)

---

## 💡 Suporte

Se você encontrar algum problema ou tiver sugestões, por favor:
- Abra uma [Issue no GitHub](https://github.com/leoyordi/Yordi.Tools/issues)
- Entre em contato através do repositório

---

**Yordi.Tools** - Simplificando o desenvolvimento .NET 🚀
