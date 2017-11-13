using System;
using System.Text;

namespace TinyLog.LogPatterns
{
    public class ReadablePattern : PatternGenerator
    {
       
        private const char SEPARATOR = '\t';
        private const string LINESTART = "##";

        public override byte[] GeneratePattern(LogLevel level, string message) {
            var builder = new StringBuilder();
            builder.Append(LINESTART);
            builder.Append(DateTime.Now.ToString());
            builder.Append(SEPARATOR);
            builder.Append((int)level);
            builder.Append(SEPARATOR);
            builder.Append(_config.Identifier);
            //builder.Append(SEPARATOR);
            builder.Append(Environment.NewLine);
            builder.Append(message);
            builder.Append(Environment.NewLine);
            return Encoding.UTF8.GetBytes(builder.ToString());
        }


    }

    public abstract class PatternGenerator
    {
        internal LogConfigurator _config;

        public PatternGenerator() {
            _config = Logger.Instance.Configure();
        }

        public abstract byte[] GeneratePattern(LogLevel level, string message);
    }
}
