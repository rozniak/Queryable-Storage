using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oddmatics.PowerUser.Windows.QueryableStorage.Database.Wrappers.MariaDb
{
    /// <summary>
    /// Represents a connection interface to a MariaDB server instance.
    /// </summary>
    internal sealed class MariaDbConnection : IDatabaseConnection
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public MariaDbConnection(
            string hostname,
            ushort port,
            string username,
            string password
            )
        {

        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public DatabaseResults Select()
        {
            throw new NotImplementedException();
        }
    }
}
