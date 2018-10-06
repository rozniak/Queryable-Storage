using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oddmatics.PowerUser.Windows.QueryableStorage.Configuration
{
    /// <summary>
    /// Specifies constants that define available configuration keys.
    /// </summary>
    internal static class ConfigurationKey
    {
        /// <summary>
        /// The "Database Connector Name" configuration key.
        /// </summary>
        public const string DatabaseConnectorName = "db-connector";

        /// <summary>
        /// The "Database Host Name" configuration key.
        /// </summary>
        public const string DatabaseHostName = "db-host";

        /// <summary>
        /// The "Database Password" configuration key.
        /// </summary>
        public const string DatabasePassword = "db-password";

        /// <summary>
        /// The "Database Port" configuration key.
        /// </summary>
        public const string DatabasePort = "db-port";

        /// <summary>
        /// The "Database Username" configuration key.
        /// </summary>
        public const string DatabaseUsername = "db-username";
    }
}
