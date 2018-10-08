using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Oddmatics.PowerUser.Windows.QueryableStorage.Database.Wrappers.MariaDb
{
    /// <summary>
    /// Represents a connection interface to a MariaDB server instance.
    /// </summary>
    internal sealed class MariaDbConnection : IDatabaseConnection
    {
        /// <summary>
        /// The name of the main database to use for the "Queryable Storage" service.
        /// </summary>
        private const string QueryableStorageDatabaseName = "querystordb";


        /// <summary>
        /// Gets the value that indicates whether this connection is disposing or disposed.
        /// </summary>
        public bool Disposing { get; private set; }

        /// <summary>
        /// The MYSQL * handle.
        /// </summary>
        private IntPtr MysqlHandle;

        /// <summary>
        /// The last MariaDB error message, if any.
        /// </summary>
        private string LastError
        {
            get
            {
                IntPtr szErrorPtr = MysqlError(MysqlHandle);

                return Marshal.PtrToStringAnsi(szErrorPtr);
            }
        }

        /// <summary>
        /// The last MariaDB error code.
        /// </summary>
        private int LastErrorCode
        {
            get
            {
                return MysqlErrNo(MysqlHandle);
            }
        }


        /// <summary>
        /// Initializes a new instance of the MariaDbConnection class.
        /// </summary>
        /// <param name="hostname">The MariaDB server hostname.</param>
        /// <param name="port">The MariaDB server port.</param>
        /// <param name="username">The username to use when connecting.</param>
        /// <param name="password">The password to use when connecting.</param>
        public MariaDbConnection(
            string hostname,
            ushort port,
            string username,
            string password
            )
        {
            // Attempt set up of the MariaDB connection, acquire a handle
            //
            MysqlHandle = MysqlInit(IntPtr.Zero);

            if (MysqlRealConnect(MysqlHandle, hostname, username, password, "", port, "", 0) == IntPtr.Zero)
            {
                string lastError = LastError;

                MysqlClose(MysqlHandle);

                throw new Exception(
                    String.Format(
                        "MariaDbConnection: {0}",
                        lastError
                        )
                    );
            }

            // Try to access the database, if it doesn't exist, try to make it now
            //
            SafeQuery("USE ?n;", QueryableStorageDatabaseName);
        }


        /// <summary>
        /// Releases all resources used by this <see cref="MariaDbConnection"/> object.
        /// </summary>
        public void Dispose()
        {
            AssertNotDisposed();

            Disposing = true;

            MysqlClose(MysqlHandle);
        }

        public DatabaseResults Select()
        {
            throw new NotImplementedException();
        }


        #region Private Helper Methods

        /// <summary>
        /// Asserts that this instance is not disposed.
        /// </summary>
        private void AssertNotDisposed()
        {
            if (Disposing)
                throw new ObjectDisposedException("MariaDbConnection");
        }

        /// <summary>
        /// Constructs a <see cref="DatabaseResults"/> object usable outside of the
        /// abstraction layer of this class.
        /// </summary>
        /// <param name="fields">
        /// The collection of <see cref="MysqlField"/> objects.
        /// </param>
        /// <param name="rows">
        /// The <see cref="string[][]"/> array representing rows.
        /// </param>
        /// <returns>
        /// A <see cref="DatabaseResults"/> object that is usable outside of the
        /// abstraction layer of this class.
        /// </returns>
        private DatabaseResults ConstructDatabaseResults(
            IList<MysqlField> fields,
            ReadOnlyCollection<ReadOnlyCollection<string>> rows
            )
        {
            var fieldsStr = new List<string>();

            foreach (MysqlField mysqlField in fields)
            {
                fieldsStr.Add(mysqlField.Table + "." + mysqlField.Name);
            }

            return new DatabaseResults(fieldsStr.AsReadOnly, )
        }

        /// <summary>
        /// Executes a query on the MariaDB server.
        /// </summary>
        /// <param name="query">The query statement.</param>
        private void ExecuteQuery(string query)
        {
            AssertNotDisposed();

            int successfulQuery = MysqlQuery(MysqlHandle, query);

            if (successfulQuery != 0)
            {
                throw new InvalidOperationException(
                    String.Format(
                        "MariaDbConnection.ExecuteQuery: Query error ({0}), {1}.",
                        LastErrorCode,
                        LastError
                        )
                    );
            }
        }

        /// <summary>
        /// Fetches the field headings from a result set via its handle.
        /// </summary>
        /// <param name="hResultset">
        /// An <see cref="IntPtr"/> handle to the result set.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="MysqlField"/> objects representing the
        /// fields acquired from the resultset.
        /// </returns>
        private ReadOnlyCollection<MysqlField> FetchFields(IntPtr hResultset)
        {
            int sizeOfMysqlField = Marshal.SizeOf(typeof(MysqlField));

            IntPtr arrayIndexPtr = MysqlFetchFields(hResultset);
            int fieldCount = MysqlNumFields(hResultset);
            var fields = new List<MysqlField>();

            for (int i = 0; i < fieldCount; i++)
            {
                // Marshal a field and add it to our collection
                //
                fields.Add(
                    (MysqlField)Marshal.PtrToStructure(arrayIndexPtr, typeof(MysqlField))
                    );

                // Shift the pointer in the array to the next structure
                //
                arrayIndexPtr = IntPtr.Add(
                    arrayIndexPtr,
                    sizeOfMysqlField - IntPtr.Size
                    );
            }

            // Return a read only copy of our (now managed) field objects
            //
            return fields.AsReadOnly();
        }

        /// <summary>
        /// Fetches the results of the previous query into a
        /// <see cref="DatabaseResults"/> instance.
        /// </summary>
        /// <returns>
        /// A <see cref="DatabaseResults"/> instance containing the result set of the
        /// last query.
        /// </returns>
        private DatabaseResults FetchResults()
        {
            AssertNotDisposed();

            int fieldCount = MysqlFieldCount(MysqlHandle);

            // Check if there are any results, if there aren't respond with the empty
            // object
            //
            if (fieldCount == 0)
                return DatabaseResults.Empty;

            // We have results! Time to parse them from unmanaged memory into our neat
            // object
            //
            IntPtr hResultset = MysqlStoreResult(MysqlHandle);

            IList<MysqlField> fields = FetchFields(hResultset);
            ReadOnlyCollection<ReadOnlyCollection<string>> rows = FetchRows(hResultset);

            // Construct a DatabaseResults object that we can return
            //
            return ConstructDatabaseResults(fields, rows);
        }

        /// <summary>
        /// Fetches the rows from a result set via its handle.
        /// </summary>
        /// <param name="hResultset">
        /// An <see cref="IntPtr"/> handle to the result set.
        /// </param>
        /// <returns>
        /// A <see cref="ReadOnlyCollection{T}"/> instance containing the row data.
        /// </returns>
        private ReadOnlyCollection<ReadOnlyCollection<string>> FetchRows(IntPtr hResultset)
        {
            int fieldCount = MysqlNumFields(hResultset);
            IntPtr nextRow = IntPtr.Zero;
            var rows = new List<ReadOnlyCollection<string>>();

            while ((nextRow = MysqlFetchRow(hResultset)) != IntPtr.Zero)
            {
                IntPtr fieldPtr = nextRow;
                var fields = new List<string>();

                for (int i = 0; i < fieldCount; i++)
                {
                    IntPtr szFieldValue = Marshal.ReadIntPtr(fieldPtr);
                    string fieldValue = Marshal.PtrToStringAnsi(szFieldValue);

                    fields.Add(fieldValue);

                    fieldPtr = IntPtr.Add(fieldPtr, IntPtr.Size);
                }

                rows.Add(fields.AsReadOnly());
            }

            return rows.AsReadOnly();
        }

        /// <summary>
        /// Encodes a string such that it is safe to pass in an SQL statement.
        /// </summary>
        /// <param name="unsafeString">The potentially unsafe string to encode.</param>
        /// <returns>The unsafe string now encoded such that it is safe to pass in an SQL statement.</returns>
        private string SafeEncode(string unsafeString)
        {
            AssertNotDisposed();

            int unsafeStringLength = unsafeString.Length;
            var encodedBuffer = new StringBuilder(unsafeStringLength * 2 + 1);

            ulong encodedStringLength = MysqlRealEscapeString(
                MysqlHandle,
                encodedBuffer,
                unsafeString,
                (ulong)unsafeStringLength
                );

            return encodedBuffer.ToString(0, (int)encodedStringLength);
        }

        /// <summary>
        /// Encodes a query statement so that it is safe to submit to MariaDB.
        /// </summary>
        /// <param name="query">
        /// The query statement with placeholder tokens (?i, ?n, ?s) instead of values.
        /// </param>
        /// <param name="tokenValues">The values to insert at each placeholder token.</param>
        /// <returns>The query statement now encoded such that it is safe to submit to MariaDB.</returns>
        private string SafeEncodeQuery(string query, string[] tokenValues)
        {
            // Build the query by replacing all placeholder tokens with proper values
            //
            const string tokenMatchRegex = @"\?[ins]";

            int charPosition = 0;
            string statement = String.Empty;
            MatchCollection tokenMatches = Regex.Matches(query, tokenMatchRegex);
            int tokenMatchCount = tokenMatches.Count;

            if (tokenMatchCount != tokenValues.Length)
            {
                throw new ArgumentException(
                    "MariaDbConnection.SafeEncodeQuery: Placeholder token count did not match count of suppled values."
                    );
            }

            for (int i = 0; i < tokenMatchCount; i++)
            {
                Match match = tokenMatches[i];
                string value = tokenValues[i];

                string result = String.Empty;

                switch (match.Value.ToLower())
                {
                    case "?i":
                        long parsedInt = -1;

                        if (!long.TryParse(value, out parsedInt))
                        {
                            throw new ArgumentException(
                                "MariaDbConnection.SafeEncodeQuery: Encountered non-numeric type for integer placeholder token."
                                );
                        }

                        result = parsedInt.ToString();

                        break;

                    case "?n":
                        result = String.Format(
                            "`{0}`",
                            SafeEncode(value)
                            );

                        break;

                    case "?s":
                        result = String.Format(
                            "\"{0}\"",
                            SafeEncode(value)
                            );

                        break;
                }

                // Now we need to append the next chunk of the original query plus our
                // replaced placeholder token, and forward the current index pointer
                //
                int index = match.Index;
                int diff = charPosition - index;
                int placeholderSize = match.Length;

                statement += query.Substring(charPosition, diff);
                statement += result;

                charPosition += diff + placeholderSize;
            }

            return statement;
        }
        
        /// <summary>
        /// Performs a query safely on the database.
        /// </summary>
        /// <param name="query">
        /// The query statement with placeholder tokens (?i, ?n, ?s) instead of values.
        /// </param>
        /// <param name="tokenValues">The values to insert at each placeholder token.</param>
        private DatabaseResults SafeQuery(string query, params string[] tokenValues)
        {
            AssertNotDisposed();

            string safeQuery = SafeEncodeQuery(query, tokenValues);

            ExecuteQuery(safeQuery);

            return FetchResults();
        }

        #endregion


        #region MariaDB C API P/Invoke Contracts

        /// <summary>
        /// Closes a previously opened connection.
        /// </summary>
        /// <param name="mysql">
        /// MySQL handle, which was previously allocated by
        /// <see cref="MysqlRealConnect(IntPtr, string, string, string, string, uint, string, ulong)"/>
        /// or
        /// <see cref="MysqlInit(IntPtr)"/>.
        /// </param>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_close")]
        private static extern void MysqlClose(IntPtr mysql);

        /// <summary>
        /// Gets the last error code for the most recent function call that can succeed or fail.
        /// </summary>
        /// <param name="mysql">
        /// An <see cref="IntPtr"/> to a MYSQL handle, which was previously allocated
        /// by <see cref="MysqlInit(IntPtr)"/>.
        /// </param>
        /// <returns>The last error code for the most recent function call that can succeed or fail,</returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_errno")]
        private static extern int MysqlErrNo(IntPtr mysql);

        /// <summary>
        /// Gets the last error message for the most recent function call that can succeed or fail.
        /// </summary>
        /// <param name="mysql">
        /// An <see cref="IntPtr"/> to a MYSQL handle, which was previously allocated
        /// by <see cref="MysqlInit(IntPtr)"/>.
        /// </param>
        /// <returns>The last error message for the most recent function call that can succeed or fail.</returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_error")]
        private static extern IntPtr MysqlError(IntPtr mysql);

        /// <summary>
        /// Fetches all fields from the result set as an array, each field contains a
        /// definition for a column of the result set.
        /// </summary>
        /// <param name="result">
        /// An <see cref="IntPtr"/> to a result set handle.
        /// </param>
        /// <returns>
        /// All fields from the result set as an <see cref="IntPtr"/> to the first
        /// index in the array.
        /// </returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_fetch_fields")]
        private static extern IntPtr MysqlFetchFields(IntPtr result);

        /// <summary>
        /// Fetches one row of data from the result set. Each subsequent call to this
        /// function will return the next row within the result set, or NULL if there
        /// are no more rows.
        /// </summary>
        /// <param name="result">
        /// An <see cref="IntPtr"/> to a result set handle.
        /// </param>
        /// <returns>
        /// One row of data from the result set as an <see cref="IntPtr"/> to the
        /// first index of an array of char arrays.
        /// </returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_fetch_row")]
        private static extern IntPtr MysqlFetchRow(IntPtr result);

        /// <summary>
        /// Gets the number of columns for the most recent query.
        /// </summary>
        /// <param name="mysql">
        /// An <see cref="IntPtr"/> to a MYSQL handle, which was previously allocated
        /// by <see cref="MysqlRealConnect(IntPtr, string, string, string, string, uint, string, ulong)"/>.
        /// </param>
        /// <returns>The number of columns for the most recent query.</returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_field_count")]
        private static extern int MysqlFieldCount(IntPtr mysql);

        /// <summary>
        /// Frees the memory associated with a result set.
        /// </summary>
        /// <param name="result">An <see cref="IntPtr"/> to a result set handle.</param>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_free_result")]
        private static extern void MysqlFreeResult(IntPtr result);

        /// <summary>
        /// Prepares an initializes a MYSQL structure to be used with
        /// <see cref="MysqlRealConnect(IntPtr, string, string, string, string, uint, string, ulong)"/>.
        /// </summary>
        /// <param name="mysql">An <see cref="IntPtr"/> to MYSQL or NULL.</param>
        /// <returns>
        /// A MYSQL * handle cast as an <see cref="IntPtr"/> or
        /// <see cref="IntPtr.Zero"/> if an error occurred.
        /// </returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_init")]
        private static extern IntPtr MysqlInit(IntPtr mysql);

        /// <summary>
        /// Gets the number of fields in a specified result set.
        /// </summary>
        /// <param name="result">An <see cref="IntPtr"/> to a result set handle.</param>
        /// <returns>The number of fields in a specified result set.</returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_num_fields")]
        private static extern int MysqlNumFields(IntPtr result);

        /// <summary>
        /// Gets the number of rows in a specified result set.
        /// </summary>
        /// <param name="result">An <see cref="IntPtr"/> to a result set handle.</param>
        /// <returns>The number of rows in a specified result set.</returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_num_rows")]
        private static extern ulong MysqlNumRows(IntPtr result);

        /// <summary>
        /// Performs a statement pointed to by the query against the database.
        /// </summary>
        /// <param name="mysql">
        /// An <see cref="IntPtr"/> to a MYSQL handle, which was previously allocated
        /// by <see cref="MysqlRealConnect(IntPtr, string, string, string, string, uint, string, ulong)"/>.
        /// </param>
        /// <param name="query">The statement to be performed.</param>
        /// <returns>Zero on success, non zero on failure.</returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_query")]
        private static extern int MysqlQuery(
            IntPtr mysql,
            [MarshalAs(UnmanagedType.LPStr)]
            string query
            );

        /// <summary>
        /// Establishes a connection to a database server.
        /// </summary>
        /// <param name="mysql">
        /// An <see cref="IntPtr"/> to a MYSQL handle, which was previously allocated
        /// by <see cref="MysqlInit(IntPtr)"/>.
        /// </param>
        /// <param name="host">
        /// Either a hostname or IP address. Passing <see cref="String.Empty"/> or
        /// "localhost" to this parameter, the local host is assumed. When possible,
        /// pipes will be used instead of the TCP/IP protocol.
        /// </param>
        /// <param name="user">The user name.</param>
        /// <param name="password">
        /// If provided or <see cref="String.Empty"/>, the server will attempt to
        /// authenticate the user against those user records which have no password
        /// only.
        /// </param>
        /// <param name="database">
        /// If provided will specify the default database to be used when performing
        /// queries.
        /// </param>
        /// <param name="port">
        /// The port number to attempt to connect to the server.
        /// </param>
        /// <param name="unixSocket">
        /// The socket or named pipe that should be used.
        /// </param>
        /// <param name="flags">
        /// The connection options flags.
        /// </param>
        /// <returns>
        /// A MYSQL * handle cast as an <see cref="IntPtr"/> or
        /// <see cref="IntPtr.Zero"/> if an error occurred.
        /// </returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_real_connect")]
        private static extern IntPtr MysqlRealConnect(
            IntPtr mysql,
            [MarshalAs(UnmanagedType.LPStr)]
            string host,
            [MarshalAs(UnmanagedType.LPStr)]
            string user,
            [MarshalAs(UnmanagedType.LPStr)]
            string password,
            [MarshalAs(UnmanagedType.LPStr)]
            string database = "",
            uint port = 0,
            [MarshalAs(UnmanagedType.LPStr)]
            string unixSocket = "",
            ulong flags = 0
            );

        /// <summary>
        /// Creates a legal SQL string that can be used in an SQL statement from a given string.
        /// </summary>
        /// <param name="mysql">
        /// An <see cref="IntPtr"/> to a MYSQL handle, which was previously allocated
        /// by <see cref="MysqlRealConnect(IntPtr, string, string, string, string, uint, string, ulong)"/>.
        /// </param>
        /// <param name="to">
        /// The buffer for the encoded string, the size of this buffer must be
        /// length * 2 + 1 bytes in case every character must be escaped and a null
        /// terminator appended.
        /// </param>
        /// <param name="from">The string that will be encoded.</param>
        /// <param name="length">The length of the string to encode.</param>
        /// <returns>The length of the encoded string.</returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mysql_real_escape_string")]
        private static extern ulong MysqlRealEscapeString(
            IntPtr mysql,
            [MarshalAs(UnmanagedType.LPStr)]
            StringBuilder to,
            [MarshalAs(UnmanagedType.LPStr)]
            string from,
            ulong length
            );

        /// <summary>
        /// Gets a buffered resultset from the last executed query.
        /// </summary>
        /// <param name="mysql">
        /// An <see cref="IntPtr"/> to a MYSQL handle, which was previously allocated
        /// by <see cref="MysqlRealConnect(IntPtr, string, string, string, string, uint, string, ulong)"/>.
        /// </param>
        /// <returns>
        /// An <see cref="IntPtr"/> to a resultset handle or <see cref="IntPtr.Zero"/>
        /// in case an error occurred or if the query didn't return data.
        /// </returns>
        [DllImport(@"C:\Program Files\MariaDB 10.3\lib\libmariadb.dll", EntryPoint = "mnysql_store_result")]
        private static extern IntPtr MysqlStoreResult(IntPtr mysql);

        #endregion
    }
}
