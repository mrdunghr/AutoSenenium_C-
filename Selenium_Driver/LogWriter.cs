using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Selenium_Driver
{
    public class LogWriter
    {
        public string message {  get; set; }

        public LogWriter() { }
        public void logWl(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            this.message = message;
            Console.WriteLine($"Thông Báo: {message}");
            Console.ResetColor();
        }

        public void logW(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            this.message = message;
            Console.Write($"Thông Báo: {message}");
            Console.ResetColor();
        }

        public override string? ToString()
        {
            return $"Thông Báo: {message}";
        }
    }
}
