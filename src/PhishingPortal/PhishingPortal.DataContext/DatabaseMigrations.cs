using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhishingPortal.DataContext
{
    public static class DatabaseMigrations
    {
        public static bool IsMigrated = false;
        // TODO: develop this
        /// <summary>
        /// apply migrations via db scripts
        /// </summary>
        public static void ApplyMigrations(CentralDbContext dbContext)
        {
            // execute scripts kept in the scripts folder and kep track of it
        }

        /// <summary>
        /// rollback pervious versions
        /// </summary>
        /// <param name="level"></param>
        public static void Rollback(int level)
        {
            // rollback scripts
        }

    }
}
