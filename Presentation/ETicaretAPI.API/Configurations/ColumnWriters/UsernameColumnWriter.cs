using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using NpgsqlTypes;

namespace ETicaretAPI.API.Configurations.ColumnWriters
{
    public class UsernameColumnWriter : ColumnWriterBase
    {
        public UsernameColumnWriter() : base(NpgsqlDbType.Varchar)
        {
        }

        public override object GetValue(LogEvent logEvent, IFormatProvider formatProvider = null)
        {
            var (username, value) = logEvent.Properties.FirstOrDefault(a => a.Key == "user_name");
            return value?.ToString() ?? null;
        }
    }
}
