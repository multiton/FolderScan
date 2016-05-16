using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace DiskWatcher
{
    public class MainViewModel : BaseViewModel
    {
        private string selectedFolder;
        public string SelectedFolder
        {
            get { return this.selectedFolder; }
            set
            {
                this.selectedFolder = value;
                RaisePropertyChanged(() => SelectedFolder);
            }
        }

        private ObservableCollection<DataChange> fileChanges;
        public ObservableCollection<DataChange> FileChanges
        {
            get { return this.fileChanges ?? (this.fileChanges = new ObservableCollection<DataChange>()); }
            set
            {
                this.fileChanges = value;
                RaisePropertyChanged(() => FileChanges );
            }
        }

        private readonly MD5 md5Hasher = MD5.Create();
        private readonly StringBuilder stringifiedHash = new StringBuilder();
        private readonly Dictionary<string, string> newHashes = new Dictionary<string, string>();
        private readonly Dictionary<string, string> oldHashes = new Dictionary<string, string>();

        public DelegateCommand Scan
        {
            get { return new DelegateCommand(() =>
            {
                var allFilesInSelectedFolder = Utils.Traverse(this.SelectedFolder).ToArray();
            });}
        }

        public DelegateCommand SelectFolder
        {
            get { return new DelegateCommand(() =>
            {
                this.FileChanges.Clear();
                newHashes.Clear();

                this.SelectedFolder = Utils.SelectFolder(this.selectedFolder ?? "c:\\");
                var allFilesInSelectedFolder = Utils.Traverse(this.SelectedFolder).ToArray();

                foreach (var file in allFilesInSelectedFolder)
                {
                    var nextFileInfo = new FileInfo(file);

                    if (!nextFileInfo.Exists) continue;

                    var dataChange = new DataChange
                    {
                        FileName = nextFileInfo.Name,
                        FilePath = nextFileInfo.DirectoryName
                    };

                    if (nextFileInfo.Attributes.HasFlag(FileAttributes.ReparsePoint) ||
                        nextFileInfo.Attributes.HasFlag(FileAttributes.System))
                    {
                        dataChange.ErrorMessage = "System File";
                    }

                    if (nextFileInfo.Length > Int32.MaxValue)
                    {
                        dataChange.ErrorMessage = "Big File";
                    }

                    try
                    {
                        if (string.IsNullOrEmpty(dataChange.ErrorMessage))
                        {
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
                                            dataChange.ChangeType = "Updated";
                                        }
                                    }
                                }
                                else
                                {
                                    dataChange.ChangeType = "Created";
                                }
                                oldHashes.Remove(nextFileInfo.FullName);
                            }
                            else
                            {
                                //if (newHashes.Any() && !firstScan)
                                //{
                                //    modifiedFilesList.Append("CREATED ==> ").Append(nextFileInfo.FullName).Append(Environment.NewLine);
                                //}
                            }
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        dataChange.ErrorMessage = "Unauthorized Access";
                    }
                    catch (IOException)
                    {
                        dataChange.ErrorMessage = "IO Exception";
                    }

                    this.FileChanges.Add(dataChange);
                }
            });}
        }
    }
}