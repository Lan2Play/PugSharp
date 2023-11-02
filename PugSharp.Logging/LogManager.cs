using Microsoft.Extensions.Logging;

namespace PugSharp.Logging
{
    public static class LogManager
    {
        public static ILoggerFactory? LoggerFactory { get; set; }

        public static ILogger<T> CreateLogger<T>()
        {
            if (LoggerFactory == null)
            {
                LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                });
            }

            return LoggerFactory.CreateLogger<T>();
        }
    }
}
