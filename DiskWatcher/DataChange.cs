namespace DiskWatcher
{
    public class DataChange : BaseViewModel
    {
        private bool selected;
        public bool Selected
        {
            get { return this.selected; }
            set
            {
                this.selected = value;
                RaisePropertyChanged(() => Selected);
            }
        }

        private string filePath;
        public string FilePath
        {
            get { return this.filePath; }
            set
            {                
                this.filePath = value;
                RaisePropertyChanged(() => FilePath);
            }
        }

        private string fileName;
        public string FileName
        {
            get { return this.fileName; }
            set
            {
                this.fileName = value;
                RaisePropertyChanged(() => FileName);
            }
        }

        private string changeType;
        public string ChangeType
        {
            get { return this.changeType; }
            set
            {
                this.changeType = value;
                RaisePropertyChanged(() => ChangeType);
            }
        }

        private string fileType;
        public string FileType
        {
            get { return this.fileType; }
            set
            {
                this.fileType = value;
                RaisePropertyChanged(() => FileType);
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get { return this.errorMessage; }
            set
            {
                this.errorMessage = value;
                RaisePropertyChanged(() => ErrorMessage);
            }
        }
    }
}