using Oddmatics.PowerUser.Windows.QueryableStorage.Configuration;
using Oddmatics.PowerUser.Windows.QueryableStorage.Database;
using Oddmatics.PowerUser.Windows.QueryableStorage.FileSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Oddmatics.PowerUser.Windows.QueryableStorage
{
    /// <summary>
    /// Represents the Windows Service for the Queryable Storage daemon.
    /// </summary>
    internal partial class ServiceMain : ServiceBase
    {
        /// <summary>
        /// The configurations for this service.
        /// </summary>
        private ServiceConfiguration Configuration;

        /// <summary>
        /// The active database connection.
        /// </summary>
        private IDatabaseConnection DatabaseConnection;

        /// <summary>
        /// The file system monitor for watching changes to files and folders.
        /// </summary>
        private FileSystemMonitor FileSystemMonitor;


        /// <summary>
        /// Initializes a new instance of the ServiceMain class.
        /// </summary>
        internal ServiceMain()
        {
            InitializeComponent();
        }


        #region ServiceBase Method Implementations

        /// <summary>
        /// Executes when a Start command is sent to this service.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            // Load configurations
            //
            Configuration = new ServiceConfiguration();

            // Link up database
            //
            DatabaseConnection = DatabaseConnectionFactory.GetConnection(
                Configuration[ConfigurationKey.DatabaseConnectorName],
                Configuration[ConfigurationKey.DatabaseHostName],
                Convert.ToUInt16(Configuration[ConfigurationKey.DatabasePort]),
                Configuration[ConfigurationKey.DatabaseUsername],
                Configuration[ConfigurationKey.DatabasePassword]
                );

            // Set up the file system monitor
            //
            FileSystemMonitor = new FileSystemMonitor(DatabaseConnection);
        }

        /// <summary>
        /// Executes when a Stop command is sent to this service.
        /// </summary>
        protected override void OnStop()
        {
            FileSystemMonitor.Dispose();
        }

        #endregion
    }
}
