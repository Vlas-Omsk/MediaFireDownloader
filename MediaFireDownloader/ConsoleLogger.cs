using System;

namespace MediaFireDownloader
{
    internal sealed class ConsoleLogger
    {
        public void Info(string message)
        {
            WriteLine("INF", message);
        }

        public void Skipped(string message)
        {
            WriteLine("SKP", message);
        }

        public void Download(string message)
        {
            WriteLine("DWN", message);
        }

        public void End(string message)
        {
            WriteLine("END", message);
        }

        public void Error(string message)
        {
            WriteLine("ERR", message);
        }

        public void Exception(string message, Exception ex)
        {
            Error(message);
            Padding(ex.ToString());
            //Padding("Type: " + ex.GetType().Name);
            //message = ex.Message;
            //while (true)
            //{
            //    ex = ex.InnerException;
            //    if (ex == null)
            //        break;

            //    message += $" ({ex.Message})";
            //}
            //Padding("Message: " + message);
        }

        public void Padding(string message)
        {
            Console.WriteLine("  " + message);
        }

        private void WriteLine(string prefix, string message)
        {
            Console.WriteLine($"[{prefix}] {message}");
        }
    }
}
