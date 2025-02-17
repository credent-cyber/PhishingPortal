﻿using Microsoft.EntityFrameworkCore;
using PhishingPortal.DataContext;
using PhishingPortal.Dto;

namespace PhishingPortal.Services.Notification.Helper
{
    public class DbConnManager : IDbConnManager
    {
        private readonly List<PhishingPortalDbContext> _dicConnections;
        private readonly ILogger<DbConnManager> _logger;
        private CentralDbContext centralDbContext;

        private string _sqlLiteDbPath { get; } = "./../PhishingPortal.Server/App_Data";
        private bool _useSqlLite = false;

        public DbConnManager(ILogger<DbConnManager> logger, IConfiguration config, CentralDbContext centralDbContext)
        {
            _dicConnections = new List<PhishingPortalDbContext>();
            if (config != null)
            {
                _useSqlLite = config.GetValue<bool>("UseSqlLite");
                _sqlLiteDbPath = config.GetValue<string>("SqlLiteDbPath");

                logger.LogInformation($"UserSqlLite : {_useSqlLite}");
                logger.LogInformation($"SqlLiteDbpath  : {_sqlLiteDbPath}");
            }
            _logger = logger;
            this.centralDbContext = centralDbContext;
        }

        public DemoRequestor GetContext()
        {
            try
            {
                    var demoReq = centralDbContext.DemoRequestor.Where(o=>o.isNotified==false).FirstOrDefault();
                    if (demoReq == null)
                        _logger.LogInformation($"No new Demo Requestor found!!!");

                return demoReq;
            }
            catch(Exception ex)
            {
                _logger.LogInformation("Error : Unable to Get DemoRequestor");
                return null;
            }
        }
        public bool SetContext(DemoRequestor dm)
        {
                try
                {
                    dm.isNotified = true;
                    dm.ModifiedBy = "DemoRequestHandler";
                    dm.ModifiedOn = DateTime.Now;
                    centralDbContext.DemoRequestor.Update(dm);
                    centralDbContext.SaveChanges();
                    _logger.LogInformation($"DemoRequestor Notified");
                    return true;
                }
                catch(Exception ex)
                {
                    _logger.LogInformation("DemoRequestor Notify failed!");
                    return false;
                }
            
        }
        public DbContextOptionsBuilder<PhishingPortalDbContext> SetupDbContextBuilder(DemoRequestor requestor, string connString)
        {
            _logger.LogInformation($"Setting up db context builder for DemoRequestor...");
            var optionsBuilder = new DbContextOptionsBuilder<PhishingPortalDbContext>();

            //if (tenant.DatabaseOption == DbOptions.SqlLite)
            //{
            //    _logger.LogInformation($"Using sqllite");
            //    var cstr = connString.Replace("./App_Data", _sqlLiteDbPath);
            //    _logger.LogDebug($"ConnectionString: {connString}");
            //    optionsBuilder.UseSqlite(cstr);
            //}

            _logger.LogInformation($"Using mysql");
            optionsBuilder.UseMySql(connString, ServerVersion.AutoDetect(connString));



            return optionsBuilder;
        }
        public void Dispose()
        {
            //if (_dicConnections != null)
            //{
            //    _logger.LogInformation("Disposing all tenant db connections");
            //    foreach (var conn in _dicConnections.Values)
            //    {
            //        _logger.LogDebug($"{conn}");
            //        conn.Dispose();
            //    }

            //    _logger.LogInformation("Disposing all tenant db connections - done");
            //}

            //_dicConnections?.Clear();
        }
    }
}
