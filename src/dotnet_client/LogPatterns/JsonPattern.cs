using System;
using System.Text;

namespace TinyLog.LogPatterns
{
    public class JsonPattern : PatternGenerator
    {
        private const char ObjectStart = '{';
        private const char ObjectEnd = '}';
        private const char StringChar = '"';
        private const char KeyValueSeparator = ':';

        private string BetweenKeyAndValue = "\":\"";

        private void AddKey(StringBuilder builder, string key, string value) {
            builder.Append(StringChar);
            builder.Append(key);
            builder.Append(BetweenKeyAndValue);
            builder.Append(value.Replace("\"","'"));
            builder.Append(StringChar);
        }

        public override byte[] GeneratePattern(LogLevel level, string message)
        {
            var resultString = new StringBuilder();
            resultString.Append(ObjectStart);
            resultString.Append(StringChar);
            AddKey(resultString, "level", ((int)level).ToString());
            resultString.Append(",");
            AddKey(resultString, "device", _config.Identifier);
            resultString.Append(",");
            AddKey(resultString, "message", message);
            resultString.Append(",");
            AddKey(resultString, "ts", DateTime.Now.ToString());
            resultString.Append(ObjectEnd);
            return Encoding.UTF8.GetBytes(resultString.ToString());
        }
    }
}
