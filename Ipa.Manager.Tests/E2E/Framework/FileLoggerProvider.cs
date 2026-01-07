namespace Ipa.Manager.Tests.E2E.Framework;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly string filePath;
    private readonly object @lock = new object();

    public FileLoggerProvider(string filePath)
    {
        this.filePath = filePath;
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    }

    public ILogger CreateLogger(string categoryName)
        => new FileLogger(categoryName, filePath, @lock);

    public void Dispose() { }
}

public class FileLogger(string categoryName, string filePath, object writeLock) : ILogger
{
#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
    public IDisposable BeginScope<TState>(TState state) => null!;
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (writeLock)
        {
            File.AppendAllText(filePath,
                $"{DateTime.Now:HH:mm:ss} [{logLevel}] {categoryName}: {formatter(state, exception)}{Environment.NewLine}");

            if (exception != null)
            {
                File.AppendAllText(filePath, exception + Environment.NewLine);
            }
        }
    }
}
