using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Oddmatics.PowerUser.Windows.QueryableStorage.Database
{
    /// <summary>
    /// Represents a result set acquired from a query on a database.
    /// </summary>
    internal sealed class DatabaseResults
    {
        /// <summary>
        /// Gets the field names of the result set.
        /// </summary>
        public ReadOnlyCollection<string> Fields { get; private set; }

        /// <summary>
        /// Gets the row data of the result set.
        /// </summary>
        public ReadOnlyCollection<ReadOnlyCollection<string>> Rows { get; private set; }


        public static readonly DatabaseResults Empty; // TODO: Implement this


        /// <summary>
        /// Initializes a new instance of the DatabaseResults class.
        /// </summary>
        /// <param name="resultSet">The result set as an array of arrays of strings.</param>
        public DatabaseResults(string[][] resultSet)
        {
            //
            // TODO: Implement this
            //
        }
    }
}
