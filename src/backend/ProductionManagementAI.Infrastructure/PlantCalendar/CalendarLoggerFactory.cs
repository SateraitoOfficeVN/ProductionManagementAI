using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ProductionManagementAI.Infrastructure.PlantCalendar;

/// <summary>Suppresses EF diagnostic SQL and exception details only during calendar requests.</summary>
public sealed class CalendarLoggerFactory(ILoggerFactory inner, IHttpContextAccessor context) : ILoggerFactory
{
    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => new CalendarLogger(inner.CreateLogger(categoryName), context);
    /// <inheritdoc />
    public void AddProvider(ILoggerProvider provider) => inner.AddProvider(provider);
    /// <summary>Leaves ownership of the shared logger factory with dependency injection.</summary>
    public void Dispose() { }
    private sealed class CalendarLogger(ILogger inner, IHttpContextAccessor context) : ILogger
    {
        private bool Suppressed => context.HttpContext?.Request.Path.StartsWithSegments("/api/plant-calendar") == true;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => inner.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => !Suppressed && inner.IsEnabled(logLevel);
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!Suppressed) inner.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
