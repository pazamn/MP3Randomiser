using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using FileRandomiser.MainLogic;
using MessageBox = System.Windows.MessageBox;

namespace FileRandomiser.Window
{
    public partial class MainWindow
    {
        #region Private Things and Initialization

        private MainWindowViewModel ViewModel { get; set; }

        public MainWindow()
        {
            ViewModel = new MainWindowViewModel();
            DataContext = ViewModel;

            InitializeBaseValues();
            InitializeComponent();
        }

        private void InitializeBaseValues()
        {
            var sourceDrive = DriveInfo.GetDrives().FirstOrDefault(x => x.DriveType == DriveType.Fixed && x.IsReady && Directory.Exists(Path.Combine(x.RootDirectory.FullName, "Music")));
            var targetDrive = DriveInfo.GetDrives().FirstOrDefault(x => x.DriveType == DriveType.Removable && x.IsReady);

            ViewModel.SourceFolder = sourceDrive != null ? Path.Combine(sourceDrive.RootDirectory.FullName, "Music") : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ViewModel.TargetFolder = targetDrive != null ? targetDrive.RootDirectory.FullName : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            ViewModel.ClearBefore = false;
            ViewModel.SplitByFolders = true;
            ViewModel.SplitBy = 100;
            ViewModel.CopyingFormats = "mp3, wma";
            ViewModel.ClearID3Tags = false;

            ViewModel.TextBlockVisibility = Visibility.Visible;
            ViewModel.ProgressBarVisibility = Visibility.Collapsed;
            ViewModel.InfiniteProgress = false;
            ViewModel.Progress = 0;

            ViewModel.IsEnabled = true;
        }

        #endregion Private Things and Initialization

        #region Main Methods

        private void SelectSourceFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = ViewModel.SourceFolder, Description = "Source folder" };
            dialog.ShowDialog();

            ViewModel.SourceFolder = dialog.SelectedPath;
        }

        private void SelectTargetFolder(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog { SelectedPath = ViewModel.TargetFolder, Description = "Target folder" };
            dialog.ShowDialog();

            ViewModel.TargetFolder = dialog.SelectedPath;
        }

        private void Randomise(object sender, RoutedEventArgs e)
        {
            ViewModel.IsEnabled = false;
            ViewModel.TextBlockVisibility = Visibility.Collapsed;
            ViewModel.ProgressBarVisibility = Visibility.Visible;

            Task.Factory.StartNew(() => Worker.Randomize(ViewModel));
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you want to close?", "Close", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            Close();
        }

        #endregion Main Methods
    }
}