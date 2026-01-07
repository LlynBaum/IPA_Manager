namespace Ipa.Manager.Tests.E2E;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    }

    public ILogger CreateLogger(string categoryName)
        => new FileLogger(categoryName, _filePath, _lock);

    public void Dispose() { }
}

public class FileLogger : ILogger
{
    private readonly string category;
    private readonly string filePath;
    private readonly object @lock;

    public FileLogger(string categoryName, string filePath, object writeLock)
    {
        category = categoryName;
        this.filePath = filePath;
        @lock = writeLock;
    }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (@lock)
        {
            File.AppendAllText(filePath,
                $"{DateTime.Now:HH:mm:ss} [{logLevel}] {category}: {formatter(state, exception)}{Environment.NewLine}");

            if (exception != null)
            {
                File.AppendAllText(filePath, exception + Environment.NewLine);
            }
        }
    }
}
