using System;
using System.Collections.Generic;
using System.Net;
using TinyLog.LogPatterns;

namespace TinyLog
{
    public class LogConfigurator
    {
        internal List<RemoteAddress> Remotes = new List<RemoteAddress>();
        internal bool LogToConsole;
        internal LogLevel UserMinimumLogLevel = LogLevel.Verbose;
        internal int BatchDelay = 2000;
        private bool _userHasConfiguredLogLevel = false;
        internal string Identifier = Guid.NewGuid().ToString().Substring(0, 8);

        private PatternGenerator _patternGenerator;
        public PatternGenerator PatternGenerator 
        { 
            get {
                if (_patternGenerator == null)
                    _patternGenerator = new ReadablePattern();
                return _patternGenerator;
            } 
        }

        public LogConfigurator AddRemote(string address, int port, string authkey)
        {
            Remotes.Add(new RemoteAddress() { 
                Address = IPAddress.Parse(address), 
                Port = port, 
                AuthKey = System.Text.Encoding.UTF8.GetBytes(authkey) 
            });
            return this;
        }

        public LogConfigurator SetLogPattern(PatternGenerator generator) {
            _patternGenerator = generator;
            return this;
        }

        public LogConfigurator AddConsole()
        {
            LogToConsole = true;
            return this;
        }

        public LogConfigurator SetIdentifier(string identifier)
        {
            Identifier = identifier;
            return this;
        }

        public LogConfigurator Clear()
        {
            Remotes.Clear();
            return this;
        }

        public LogConfigurator SetBatchDelay(int delay)
        {
            BatchDelay = delay;
            return this;
        }

        public LogConfigurator MinimumLogLevel(LogLevel level)
        {
            _userHasConfiguredLogLevel = true;
            UserMinimumLogLevel = level;
            return this;
        }

        public bool ShouldLog(LogLevel level)
        {
#if DEBUG
            var isDebug = true;
#else
            var isDebug = false;
#endif 

            if(isDebug || _userHasConfiguredLogLevel)
            {
                return level >= UserMinimumLogLevel;
            }

            return level > LogLevel.Info;
        }
    }
}
