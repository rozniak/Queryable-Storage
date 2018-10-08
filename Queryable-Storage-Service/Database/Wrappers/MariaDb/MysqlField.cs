using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Oddmatics.PowerUser.Windows.QueryableStorage.Database.Wrappers.MariaDb
{
    /// <summary>
    /// Represents metadata about a field.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct MysqlField
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Name;

        [MarshalAs(UnmanagedType.LPStr)]
        public string OriginalName;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Table;

        [MarshalAs(UnmanagedType.LPStr)]
        public string OriginalTable;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Database;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Catalog;

        [MarshalAs(UnmanagedType.LPStr)]
        public string DefaultValue;

        [MarshalAs(UnmanagedType.U8)]
        public ulong Length;

        [MarshalAs(UnmanagedType.U8)]
        public ulong MaxLength;

        [MarshalAs(UnmanagedType.U4)]
        public uint NameLength;

        [MarshalAs(UnmanagedType.U4)]
        public uint OriginalNameLength;

        [MarshalAs(UnmanagedType.U4)]
        public uint DatabaseLength;

        [MarshalAs(UnmanagedType.U4)]
        public uint CatalogLength;

        [MarshalAs(UnmanagedType.U4)]
        public uint DefaultValueLength;

        [MarshalAs(UnmanagedType.U4)]
        public uint Flags;

        [MarshalAs(UnmanagedType.U4)]
        public uint Decimals;

        [MarshalAs(UnmanagedType.U4)]
        public uint CharsetId;

        [MarshalAs(UnmanagedType.I4)]
        public int FieldType;
        
        public IntPtr Extension;
    }
}
