using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogLibrary
{
    public sealed class Logger
    {
        private static readonly Lazy<Logger> _instance = new Lazy<Logger>(() => new Logger());
        private readonly string _logFilePath;
        private Logger()
        {
            var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _logFilePath = Path.Combine(basePath, "events.log");
        }
        public static Logger Instance => _instance.Value;
        public void Log(string importance, string message)
        {
            var logEntry = $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} {importance}. {message}";
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);

        }
    }

}
