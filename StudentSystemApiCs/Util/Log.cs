using System;
using System.IO;
using System.Linq;

namespace StudentSystemApiCs.Util
{
    public static class Log
    {
        /// <summary>
        /// Logs data to stdout and writes to file.
        /// </summary>
        /// <param name="data">Data to be written</param>
        public static void Write(string data)
        {
            data = $"{DateTime.UtcNow.ToShortDateString()} {DateTime.UtcNow.ToShortTimeString()}: {data}";
            Console.WriteLine(data);
            File.AppendAllLines(AppConfig.LogFileName, new[] { data });
        }
    }
}