using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Yordi.Tools
{
    public interface IEventBaseClass
    {
        string? Mensagem { get; }
        event MyError? ErroEvent;
        event MyException? ExceptionEvent;
        event MyMessage? MessageEvent;
        event MyRows? ProgressMax;
        event MyProgress? ProgressValue;
    }

    public class EventBaseClass : IEventBaseClass
    {
        float _registros;
        float _progresso;
#pragma warning disable CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere adicionar o modificador "obrigatório" ou declarar como anulável.
        private static ILogger _logger;
#pragma warning restore CS8618 // O campo não anulável precisa conter um valor não nulo ao sair do construtor. Considere adicionar o modificador "obrigatório" ou declarar como anulável.
        protected internal string? _msg;

        [JsonIgnore]
        public virtual string? Mensagem { get { return _msg; } }

        public EventBaseClass()
        {
            if (_logger == null)
            {
                _logger = LoggerYordi.LoggerInstance();
            }
        }

        #region Eventos
        public event MyMessage? MessageEvent;
        public event MyProgress? ProgressValue;
        public event MyRows? ProgressMax;
        public event MyError? ErroEvent;
        public event MyException? ExceptionEvent;
        protected internal virtual void Message(string mensagem, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        {
            if (string.IsNullOrEmpty(mensagem)) return;
            MessageEvent?.Invoke(mensagem, origem, line, path);
            if (!string.IsNullOrEmpty(path))
            {
                path = FileTools.NomeArquivoSemExtensao(path) ?? "Desconhecido";
                origem = $"{path}.{origem}";
            }
            _msg = string.IsNullOrEmpty(origem) ? mensagem : $"[{origem}:{line}] {mensagem}";
            _logger.LogInformation(mensagem, origem, line);
        }
        protected internal virtual void Error(string mensagem, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        {
            if (string.IsNullOrEmpty(mensagem)) return;
            ErroEvent?.Invoke(mensagem, origem, line, path);
            if (!string.IsNullOrEmpty(path))
            {
                path = FileTools.NomeArquivoSemExtensao(path) ?? "Desconhecido";
                origem = $"{path}.{origem}";
            }
            _msg = string.IsNullOrEmpty(origem) ? mensagem : $"[{origem}:{line}] {mensagem}";
            _logger.LogError(_msg, origem, line);
        }
        protected internal virtual void Error(Exception e, [CallerMemberName] string origem = "", [CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
        {
            _msg = string.IsNullOrEmpty(origem) ? e.Message : $"[{origem}:{line}] {e.Message}";
            if (!string.IsNullOrEmpty(origem) && !e.Data.Contains("Method"))
                e.Data.Add("Method", origem);
            if (line > 0 && !e.Data.Contains("Line"))
                e.Data.Add("Line", line);
            ExceptionEvent?.Invoke(e, origem, line, path);
            if (!string.IsNullOrEmpty(path))
            {
                path = FileTools.NomeArquivoSemExtensao(path) ?? "Desconhecido";
                origem = $"{path}.{origem}";
            }
            _logger.LogError(e, origem, line);
        }
        protected internal void Rows(float registros) 
        {
            if (registros < _progresso)
                _progresso = registros;
            _registros = registros;
            ProgressMax?.Invoke(_registros); 
        }
        protected internal void Progresso(float progresso) 
        {
            if (progresso > _registros)
                progresso = _registros;
            else
                progresso++;
            _progresso = progresso;

            ProgressValue?.Invoke(_progresso); 
        }

        #endregion


        public static ILogger GetLogger()
        {
            if (_logger == null)
                _logger = LoggerYordi.LoggerInstance();
            return _logger;
        }
        public static void SetLogger(ILogger logger) { _logger = logger; }

    }
}
