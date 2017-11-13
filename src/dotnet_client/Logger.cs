using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TinyLog.LogPatterns;

namespace TinyLog
{
    public class Logger
    {
        private const int MAX_QUEUE_LENGTH = 100;

        private static Logger _instance = new Logger();
        private readonly List<byte[]> _queue = new List<byte[]>();
        private System.Threading.Timer _timer;
        private LogConfigurator _config = new LogConfigurator();

        public static Logger Instance 
        {
            get
            {
                return _instance;
            }
        }

        public LogConfigurator Configure()
        {
            return _config;
        }



        public void Log(LogLevel level, string message) 
        {
            
            AddToQueue(_config.PatternGenerator.GeneratePattern(level, message));
        }

        public void LogInfo(string message) 
        {
            Log(LogLevel.Info, message);
        }

        public void LogVerbose(string message)
        {
            Log(LogLevel.Verbose, message);
        }

        public void LogVerboseEnterMethod([CallerMemberName] string methodName = null)
        {
            Log(LogLevel.Verbose, "Entering method " + methodName);
        }

        public void LogVerboseExitMethod([CallerMemberName] string methodName = null)
        {
            Log(LogLevel.Verbose, "Exiting method " + methodName);
        }

        public void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        public void LogWarning(string message)
        {
            Log(LogLevel.Warn, message);
        }

        public void LogCritial(string message)
        {
            Log(LogLevel.Critical, message);
        }
                        
        public void Log(Exception ex, string message = "Exception") {
            var msgBuilder = new StringBuilder();
            msgBuilder.Append(message);
            msgBuilder.Append(": ");
            msgBuilder.Append(ex.Message);
            msgBuilder.Append(", ");
            msgBuilder.Append(ex.StackTrace);
            LogError(msgBuilder.ToString());
        }

        private void AddToQueue(byte[] message)
        {
            if(_config.LogToConsole)
            {
				Console.WriteLine($"TinyLog: {message}");
            }

            _queue.Add(message);
            //if (_queue.Count() > MAX_QUEUE_LENGTH)
              //  Internal_Send();
            if(_timer != null)
            {
                _timer.Change(_queue.Count() > MAX_QUEUE_LENGTH?1:_config.BatchDelay, 0);
			}
            else
            {
				_timer = new System.Threading.Timer(
					async (state) => await Internal_Send(), null, _config.BatchDelay, 0); 
            }
        }

        private readonly byte[] ENDSUBMISSTION = Encoding.UTF8.GetBytes("<EOF>");


        private async Task Internal_Send()
        {
            List<byte[]> temp;
            lock(_queue)
            {
                temp = _queue.ToList();
                _queue.Clear();
            }

            foreach(var remote in _config.Remotes)
            {
                try
                {
                    using(var client = new System.Net.Sockets.TcpClient())
                    {
                        await Connect(client, remote);
                        foreach(var m in temp)
                        {
                            //var bytes = Encoding.UTF8.GetBytes(m + Environment.NewLine);
                            client.Client.Send(m);
                        }
                    }
				}
                catch (Exception ex)
                {
                    // TODO Better fail
                    Console.WriteLine($"Failed to send log to remote {remote.Address}:{remote.Port}{Environment.NewLine}{ex.ToString()}");
                }
			}
        }

        private async Task Connect(TcpClient client, RemoteAddress remote)
        {
            await client.ConnectAsync(remote.Address, remote.Port);
            client.Client.Send(remote.AuthKey);
        }
    }
}
