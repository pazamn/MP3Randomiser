using System;
using System.IO;
using System.Linq;
using System.Windows;
using FileRandomiser.Helpers;
using FileRandomiser.Window;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace FileRandomiser.MainLogic
{
    public static class Worker
    {
        public static void Randomize(MainWindowViewModel viewModel)
        {
            try
            {
                if (!Directory.Exists(viewModel.SourceFolder))
                {
                    throw new Exception(string.Format("Source directory {0} does not exist.", viewModel.SourceFolder));
                }

                if (!Directory.Exists(viewModel.TargetFolder))
                {
                    throw new Exception(string.Format("Target directory {0} does not exist.", viewModel.TargetFolder));
                }

                if (viewModel.SplitByFolders && viewModel.SplitBy <= 0)
                {
                    throw new Exception(string.Format("Split by value is invalid: {0}.", viewModel.SplitBy));
                }

                viewModel.InfiniteProgress = true;
                if (viewModel.ClearBefore)
                {
                    var thingsToRemove = Directory.GetFileSystemEntries(viewModel.TargetFolder);
                    foreach (var entry in thingsToRemove)
                    {
                        Directory.Delete(entry, true);
                    }
                }
                
                var supportedExtensions = viewModel.CopyingFormats.Split(',').Select(x => "." + x.Trim().ToLowerInvariant()).ToList();
                var allFiles = Directory.EnumerateFiles(viewModel.SourceFolder, "*.*", SearchOption.AllDirectories).ToList();
                var interestingFiles = allFiles.Where(x => supportedExtensions.Contains((Path.GetExtension(x) ?? string.Empty).ToLowerInvariant())).ToList();

                interestingFiles.Shuffle();
                interestingFiles.Shuffle();
                interestingFiles.Shuffle();

                var filesLengths = interestingFiles.ToDictionary(x => x, x => new FileInfo(x).Length);
                var targetDrive = DriveInfo.GetDrives().Single(x => x.RootDirectory.FullName == Directory.GetDirectoryRoot(viewModel.TargetFolder));

                long loadingLength = 0;
                var loadingFiles = filesLengths.TakeWhile(x => { loadingLength += x.Value; return loadingLength < targetDrive.TotalFreeSpace; }).Select(x => x.Key).ToList();

                var filesPadCount = loadingFiles.Count.ToString().Count();
                var directoriesPadCount = Convert.ToInt32(Math.Ceiling((double) loadingFiles.Count / viewModel.SplitBy)).ToString().Count();

                var splittedDirectoryNumber = 1;
                var filesInCurrentDirectory = 0;

                viewModel.InfiniteProgress = false;
                viewModel.MaxValue = loadingFiles.Count;
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);

                foreach (var file in loadingFiles)
                {
                    var currentDirectoryName = string.Format("D{0}", splittedDirectoryNumber.ToString().PadLeft(directoriesPadCount, '0'));
                    var currentDirectoryPath = Path.Combine(viewModel.TargetFolder, currentDirectoryName);
                    if (!Directory.Exists(currentDirectoryPath))
                    {
                        Directory.CreateDirectory(currentDirectoryPath);
                    }

                    filesInCurrentDirectory++;
                    if (viewModel.SplitByFolders && filesInCurrentDirectory >= viewModel.SplitBy)
                    {
                        splittedDirectoryNumber++;
                        filesInCurrentDirectory = 0;
                    }

                    var index = loadingFiles.IndexOf(file);
                    var fileName = string.Format("R{0}_{1}", index.ToString().PadLeft(filesPadCount, '0'), Path.GetFileName(file));
                    var filePath = Path.Combine(viewModel.TargetFolder, currentDirectoryName, fileName);

                    File.Copy(file, filePath);

                    viewModel.Progress = index + 1;
                    TaskbarManager.Instance.SetProgressValue(index + 1, loadingFiles.Count);
                }

                viewModel.InfiniteProgress = false;
                viewModel.Progress = 0;
            }
            catch (Exception e)
            {
                var message = string.Format("Exception occured.\r\n{0}\r\n{1}", e.GetType().Name, e.Message);
                MessageBox.Show(message, "Fatal error.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}