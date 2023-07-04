using Xunit.Abstractions;

namespace Bindings
{
    public class Log
    {
        private ITestOutputHelper output;
        private string logPrefix;

        public Log(ITestOutputHelper o, string l)
        {
            output = o;
            logPrefix = l;
        }

        public T Write<T>(string message, T value)
        {
            output.WriteLine($"{logPrefix}:: {message} `{value}`");
            Console.WriteLine($"{logPrefix}:: {message} `{value}`");
            return value;
        }
    }
}