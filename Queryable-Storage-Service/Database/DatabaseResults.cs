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


        /// <summary>
        /// Represents database results with no data.
        /// </summary>
        public static readonly DatabaseResults Empty = new DatabaseResults(
            new List<string>().AsReadOnly(),
            new List<ReadOnlyCollection<string>>().AsReadOnly()
            );


        /// <summary>
        /// Initializes a new instance of the DatabaseResults class.
        /// </summary>
        /// <param name="resultSet">The result set as an array of arrays of strings.</param>
        public DatabaseResults(
            ReadOnlyCollection<string> fields,
            ReadOnlyCollection<ReadOnlyCollection<string>> rows
            )
        {
            Fields = fields;
            Rows = rows;
        }
    }
}
