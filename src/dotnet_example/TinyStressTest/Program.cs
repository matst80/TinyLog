using System;
using System.Threading.Tasks;
using TinyLog;

namespace TinyStressTest
{
    class Program
    {
        public class LogTestSender
        {

            public LogTestSender(Logger logger)
            {
                _logger = logger;
            }

            private Logger _logger;

            public void Send(int noi, LogLevel level)
            {
                for (var i = 0; i < noi; i++)
                {
                    _logger.Log(level, "Message with some data...");
                }
            }
        }


        static void Main(string[] args)
        {
            Logger.Instance.Configure().AddRemote("10.10.10.228", 11000, "somekey").SetIdentifier("stresstest");//.AddConsole();
            var lg = Logger.Instance;
            var start = DateTime.Now;
            for (var i = 0; i < 1000; i++)
            {
                //Task.Run(() => {
                    new LogTestSender(lg).Send(10, LogLevel.Info);
                    new LogTestSender(lg).Send(10, LogLevel.Warn);
                    new LogTestSender(lg).Send(10, LogLevel.Error);
                    new LogTestSender(lg).Send(10, LogLevel.FUBAR);
                //});
            }
            Console.WriteLine("Sent:" + (DateTime.Now-start).TotalMilliseconds);
            Console.ReadKey();
        }

    }
}
