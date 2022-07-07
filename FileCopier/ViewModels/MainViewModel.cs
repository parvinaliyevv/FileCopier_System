using System.IO;
using System.Windows;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using FileCopier.Commands;
using Ookii.Dialogs.Wpf;

namespace FileCopier.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Thread CopierThread { get; set; }
        public bool ThreadStarted { get; set; }

        public RelayCommand GetSourceFilePathCommand { get; set; }
        public RelayCommand GetDestinationFilePathCommand { get; set; }

        public RelayCommand SuspendCommand { get; set; }
        public RelayCommand ResumeCommand { get; set; }
        public RelayCommand AbortCommand { get; set; }
        public RelayCommand CopyCommand { get; set; }


        private long _progressBarValue;
        public long ProgressBarValue
        {
            get { return _progressBarValue; }
            set { _progressBarValue = value; OnPropertyChanged(); }
        }

        private long _progressBarMaximumValue;
        public long ProgressBarMaximumValue
        {
            get { return _progressBarMaximumValue; }
            set { _progressBarMaximumValue = value; OnPropertyChanged(); }
        }

        private string _sourceFilePathValue;
        public string SourceFilePathValue
        {
            get { return _sourceFilePathValue; }
            set { _sourceFilePathValue = value; OnPropertyChanged(); }
        }

        private string _destinationFilePathValue;
        public string DestinationFilePathValue
        {
            get { return _destinationFilePathValue; }
            set { _destinationFilePathValue = value; OnPropertyChanged(); }
        }


        public MainViewModel()
        {
            CopierThread = new Thread(() => CopyFile());
            ThreadStarted = false;

            GetSourceFilePathCommand = new RelayCommand((sender) =>
            {
                var path = GetFilePath();

                if (path != null) SourceFilePathValue = path;
            }, (sender) => !ThreadStarted);
            GetDestinationFilePathCommand = new RelayCommand((sender) =>
            {
                var path = GetFilePath();

                if (path != null) DestinationFilePathValue = path;
            }, (sender) => !ThreadStarted);

            SourceFilePathValue = string.Empty;
            DestinationFilePathValue = string.Empty;

            ProgressBarValue = default;
            ProgressBarMaximumValue = 100;

            CopyCommand = new RelayCommand((sender) => { CopierThread?.Start(); ThreadStarted = true; }, (sender) => ThreadStarted is false && !string.IsNullOrWhiteSpace(SourceFilePathValue) && !string.IsNullOrWhiteSpace(DestinationFilePathValue));
            ResumeCommand = new RelayCommand((sender) => CopierThread?.Resume(), (sender) => CopierThread.ThreadState == ThreadState.Suspended);
            SuspendCommand = new RelayCommand((sender) => CopierThread?.Suspend(), (sender) => ThreadStarted && CopierThread.ThreadState != ThreadState.Suspended && CopierThread.ThreadState != ThreadState.Aborted);
            AbortCommand = new RelayCommand((sender) =>
            {
                CopierThread?.Abort();

                CopierThread = new Thread(() => CopyFile());
                ThreadStarted = false;

                ProgressBarValue = default;
                ProgressBarMaximumValue = 100;

            }, (sender) => CopierThread.ThreadState != ThreadState.Suspended && ThreadStarted);
        }


        public string GetFilePath()
        {
            var dialog = new VistaOpenFileDialog();
            dialog.Filter = "TXT Files|*.txt";

            return (dialog.ShowDialog() is true) ? dialog.FileName : null;
        }

        public void CopyFile()
        {
            if (!File.Exists(SourceFilePathValue))
            {
                MessageBox.Show("Source file path wrong.", "File not found!", MessageBoxButton.OK, MessageBoxImage.Error);

                CopierThread = new Thread(() => CopyFile());
                ThreadStarted = false;

                return;
            }
            else if (!File.Exists(DestinationFilePathValue))
            {
                MessageBox.Show("Destination file path wrong.", "File not found!", MessageBoxButton.OK, MessageBoxImage.Error);

                CopierThread = new Thread(() => CopyFile());
                ThreadStarted = false;

                return;
            }
            else if (SourceFilePathValue == DestinationFilePathValue)
            {
                MessageBox.Show("Same names for source and target files.", "File copy error!", MessageBoxButton.OK, MessageBoxImage.Error);

                CopierThread = new Thread(() => CopyFile());
                ThreadStarted = false;

                return;
            }

            using (var sourceFileStream = new FileStream(SourceFilePathValue, FileMode.Open, FileAccess.Read))
            {
                using (var destinationFileStream = new FileStream(DestinationFilePathValue, FileMode.Open, FileAccess.Write))
                {
                    destinationFileStream.Seek(destinationFileStream.Length, SeekOrigin.Current);
                    ProgressBarMaximumValue = sourceFileStream.Length;

                    byte lengthArray = 10;
                    byte[] byteArray = null;

                    for (int i = 0; i < ProgressBarMaximumValue; i += lengthArray)
                    {
                        byteArray = new byte[lengthArray];

                        sourceFileStream.Read(byteArray, 0, lengthArray);
                        destinationFileStream.Write(byteArray, 0, lengthArray);

                        ProgressBarValue += lengthArray;

                        Thread.Sleep(5);
                    }

                    MessageBox.Show("File copy successfully completed.", "Operation Completed!", MessageBoxButton.OK, MessageBoxImage.Information);

                    ThreadStarted = false;
                    ProgressBarValue = default;
                    ProgressBarMaximumValue = 100;

                    CopierThread = new Thread(() => CopyFile());
                }
            }
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
