using Oddmatics.PowerUser.Windows.QueryableStorage.Database.Wrappers.MariaDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oddmatics.PowerUser.Windows.QueryableStorage.Database
{
    /// <summary>
    /// Provides methods for creating database connections using abstracted drivers.
    /// </summary>
    internal static class DatabaseConnectionFactory
    {
        /// <summary>
        /// Creates a new database connection using the abstracted driver of the specified name.
        /// </summary>
        /// <param name="connectorName">The name of the abstracted database driver.</param>
        /// <returns>A database connection instance if successful.</returns>
        public static IDatabaseConnection GetConnection(
            string connectorName,
            string hostname,
            ushort port,
            string username,
            string password
            )
        {
            switch (connectorName.ToLower())
            {
                case "mariadb":
                    return new MariaDbConnection(
                        hostname,
                        port,
                        username,
                        password
                        );

                default:
                    throw new ArgumentException(
                        String.Format(
                            "DatabaseConnectionFactory.GetConnection: Unknown connector '{0}'.",
                            connectorName
                            )
                        );
            }
        }
    }
}
