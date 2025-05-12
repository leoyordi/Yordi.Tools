# Yordi.Tools
Ferramentas simples .NET para trabalhar com arquivos, strings, números, datas, etc.

## Classes e Ferramentas

### ValidaObjetos
Uma classe para validação de diferentes tipos de dados.

#### Métodos
- `IsEmail(string email)`: Verifica se uma string é um endereço de e-mail válido.
- `IsCPF(string cpf)`: Verifica se uma string é um CPF válido.
- `IsCNPJ(string cnpj)`: Verifica se uma string é um CNPJ válido.
- `IsFone(string fone)`: Verifica se uma string é um número de telefone válido.
- `IsCircuito(string circuito)`: Verifica se uma string é um circuito válido.
- `IsMacAddress(string macAddress)`: Verifica se uma string é um endereço MAC válido.
- `IsIPv4(string value)`: Verifica se uma string é um endereço IPv4 válido.
- `IsIP(string value, out IPAddress? address)`: Verifica se uma string é um endereço IP válido.
- `IsURL(string value)`: Verifica se uma string é uma URL válida.
- `IsIPorURL(string value)`: Verifica se uma string é um endereço IP ou URL válido.
- `IsInt(string value, out int valor)`: Verifica se uma string pode ser convertida para um inteiro.
- `IsDouble(string value, out double valor)`: Verifica se uma string pode ser convertida para um double.
- `IsEnumValid(Enum enumValue, int value, int minValue, int maxValue)`: Verifica se um valor é válido para um enum.

### LoggerYordi
Uma classe para logging.

#### Métodos
- `LoggerInstance(string path = "")`: Retorna uma instância do logger.
- `BeginScope<TState>(TState state)`: Inicia um escopo de logging.
- `IsEnabled(LogLevel logLevel)`: Verifica se um nível de log está habilitado.
- `Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)`: Registra uma mensagem de log.

### NewGuid
Uma classe para gerar GUIDs sequenciais.

#### Métodos
- `NewSequentialGuid()`: Gera um novo GUID sequencial baseado no timestamp e tipo de banco de dados.

### FileTools
Uma classe para operações comuns com arquivos.

#### Métodos
- `ReadAllTextAsync(string? filePath, Encoding? encoding = null)`: Lê todo o conteúdo de um arquivo de forma assíncrona.
- `ReadAllText(string? filePath)`: Lê todo o conteúdo de um arquivo.
- `ReadAllLines(string? filePath, Encoding? encoding = null)`: Lê todas as linhas de um arquivo e retorna como um array de strings.
- `WriteText(string? filePath, string text, Encoding? encoding = null, bool replace = false)`: Escreve texto em um arquivo.
- `WriteAllBytes(string? filePath, byte[] bytes, bool replace = false)`: Escreve bytes em um arquivo.
- `WriteTextAsync(string? filePath, string text, Encoding? encoding = null)`: Escreve texto em um arquivo de forma assíncrona.
- `DetectFileEncoding(string? filename, Encoding? defaultEncoding = null)`: Detecta a codificação de um arquivo.
- `NomeArquivo(string? filePath)`: Retorna o nome do arquivo de um caminho especificado.
- `PastaSomente(string? nomeArquivoCompleto)`: Retorna o nome da pasta de um caminho especificado.
- `PastaTemporaria()`: Retorna o caminho da pasta temporária.
- `NomeArquivoSemExtensao(string? filePath)`: Retorna o nome do arquivo sem a extensão.
- `Extensao(string? filePath)`: Retorna a extensão do arquivo.
- `GetBuildDate()`: Retorna a data de compilação do assembly.
- `ArquivoExiste(string? arquivo)`: Verifica se um arquivo existe.
- `PastaExiste(string? pasta)`: Verifica se uma pasta existe.
- `CriaDiretorio(string? path)`: Cria um diretório.
- `Combina(string? pasta, string? arquivo)`: Combina uma pasta e um arquivo em um caminho.
- `UltimoLog(string? pasta)`: Retorna o último arquivo de log na pasta especificada.
- `LogDiaAnterior(string? pasta)`: Retorna o arquivo de log do dia anterior na pasta especificada.
- `Excluir(string? arquivo)`: Exclui um arquivo.
- `Mover(string? arquivoOrigem, string? arquivoDestino)`: Move um arquivo de origem para um arquivo de destino.
- `DataCriacao(string? arq)`: Retorna a data de criação de um arquivo.
- `DataAtualizacao(string? arq)`: Retorna a data de atualização de um arquivo.
- `ArquivosPorData(string? folder, string? criteria, DateTime ultimoLidoUTC)`: Retorna um dicionário de arquivos modificados após uma data específica.
- `Arquivos(string? pasta, string? criterio = "*.log", bool? topDirectoryOnly = true)`: Retorna os arquivos de uma pasta com base em um critério.
- `ListarArquivos(string? pasta, string? criterio, bool? topDirectoryOnly = true)`: Retorna uma lista preguiçosa de arquivos em um diretório com base em critérios.
- `Excluir(string pasta, string extensao, DateTime olderThan)`: Exclui arquivos mais antigos que uma data específica.

### EventBaseClass
Uma classe base para eventos.

#### Métodos
- `Message(string mensagem, string origem = "", int line = 0, string path = "")`: Dispara um evento de mensagem.
- `Error(string mensagem, string origem = "", int line = 0, string path = "")`: Dispara um evento de erro.
- `Error(Exception e, string origem = "", int line = 0, string path = "")`: Dispara um evento de exceção.
- `Rows(float registros)`: Dispara um evento de quantidade de registros.
- `Progresso(float progresso)`: Dispara um evento de progresso.

### DataPadrao
Uma classe para manipulação de datas padrão.

#### Propriedades
- `Brasilia`: Retorna a data e hora atual de Brasília.
- `Maquina`: Retorna a data e hora atual da máquina.
- `DataBrasiliaToMSSQL`: Retorna a data e hora atual de Brasília no formato MSSQL.
- `MinValue`: Retorna a data mínima no MySQL.

### Cripto
Uma classe para criptografia.

#### Métodos
- `GetKey()`: Gera a chave de criptografia válida.
- `Encrypt(string texto)`: Criptografa um texto.
- `Decrypt(string textoCriptografado)`: Descriptografa um texto criptografado.

### Conversores
Uma classe para conversão de tipos de dados.

#### Métodos
- `ToDataHora(string expressao)`: Converte uma string para DateTime.
- `ToDataHora(object objeto)`: Converte um objeto para DateTime.
- `DateToString(DateTime dataHora)`: Converte um DateTime para string no formato "yyyy-MM-dd HH:mm:ss".
- `ToDecimal(string? expressao)`: Converte uma string para decimal.
- `ToDecimal(object? expressao)`: Converte um objeto para decimal.
- `ToDouble(string? expressao)`: Converte uma string para double.
- `ToDouble(object? expressao)`: Converte um objeto para double.
- `ToInt(string? expressao)`: Converte uma string para int.
- `ToInt(object? expressao)`: Converte um objeto para int.
- `ToInt(bool valor)`: Converte um bool para int.
- `ToInt(this bool? valor)`: Converte um bool? para int?.
- `ToBool(int valor)`: Converte um int para bool.
- `ToBool(string? expressao)`: Converte uma string para bool.
- `ToBool(object? objeto)`: Converte um objeto para bool.
- `ToBool(this int? valor)`: Converte um int? para bool?.
- `PropriedadeTipo(PropertyInfo p)`: Retorna o tipo de uma propriedade.
- `Right(string value, int size)`: Retorna os últimos caracteres de uma string.
- `SubstituiDiacritico(this string texto)`: Substitui caracteres diacríticos em uma string.
- `RemoveCaracteresEspeciais(this string texto, bool aceitaEspaco = true, bool substituiAcentos = false, bool substituiPontos = true)`: Remove caracteres especiais de uma string.
- `RemovePontos(this string texto)`: Remove pontos de uma string.
- `ToVersion(string texto)`: Converte uma string para Version.
- `RetornaNumeros(string texto)`: Retorna apenas os números de uma string.
- `ToJson<T>(T? obj, bool writeIndent = false)`: Converte um objeto para JSON.
- `ToJson(this object? obj, bool writeIndent = false)`: Converte um objeto para JSON.
- `ToJsonUtf8(Object obj)`: Converte um objeto para JSON em bytes UTF-8.
- `FromJson<T>(string obj) where T : class`: Converte uma string JSON para um objeto.
- `FromJson(string? obj, Type type)`: Converte uma string JSON para um objeto de um tipo específico.
- `FromJson(string? obj, string? assemblyQualifiedName)`: Converte uma string JSON para um objeto de um tipo específico.
- `FromJson<T>(byte[] bytes) where T : class`: Converte bytes JSON para um objeto.
- `FromJson(byte[] obj, Type type)`: Converte bytes JSON para um objeto de um tipo específico.
- `FromJson(byte[] obj, string assemblyQualifiedName)`: Converte bytes JSON para um objeto de um tipo específico.


## RepositórioFile

A classe `RepositorioFile<T>` é um repositório em arquivo no formato JSON. Ela oferece métodos para salvar e ler objetos do tipo `T` em arquivos JSON, além de métodos para ler e escrever texto diretamente em arquivos.

### Funcionalidades

- **Salvar(T objeto)**: Salva o objeto informado no arquivo especificado.
- **Ler()**: Lê o arquivo, converte-o no tipo informado e devolve o objeto.
- **LerAsync()**: Modo assíncrono para ler o arquivo e converter no tipo informado.
- **LerComoTexto()**: Lê o arquivo base e devolve-o em formato texto, sem conversão.
- **LerComoTexto(string arquivo)**: Lê o arquivo informado e devolve-o em formato texto, sem conversão.
- **LerLinhas(string arquivo)**: Devolve o conteúdo do arquivo em linhas, dentro de uma array de string.
- **Escrever(string texto)**: Escreve um texto no arquivo base.
- **Escrever(string texto, string arquivo)**: Escreve o texto no arquivo informado de forma assíncrona.

### Exemplo de Uso
1. Exemplo para salvar configurações em um arquivo JSON:
```csharp
using System.Text; 
using Yordi.Tools;
public class Program 
{ 
    public static void Main() 
    { 
        var repo = new RepositorioFile<MyClass>("caminho/para/arquivo.json", Encoding.UTF8);
        // Salvar um objeto
        var myObject = new MyClass { Property1 = "Valor1", Property2 = "Valor2" };
        repo.Salvar(myObject);
        // Ler um objeto
        var loadedObject = repo.Ler();
        // Ler um arquivo como texto
        var texto = repo.LerComoTexto();
    }
}
public class MyClass 
{ 
    public string Property1 { get; set; } 
    public string Property2 { get; set; } 
}
```

2. Exemplo para salvar uma lista de objetos em um arquivo JSON:

```csharp
using System.Text;
using Yordi.Tools;
public class MyRepo : RepositorioFile<IEnumerable<MyClass>>
{
    private static MyRepo _instance;
    private MyRepo(string path) : base(path, Encoding.UTF8)  { }
    public static MyRepo Instance => _instance ??= _new MyRepo("caminho/para/arquivo.json");

    public bool Salvar(MyClass objeto)
    {
        var list = Ler().ToList();
        list.Add(objeto);
        return base.Salvar(list);
    }    
}
```

## Instalação

Para instalar o pacote, use o seguinte comando:

<pre>dotnet add package Yordi.Tools</pre>


## Uso

Aqui estão alguns exemplos de como usar as ferramentas:
```csharp
using Yordi.Tools;
// Exemplo de uso do ValidaObjetos bool isEmail = ValidaObjetos.IsEmail("example@example.com"); bool isCPF = ValidaObjetos.IsCPF("123.456.789-09");
// Exemplo de uso do LoggerYordi var logger = LoggerYordi.LoggerInstance(); logger.Log(LogLevel.Information, new EventId(), "Mensagem de log", null, (state, exception) => state.ToString());
// Exemplo de uso do NewGuid Guid newGuid = NewGuid.NewSequentialGuid();
// Exemplo de uso do FileTools string content = FileTools.ReadAllText("example.txt"); FileTools.WriteText("example.txt", "Hello, World!");
// Exemplo de uso do EventBaseClass var eventBase = new EventBaseClass(); eventBase.Message("Mensagem de evento");
// Exemplo de uso do DataPadrao DateTime brasiliaTime = DataPadrao.Brasilia;
// Exemplo de uso do Cripto var cripto = new Cripto("chave"); string encrypted = cripto.Encrypt("texto"); string? decrypted = cripto.Decrypt(encrypted);
// Exemplo de uso do Conversores int intValue = Conversores.ToInt("123"); bool boolValue = Conversores.ToBool("true");
```


## Contribuição

Se você quiser contribuir para o projeto, por favor, siga as diretrizes de contribuição.

## Licença

Este projeto está licenciado sob a licença MIT.

## Changelog
- **v1.0.10**: 
  - Classe Cripto corrigida. A palavra-chave convertida poderia ter dois valores se a mesma instância fosse usada mais de uma vez.
  - Acréscimo de um projeto do tipo console para testes. Não acompanha o pacote nugget. Versionamento seguirá com base na dll
- **v1.0.9**: Adicionada a classe `RepositorioFile<T>` para facilitar o trabalho com arquivos JSON.
