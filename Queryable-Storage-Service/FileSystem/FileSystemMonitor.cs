using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Oddmatics.PowerUser.Windows.QueryableStorage.FileSystem
{
    internal sealed class FileSystemMonitor : IDisposable
    {
        private const string LogPath = @"C:\fswatch.log";


        private Dictionary<string, FileSystemWatcher> Watchers;


        /// <summary>
        /// Initializes a new instance of the FileSystemMonitor class.
        /// </summary>
        public FileSystemMonitor()
        {
            Watchers = new Dictionary<string, FileSystemWatcher>();
        }


        /// <summary>
        /// Adds a directory to the file system watcher list.
        /// </summary>
        /// <param name="path">The directory path.</param>
        public void AddMonitoredPath(string path)
        {
            if (Watchers.ContainsKey(path))
                throw new ArgumentException("FileSystemMonitor.AddMonitoredPath: Path is already being monitored.");

            // Set up our watcher
            //
            var watcher = new FileSystemWatcher();

            watcher.Filter = "*";
            watcher.Path = path;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.FileName;

            // Attach events
            //
            watcher.Changed += Watcher_Changed;
            watcher.Created += Watcher_Changed;
            watcher.Deleted += Watcher_Changed;
            watcher.Renamed += Watcher_Renamed;

            // Begin watching
            //
            watcher.EnableRaisingEvents = true;

            // Add to internal dictionary
            //
            Watchers.Add(path, watcher);
        }

        /// <summary>
        /// Releases all resources used by this FileSystemMonitor.
        /// </summary>
        public void Dispose()
        {
            // Remove all watchers
            //
            var paths = new List<string>(Watchers.Keys).AsReadOnly();

            foreach (string path in paths)
            {
                RemoveMonitoredPath(path);
            }
        }

        /// <summary>
        /// Removes a directory from the file system watcher list.
        /// </summary>
        /// <param name="path">The directory path.</param>
        public void RemoveMonitoredPath(string path)
        {
            if (!Watchers.ContainsKey(path))
                throw new ArgumentException("FileSystemMonitor.RemoveMonitoredPath: Path is not being monitored.");

            FileSystemWatcher watcher = Watchers[path];

            // Detach all events
            //
            watcher.EnableRaisingEvents = false;

            watcher.Changed -= Watcher_Changed;
            watcher.Created -= Watcher_Changed;
            watcher.Deleted -= Watcher_Changed;
            watcher.Renamed -= Watcher_Renamed;

            // Dispose
            //
            watcher.Dispose();

            // Remove from internal dictionary
            //
            Watchers.Remove(path);
        }


        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            File.AppendAllText(
                LogPath,
                String.Format(
                    "File Change: {0} ; {1}",
                    e.FullPath,
                    e.ChangeType
                    )
                );
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            File.AppendAllText(
                LogPath,
                String.Format(
                    "File Rename: {0} ; {1}",
                    e.OldFullPath,
                    e.FullPath
                    )
                );
        }
    }
}
