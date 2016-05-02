using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace FolderChangeScan
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var lastError = string.Empty;
            var stringifiedHash = new StringBuilder();
            var selectedFolder = ConfigurationManager.AppSettings.Get("LastSelectedFolder");

            if (string.IsNullOrEmpty(selectedFolder))
            {
                selectedFolder = "c:\\$Recycle.Bin";
            }

            while (true)
            {
                var folderBrowserDialog = new FolderBrowserDialog
                {
                    SelectedPath = selectedFolder,
                    ShowNewFolderButton = false,
                    Description = string.IsNullOrEmpty(lastError)
                        ? "Select folder to scan all files for MD5 digest, or click Cancel button to exit. Click OK button to start."
                        : lastError
                };

                var res = folderBrowserDialog.ShowDialog();
                if (res == DialogResult.Cancel || string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath)) return;

                var newHashes = new Dictionary<string, string>();
                var oldHashes = new Dictionary<string, string>();
                var modifiedFilesList = new StringBuilder();

                var md5Hasher = MD5.Create();

                while (true)
                {
                    string[] allFilesList = null;
                    lastError = string.Empty;
                    newHashes.Clear();
                    modifiedFilesList.Clear();

                    try
                    {
                        allFilesList = Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.*", SearchOption.AllDirectories);
                    }
                    catch (Exception ex)
                    {
                        lastError = string.Concat("ERROR: ", ex.Message);
                    }

                    if (allFilesList == null) break;

                    foreach (var file in allFilesList)
                    {
                        var nextFileInfo = new FileInfo(file);

                        var computedHash = md5Hasher.ComputeHash(File.ReadAllBytes(nextFileInfo.FullName));
                        
                        stringifiedHash.Clear();
                        foreach (var t in computedHash) stringifiedHash.Append(t.ToString("x2"));

                        newHashes.Add(nextFileInfo.FullName, stringifiedHash.ToString());

                        if (oldHashes.Any())
                        {
                            if (oldHashes.ContainsKey(nextFileInfo.FullName))
                            {
                                if (newHashes.ContainsKey(nextFileInfo.FullName))
                                {
                                    if (newHashes[nextFileInfo.FullName] != oldHashes[nextFileInfo.FullName])
                                    {
                                        modifiedFilesList.Append("Updated --> ").Append(nextFileInfo.Name).Append(Environment.NewLine);
                                    }
                                }
                            }
                            else
                            {
                                modifiedFilesList.Append("Created --> ").Append(nextFileInfo.Name).Append(Environment.NewLine);
                            }

                            oldHashes.Remove(nextFileInfo.FullName);
                        }
                    }

                    foreach (var oldHash in oldHashes)
                    {
                        modifiedFilesList.Append("Deleted --> ").Append(oldHash.Key).Append(Environment.NewLine);
                    }

                    oldHashes = newHashes.ToDictionary(h => h.Key, h => h.Value);

                    if (modifiedFilesList.Length == 0)
                    {
                        modifiedFilesList.Append("Modify some file(s) in selected folder and click OK...");
                    }

                    var dialogResult = MessageBox.Show(
                        modifiedFilesList.ToString(), allFilesList.Length + " files found in " + folderBrowserDialog.SelectedPath,
                        MessageBoxButtons.OKCancel, MessageBoxIcon.None, MessageBoxDefaultButton.Button1,
                        MessageBoxOptions.ServiceNotification);

                    if (dialogResult == DialogResult.Cancel) break;
                }
            }
        }
    }
}