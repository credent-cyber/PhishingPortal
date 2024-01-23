using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Configuration;
using PhishingPortal.DataContext;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


namespace PhishingPortal.Server
{
    public class DbSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly string _dbConnString;
        private readonly string sqlProvider;
        private readonly bool _useSqlLite;

        public DbSink(IFormatProvider formatProvider, string dbConnString, string sqlProvider = "mysql", bool useSqlLite = false)
        {
            _formatProvider = formatProvider;
            this._dbConnString = dbConnString;
            this.sqlProvider = sqlProvider;
            this._useSqlLite = useSqlLite;
        }

        public async void Emit(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(_formatProvider);
            var dbContext = GetDbContext(_useSqlLite, _dbConnString);

            try
            {
                if (logEvent.Level == LogEventLevel.Error || logEvent.Level == LogEventLevel.Fatal) 
                {
                    var cnt = await dbContext
                        .Database
                        .ExecuteSqlInterpolatedAsync($"insert into applogs(Type, Message, ErrorDetail, IsEmailed, CreatedOn, CreatedBy) Values('{logEvent.Level.ToString()}','{message}','{JsonConvert.SerializeObject(logEvent.Exception)}',0, CURRENT_TIMESTAMP(), '{nameof(DbSink)}')");
                }
            }
            catch (Exception exception)
            {
                Log.Logger.Fatal(exception, "Db Sink error");
            }
        }

        private DbContext GetDbContext(bool useSqlLite, string dbConnString)
        {
            var contextOptionBuilder = new DbContextOptionsBuilder<CentralDbContext>(new DbContextOptions<CentralDbContext>());

            if (useSqlLite)
            {
                contextOptionBuilder.UseSqlite();
            }
            else
            {
                contextOptionBuilder.UseMySql(dbConnString, ServerVersion.AutoDetect(dbConnString),
                  mySqlOptions =>
                  {
                      mySqlOptions.EnableRetryOnFailure(
                          maxRetryCount: 5,
                          maxRetryDelay: TimeSpan.FromSeconds(30),
                          errorNumbersToAdd: null);
                  });

            }

            return new CentralDbContext(contextOptionBuilder.Options);
        }

    }



    public static class DbSinkExtensions
    {
        public static LoggerConfiguration DbSink(
                  this LoggerSinkConfiguration loggerConfiguration, string connString, bool useSqlLite,
                  IFormatProvider formatProvider = null)
        {
            return loggerConfiguration.Sink(new DbSink(formatProvider, connString, useSqlLite: useSqlLite));
        }
    }
}
