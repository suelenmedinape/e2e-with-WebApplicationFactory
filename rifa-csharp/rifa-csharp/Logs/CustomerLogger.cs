namespace rifa_csharp.Logging;

public class CustomerLogger : ILogger
{
    private readonly string LoggerName;
    private readonly CustomLoggerProviderConfiguration LoggerConfig;
    private static readonly object _lock = new object();

    public CustomerLogger(string name, CustomLoggerProviderConfiguration config)
    {
        LoggerName = name;
        LoggerConfig = config;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel == LoggerConfig.LogLevel;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string message = $"{logLevel.ToString()}: {eventId.Id} - {formatter(state, exception)}";
        WriteTextInTheFile(message);
    }

    private void WriteTextInTheFile(string message)
    {
        try
        {
            Console.WriteLine(message); 
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Erro ao escrever log: {ex.Message}");
        }
    }
}

/*
 * PARA QUANDO ESTIVER EM PRODUÇÃO
 public class CustomerLogger : ILogger
   {
       private readonly string LoggerName;
       private readonly CustomLoggerProviderConfiguration LoggerConfig;
       private static readonly object _lock = new object(); // Thread-safety
   
       public CustomerLogger(string name, CustomLoggerProviderConfiguration config)
       {
           LoggerName = name;
           LoggerConfig = config;
       }
   
       public IDisposable? BeginScope<TState>(TState state) where TState : notnull
       {
           return null;
       }
   
       public bool IsEnabled(LogLevel logLevel)
       {
           return logLevel >= LoggerConfig.LogLevel; // ✅ >= ao invés de ==
       }
   
       public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
       {
           if (!IsEnabled(logLevel)) return;
   
           // ✅ Adiciona timestamp e nome do logger
           string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
           string message = $"[{timestamp}] [{logLevel}] [{LoggerName}] {formatter(state, exception)}";
           
           if (exception != null)
           {
               message += $"\nException: {exception}";
           }
   
           WriteTextInTheFile(message);
       }
   
       private void WriteTextInTheFile(string message)
       {
           try
           {
               // ✅ Usa caminho absoluto baseado no diretório da aplicação
               string logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
               Directory.CreateDirectory(logDirectory); // Cria se não existir
   
               // ✅ Arquivo com data para rotação automática
               string fileName = $"log-{DateTime.Now:yyyy-MM-dd}.txt";
               string filePathLog = Path.Combine(logDirectory, fileName);
   
               // ✅ Thread-safe para múltiplas requisições simultâneas
               lock (_lock)
               {
                   File.AppendAllText(filePathLog, message + Environment.NewLine);
               }
           }
           catch (Exception ex)
           {
               // ✅ Em produção, falha no log não deve derrubar a aplicação
               // Pode usar Console.Error ou System.Diagnostics.Debug
               Console.Error.WriteLine($"Erro ao escrever log: {ex.Message}");
           }
       }
   }
 */