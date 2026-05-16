using System;
using System.IO;

namespace TrainCrewMoniter
{
    public static class Log
    {
        /// <summary>
        /// エラーログをファイルに書き込む
        /// </summary>
        /// <param name="ex"></param>
        public static void WriteErrorLog(Exception ex)
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLog.txt");
            string logEntry = string.Format(
                "[{0}] {1}\n{2}\n{3}\n{4}\n",
                DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                ex.GetType().FullName,
                ex.Message,
                ex.StackTrace,
                new string('-', 80));

            File.AppendAllText(logFilePath, logEntry);
        }
    }
}
