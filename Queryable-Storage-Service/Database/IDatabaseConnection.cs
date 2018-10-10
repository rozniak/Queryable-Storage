using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Oddmatics.PowerUser.Windows.QueryableStorage.Database
{
    /// <summary>
    /// Provides functionality for connecting to and interacting with a database.
    /// </summary>
    internal interface IDatabaseConnection : IDisposable
    {
        /// <summary>
        /// Gets or sets the reference to the Windows Service instance that owns this
        /// <see cref="IDatabaseConnection"/>.
        /// </summary>
        ServiceBase OwnerService { get; set; }


        /// <summary>
        /// Performs a selection query on the database and retrieves the results.
        /// </summary>
        /// <returns>The database results that resulted from the query.</returns>
        DatabaseResults Select();
    }
}
