using CSharpServerFramework.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpServerFramework.Log
{
    public class ConsoleLogger:ILoggerLog
    {
        public void Log(string LogString)
        {
            Console.WriteLine(LogString);
        }

        public void Close()
        {
        }

        private ConsoleLogger()
        {

        }
        private static ConsoleLogger _instance = new ConsoleLogger();
        public static ConsoleLogger Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}
