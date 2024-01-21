using Xunit.Abstractions;

namespace Engine
{
    public class Log
    {
        public ITestOutputHelper output;
        public string logPrefix;

        public Log(ITestOutputHelper o, string l = "=gen")
        {
            output = o;
            logPrefix = l;
        }

        public string Write(string message, string value)
        {
            output.WriteLine($"{logPrefix}:: {message} `{value}`");
            Console.WriteLine($"{logPrefix}:: {message} `{value}`");
            return value;
        }

        public string Write(string message, string value, DateTime dt)
        {
            output.WriteLine($"{logPrefix}::{dt.ToString("HH:mm:ss:ff")}:: {message} `{value}`");
            Console.WriteLine($"{logPrefix}::{dt.ToString("HH:mm:ss:ff")}:: {message} `{value}`");
            return value;
        }

        public void Write(string message)
        {
            output.WriteLine($"{logPrefix}:: {message}");
            Console.WriteLine($"{logPrefix}:: {message}");
        }

        public void Write(string message, DateTime dt)
        {
            output.WriteLine($"{logPrefix}::{dt.ToString("HH:mm:ss:ff")}:: {message}");
            Console.WriteLine($"{logPrefix}::{dt.ToString("HH:mm:ss:ff")}:: {message}");
        }
    }
}