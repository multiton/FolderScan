using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;

namespace DiskWatcher
{
    public static class Utils
    {
        public static string SelectFolder(string selectedFolder)
        {
            var folderBrowserDialog = new FolderBrowserDialog
            {
                SelectedPath = selectedFolder,
                ShowNewFolderButton = false,
                Description = "Select folder to scan all files for MD5 digest, or click Cancel button to exit. Click OK button to start."                
            };

            var res = folderBrowserDialog.ShowDialog();

            return folderBrowserDialog.SelectedPath;
        }

        public static IEnumerable<string> Traverse(string rootDirectory)
        {
            var files = Enumerable.Empty<string>();
            var directories = Enumerable.Empty<string>();

            try
            {
                var permission = new FileIOPermission(FileIOPermissionAccess.PathDiscovery, rootDirectory);
                permission.Demand();

                files = Directory.GetFiles(rootDirectory);
                directories = Directory.GetDirectories(rootDirectory);
            }
            catch
            {
                rootDirectory = null;
            }

            if (rootDirectory != null) yield return rootDirectory;

            foreach (var file in files)
            {
                yield return file;
            }

            var subdirectoryItems = directories.SelectMany(Traverse);

            foreach (var result in subdirectoryItems)
            {
                yield return result;
            }
        }
    }
}