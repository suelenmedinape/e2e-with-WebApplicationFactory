using System.Collections.Concurrent;

namespace rifa_csharp.Logging;

public class CustomLoggerProvider : ILoggerProvider
{
    private readonly CustomLoggerProviderConfiguration LoggerConfig;
    private readonly ConcurrentDictionary<string, CustomerLogger> Loggers = new ConcurrentDictionary<string, CustomerLogger>();

    public CustomLoggerProvider(CustomLoggerProviderConfiguration config)
    {
        LoggerConfig = config;
    }
     
    public ILogger CreateLogger(string categoryName)
    {
        return Loggers.GetOrAdd(categoryName, name => new CustomerLogger(name, LoggerConfig));
    }    
        
    public void Dispose()
    {
        Loggers.Clear();
    }
}