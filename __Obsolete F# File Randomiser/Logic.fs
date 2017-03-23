module Logic

open System
open System.IO
open System.Windows
open System.Windows.Controls
open System.Windows.Markup
open System.Windows.Media
open System.Windows.Forms
open System.Windows.Controls
open System.Threading
open System.Threading.Tasks
open System.Linq
open System.Globalization
open System.Diagnostics
open System.Text
open System.Text.RegularExpressions
open System.Collections
open System.Collections.Generic
open System.Collections.Specialized
open System.Collections.ObjectModel
open HundredMilesSoftware.UltraID3Lib
open Microsoft.Win32
open Microsoft.WindowsAPICodePack
open Microsoft.WindowsAPICodePack.Shell
open Microsoft.WindowsAPICodePack.Taskbar
open Microsoft.WindowsAPICodePack.Dialogs

//LogicWindow Fields
let mutable FatalError = false
let mutable Window = new Window(Topmost = true)
let InitialisationTextBlock = new TextBlock()
let InitialisationProgressBar = new ProgressBar()
let FindingFilesTextBlock = new TextBlock()
let FindingFilesProgressBar = new ProgressBar()
let RandomiseFilesTextBlock = new TextBlock()
let RandomiseFilesProgressBar = new ProgressBar()
let ClearFolderTextBlock = new TextBlock()
let ClearFolderProgressBar = new ProgressBar()
let CopyFilesAndClearTagsTextBlock = new TextBlock()
let CopyFilesAndClearTagsProgressBar = new ProgressBar()
let CloseButton = new Button()
let ErrorsListView = new ListView()

let Errors = new ObservableCollection<string>()

let InitialiseWindow =
    Window.WindowStyle <- WindowStyle.ToolWindow
    Window.WindowStartupLocation <- WindowStartupLocation.CenterScreen
    Window.Title <- "File Randomiser v1.15 - Индикатор процесса"
    Window.Width <- 640.0
    Window.MinWidth <- 640.0
    Window.MaxWidth <- 640.0
    Window.Height <- 480.0
    Window.MinHeight <- 480.0
    Window.MaxHeight <- 480.0
    Window.Closing.Add(fun eventArgs -> if Convert.ToInt32(CloseButton.DataContext) = 0 then
                                            let result = MessageBox.Show("Вы действительно хотите прервать процесс рандомизации и закрыть программу?", "Прервать рандомизацию?", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                                            if result = DialogResult.No then eventArgs.Cancel<- true)

    InitialisationTextBlock.Margin <- new Thickness(5.0, 5.0, 0.0, 5.0)
    InitialisationTextBlock.Text <- "Инициализация..."
    InitialisationTextBlock.FontSize <- 14.0
    InitialisationTextBlock.Visibility <- Visibility.Collapsed

    InitialisationProgressBar.Margin <- new Thickness(0.0, 5.0, 0.0, 5.0)
    InitialisationProgressBar.Width <- 615.0
    InitialisationProgressBar.Height <- 30.0
    InitialisationProgressBar.IsIndeterminate <- true
    InitialisationProgressBar.Visibility <- Visibility.Collapsed

    FindingFilesTextBlock.Margin <- new Thickness(5.0, 5.0, 0.0, 5.0)
    FindingFilesTextBlock.Text <- "Поиск файлов..."
    FindingFilesTextBlock.FontSize <- 14.0
    FindingFilesTextBlock.Visibility <- Visibility.Collapsed

    FindingFilesProgressBar.Margin <- new Thickness(0.0, 5.0, 0.0, 5.0)
    FindingFilesProgressBar.Width <- 615.0
    FindingFilesProgressBar.Height <- 30.0
    FindingFilesProgressBar.IsIndeterminate <- true
    FindingFilesProgressBar.Visibility <- Visibility.Collapsed

    RandomiseFilesTextBlock.Margin <- new Thickness(5.0, 5.0, 0.0, 5.0)
    RandomiseFilesTextBlock.Text <- "Рандомизация файлов..."
    RandomiseFilesTextBlock.FontSize <- 14.0
    RandomiseFilesTextBlock.Visibility <- Visibility.Collapsed

    RandomiseFilesProgressBar.Margin <- new Thickness(0.0, 5.0, 0.0, 5.0)
    RandomiseFilesProgressBar.Width <- 615.0
    RandomiseFilesProgressBar.Height <- 30.0
    RandomiseFilesProgressBar.IsIndeterminate <- true
    RandomiseFilesProgressBar.Visibility <- Visibility.Collapsed

    ClearFolderTextBlock.Margin <- new Thickness(5.0, 5.0, 0.0, 5.0)
    ClearFolderTextBlock.Text <- "Очистка целевой папки..."
    ClearFolderTextBlock.FontSize <- 14.0
    ClearFolderTextBlock.Visibility <- Visibility.Collapsed

    ClearFolderProgressBar.Margin <- new Thickness(0.0, 5.0, 0.0, 5.0)
    ClearFolderProgressBar.Width <- 615.0
    ClearFolderProgressBar.Height <- 30.0
    ClearFolderProgressBar.IsIndeterminate <- true
    ClearFolderProgressBar.Visibility <- Visibility.Collapsed

    CopyFilesAndClearTagsTextBlock.Margin <- new Thickness(5.0, 5.0, 0.0, 5.0)
    CopyFilesAndClearTagsTextBlock.Text <- "Копирование файлов и очистка ID3 тегов..."
    CopyFilesAndClearTagsTextBlock.FontSize <- 14.0
    CopyFilesAndClearTagsTextBlock.Visibility <- Visibility.Collapsed

    CopyFilesAndClearTagsProgressBar.Margin <- new Thickness(0.0, 5.0, 0.0, 5.0)
    CopyFilesAndClearTagsProgressBar.Width <- 615.0
    CopyFilesAndClearTagsProgressBar.Height <- 30.0
    CopyFilesAndClearTagsProgressBar.IsIndeterminate <- true
    CopyFilesAndClearTagsProgressBar.Visibility <- Visibility.Collapsed

    CloseButton.Margin <- new Thickness(0.0, 0.0, 0.0, 5.0)
    CloseButton.Width <- 400.0
    CloseButton.Height <- 40.0
    CloseButton.Content <- "Отмена"
    CloseButton.DataContext <- 0
    CloseButton.Click.Add(fun _ -> Window.Close() |> ignore)

    ErrorsListView.Margin <- new Thickness(0.0, 0.0, 0.0, 5.0)
    ErrorsListView.Width <- 615.0
    ErrorsListView.Height <- 210.0
    ErrorsListView.Visibility <- Visibility.Collapsed
    ErrorsListView.ItemsSource <- Errors

    let mainPanel = new StackPanel()
    mainPanel.Orientation <- Orientation.Vertical
    mainPanel.Children.Add(InitialisationTextBlock) |> ignore
    mainPanel.Children.Add(InitialisationProgressBar) |> ignore
    mainPanel.Children.Add(FindingFilesTextBlock) |> ignore
    mainPanel.Children.Add(FindingFilesProgressBar) |> ignore
    mainPanel.Children.Add(ClearFolderTextBlock) |> ignore
    mainPanel.Children.Add(ClearFolderProgressBar) |> ignore
    mainPanel.Children.Add(RandomiseFilesTextBlock) |> ignore
    mainPanel.Children.Add(RandomiseFilesProgressBar) |> ignore
    mainPanel.Children.Add(CopyFilesAndClearTagsTextBlock) |> ignore
    mainPanel.Children.Add(CopyFilesAndClearTagsProgressBar) |> ignore
    mainPanel.Children.Add(ErrorsListView) |> ignore
    mainPanel.Children.Add(CloseButton) |> ignore
    Window.Content <- mainPanel

    Window

//Get files list
let GetInterestingFiles (musicFolder : string) (resultFolder : string) (mask :string) (fileFormats : string) = 
    let allFiles = new List<string>()
    let resultFiles = new List<string>()

    let minimumPercentage = 0
    let maximumPercentage = 25

    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> FindingFilesTextBlock.Visibility <- Visibility.Visible
                                                                                 FindingFilesProgressBar.Visibility <- Visibility.Visible)) |> ignore

    try
        if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(minimumPercentage, 100)

        if Directory.Exists(musicFolder) && Directory.Exists(resultFolder) then
            let resultPathRoot = Path.GetPathRoot(resultFolder)
            let drive = DriveInfo.GetDrives().FirstOrDefault(fun(value : DriveInfo) -> value.Name = resultPathRoot)
            let availableFreeSpaceInMegabytes = Convert.ToDouble(drive.AvailableFreeSpace) / 1024.0 / 1024.0

            let formats = Regex.Split(fileFormats, ",")
            for i in formats do
                let currentFormat = i.Replace(" ", "")
                let currentFormatFiles = Directory.GetFiles(musicFolder, "*." + currentFormat, SearchOption.AllDirectories)
                allFiles.AddRange(currentFormatFiles)

            let mutable sizeInMegabytes = 0.0
            for i in allFiles do
                let currentFileInfo = new FileInfo(i)
                let currentFileInfoSize = Convert.ToDouble(currentFileInfo.Length) / 1024.0 / 1024.0
                sizeInMegabytes <- sizeInMegabytes + currentFileInfoSize

            if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(maximumPercentage / 5, 100)

            if sizeInMegabytes < availableFreeSpaceInMegabytes then
                resultFiles.AddRange(allFiles)

                if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(maximumPercentage, 100)

                Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> FindingFilesTextBlock.Text <- "Будет скопировано " + resultFiles.Count.ToString() + " файлов."
                                                                                             FindingFilesTextBlock.Foreground <- new SolidColorBrush(Colors.Green)
                                                                                             FindingFilesProgressBar.Visibility <- Visibility.Collapsed)) |> ignore
            else
                let percentage = availableFreeSpaceInMegabytes / sizeInMegabytes * 100.0

                let randomiser = new Random(int System.DateTime.Now.Ticks)
                let mutable availableSpace = availableFreeSpaceInMegabytes
                for i = 0 to allFiles.Count do
                    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> FindingFilesProgressBar.IsIndeterminate <- false
                                                                                                 FindingFilesProgressBar.Minimum <- 0.0
                                                                                                 FindingFilesProgressBar.Maximum <- Convert.ToDouble(allFiles.Count)
                                                                                                 FindingFilesProgressBar.Value <- Convert.ToDouble(i))) |> ignore

                    let mutable currentIterationRandomNumber = randomiser.Next(allFiles.Count)
                    let mutable currentIterationRandomPath = allFiles.Item(currentIterationRandomNumber)
                    while resultFiles.Contains(currentIterationRandomPath) do
                        currentIterationRandomNumber <- randomiser.Next(allFiles.Count)
                        currentIterationRandomPath <- allFiles.Item(currentIterationRandomNumber)

                    let currentFileInfo = new FileInfo(currentIterationRandomPath)
                    let currentFileInfoSize = Convert.ToDouble(currentFileInfo.Length) / 1024.0 / 1024.0
                    if availableSpace > currentFileInfoSize then
                        resultFiles.Add(currentIterationRandomPath)
                        availableSpace <- availableSpace - currentFileInfoSize

                    let percentageInfo = maximumPercentage / 5 + Convert.ToInt32((double)i / (double)allFiles.Count * (double)maximumPercentage * 3.0 / 5.0)
                    if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(percentageInfo, 100)

                if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(maximumPercentage * 4 / 5, 100)
                Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> FindingFilesProgressBar.IsIndeterminate <- true)) |> ignore

                let mutable displaySize = 0.0
                for i in resultFiles do
                    let currentFileInfo = new FileInfo(i)
                    let currentFileInfoSize = Convert.ToDouble(currentFileInfo.Length) / 1024.0 / 1024.0
                    displaySize <- displaySize + currentFileInfoSize

                if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(maximumPercentage, 100)
                Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> FindingFilesTextBlock.Text <- "Будет скопировано " + resultFiles.Count.ToString() + " файлов."
                                                                                             FindingFilesTextBlock.Foreground <- new SolidColorBrush(Colors.Green)
                                                                                             FindingFilesProgressBar.Visibility <- Visibility.Collapsed)) |> ignore
        else
            if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error)
            Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> FindingFilesTextBlock.Text <- "Одна из папок: '" + musicFolder + "' или '" + resultFolder + "' недоступна"
                                                                                         FindingFilesTextBlock.Foreground <- new SolidColorBrush(Colors.Red)
                                                                                         FindingFilesProgressBar.Visibility <- Visibility.Collapsed
                                                                                         FatalError <- true)) |> ignore
        
    with
        | :? System.Exception as e -> if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error)
                                      Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> FindingFilesTextBlock.Text <- e.Message
                                                                                                                   FindingFilesTextBlock.Foreground <- new SolidColorBrush(Colors.Red)
                                                                                                                   FindingFilesProgressBar.Visibility <- Visibility.Collapsed
                                                                                                                   FatalError <- true)) |> ignore

    resultFiles

//Procedure to get random file name
let GetRandomName (filesCount : int) (mask :string) (charsList : List<string>) =
    let mutable symbolsCount = (int) (ceil (log ((float) filesCount) / log ((float) charsList.Count)))
    let multiplicator = if symbolsCount < 3 then 10.0
                        elif symbolsCount < 5 then 8.0
                        elif symbolsCount < 7 then 6.0
                        elif symbolsCount < 9 then 4.0
                        elif symbolsCount < 11 then 3.0
                        elif symbolsCount < 13 then 2.0
                        else 1.5

    symbolsCount <- (int) (ceil ((float) symbolsCount * (float) multiplicator))
    symbolsCount <- if symbolsCount > 0 then symbolsCount else 1

    let mutable randomName = ""
    for i = 1 to symbolsCount do
        Thread.Sleep(1)
        let rnd = System.Random(int System.DateTime.Now.Ticks)
        let first = rnd.Next(1, charsList.Count)
        randomName <- randomName + charsList.Item(first)

    let result = mask.Replace("$(Rand)", randomName)
    result

//Procedure to get folder name
let GetFolderName (currentFileNumber : int) (allFilesCount : int) (separateToFolders : int) =
    let mutable result = ""

    if allFilesCount > separateToFolders then
        let foldersCount = Convert.ToInt32(Math.Ceiling(((float)allFilesCount / (float)separateToFolders)));

        let random = System.Random()
        let folderNumber = random.Next(1, foldersCount + 1)
        //let folderNumber = Convert.ToInt32(Math.Ceiling(((float)currentFileNumber + 1.0) / (float)separateToFolders))

        if folderNumber < 10 then
            result <- "D00" + folderNumber.ToString()
        elif folderNumber < 100 then
            result <- "D0" + folderNumber.ToString()
        elif folderNumber < 1000 then
            result <- "D" + folderNumber.ToString()
        else
            result <- "DX" + folderNumber.ToString()

    result

//Get random names dictionary
let GetRandomNamesDictionary (sourceFiles : List<string>) (resultFolder : string) (separateToFolders : int) (mask :string) (charsList : List<string>) = 
    let resultDictionary = new Dictionary<string, string>()
    let tempRandomNamesList = new List<string>()
    let mutable iterationCounter = 0

    let minimumPercentage = 26
    let maximumPercentage = 50

    if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(minimumPercentage, 100)
    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> RandomiseFilesTextBlock.Visibility <- Visibility.Visible
                                                                                 RandomiseFilesProgressBar.Visibility <- Visibility.Visible
                                                                                 RandomiseFilesProgressBar.IsIndeterminate <- false
                                                                                 RandomiseFilesProgressBar.Minimum <- 0.0
                                                                                 RandomiseFilesProgressBar.Maximum <- Convert.ToDouble(sourceFiles.Count))) |> ignore

    for currentFile in sourceFiles do
        let position = sourceFiles.IndexOf(currentFile)
        Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> RandomiseFilesTextBlock.Text <- "Рандомизация файлов... (" + position.ToString() + " из " + sourceFiles.Count.ToString() + ")"
                                                                                     RandomiseFilesProgressBar.Value <- Convert.ToDouble(position))) |> ignore

        let percentage = (int)((double)position / (double)sourceFiles.Count * (double)(maximumPercentage - minimumPercentage)) + minimumPercentage
        if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(percentage, 100)

        let targetFileExtension = Path.GetExtension(currentFile)
        let mutable targetRandomisedName = GetRandomName sourceFiles.Count mask charsList
        iterationCounter <- iterationCounter + 1
        while tempRandomNamesList.Contains(targetRandomisedName) do
            targetRandomisedName <- GetRandomName sourceFiles.Count mask charsList
            iterationCounter <- iterationCounter + 1

        let currentFilePosition = sourceFiles.IndexOf currentFile

        let mutable innerCycleLocker = true
        let mutable directoryName = ""
        while innerCycleLocker do
            let currentDirectoryName = GetFolderName currentFilePosition sourceFiles.Count separateToFolders
            let filesInCurrentFolder = tempRandomNamesList.LongCount(fun f -> f.ToString().Contains(currentDirectoryName))

            if filesInCurrentFolder < Convert.ToInt64(separateToFolders) then
                directoryName <- currentDirectoryName
                innerCycleLocker <- false

        //let filesInCurrentFolder = tempRandomNamesList.LongCount(fun f -> f.ToString().Contains(directoryName))

        let targetFileFullPath = Path.Combine(Path.Combine(resultFolder, directoryName), targetRandomisedName + targetFileExtension)

        resultDictionary.Add(currentFile, targetFileFullPath)
        tempRandomNamesList.Add(targetFileFullPath)

    let iterationCounterString = iterationCounter.ToString()
    if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(maximumPercentage, 100)
    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> RandomiseFilesTextBlock.Foreground <- new SolidColorBrush(Colors.Green)
                                                                                 RandomiseFilesTextBlock.Text <- "Рандомизация файлов завершена. Итераций: " + iterationCounterString
                                                                                 RandomiseFilesProgressBar.Visibility <- Visibility.Collapsed)) |> ignore

    resultDictionary

//Clear folder
let ClearFolder (resultFolder : string) = 
    let minimumPercentage = 51
    let maximumPercentage = 75

    try
        if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(minimumPercentage, 100)
        Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> ClearFolderTextBlock.Visibility <- Visibility.Visible
                                                                                     ClearFolderProgressBar.Visibility <- Visibility.Visible)) |> ignore

        if Directory.Exists(resultFolder) then
            let insideDirectories = Directory.GetDirectories(resultFolder).ToList()
            let insideFiles = Directory.GetFiles(resultFolder).ToList()

            if (insideDirectories.Any() || insideFiles.Any()) then
                Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> ClearFolderProgressBar.IsIndeterminate <- false
                                                                                             ClearFolderProgressBar.Minimum <- 0.0
                                                                                             ClearFolderProgressBar.Maximum <- Convert.ToDouble(insideDirectories.Count + insideFiles.Count))) |> ignore

                for currentDirectory in insideDirectories do
                    let inDirectoryFiles = Directory.GetFiles(currentDirectory)
                    for currentFile in inDirectoryFiles do
                        let currentFileInfo = new FileInfo(currentFile)
                        let fileAttributes = currentFileInfo.Attributes

                        if fileAttributes &&& FileAttributes.ReadOnly = FileAttributes.ReadOnly then
                            let newAttributes = fileAttributes - FileAttributes.ReadOnly;
                            currentFileInfo.Attributes <- newAttributes

                    Directory.Delete(currentDirectory, true)

                    let position = insideDirectories.IndexOf(currentDirectory)
                    let totalCount = insideDirectories.Count + insideFiles.Count
                    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> ClearFolderProgressBar.Value <- Convert.ToDouble(position))) |> ignore

                    let percentage = (int)((double)position / (double)totalCount * (double)(maximumPercentage - minimumPercentage)) + minimumPercentage
                    if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(percentage, 100)

                for currentFile in insideFiles do
                    let currentFileInfo = new FileInfo(currentFile)
                    let fileAttributes = currentFileInfo.Attributes

                    if fileAttributes &&& FileAttributes.ReadOnly = FileAttributes.ReadOnly then
                        let newAttributes = fileAttributes - FileAttributes.ReadOnly;
                        currentFileInfo.Attributes <- newAttributes 

                    File.Delete(currentFile)

                    let position = insideDirectories.Count + insideFiles.IndexOf(currentFile)
                    let totalCount = insideDirectories.Count + insideFiles.Count
                    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> ClearFolderProgressBar.Value <- Convert.ToDouble(position))) |> ignore

                    let percentage = (int)((double)position / (double)totalCount * (double)(maximumPercentage - minimumPercentage)) + minimumPercentage
                    if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(percentage, 100)

                Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> ClearFolderTextBlock.Foreground <- new SolidColorBrush(Colors.Green)
                                                                                             ClearFolderTextBlock.Text <- "Папка " + resultFolder + " очищена"
                                                                                             ClearFolderProgressBar.Visibility <- Visibility.Collapsed)) |> ignore
            else
                Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> ClearFolderTextBlock.Foreground <- new SolidColorBrush(Colors.Green)
                                                                                             ClearFolderTextBlock.Text <- "Папка " + resultFolder + " пустая"
                                                                                             ClearFolderProgressBar.Visibility <- Visibility.Collapsed)) |> ignore

            if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(maximumPercentage, 100)

        else
            if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error)
            Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> ClearFolderTextBlock.Foreground <- new SolidColorBrush(Colors.Red)
                                                                                         ClearFolderTextBlock.Text <- "Папка " + resultFolder + " не существует"
                                                                                         ClearFolderProgressBar.Visibility <- Visibility.Collapsed
                                                                                         FatalError <- true)) |> ignore
        
    with
        | :? System.Exception as e -> if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error)
                                      Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> ClearFolderTextBlock.Text <- e.Message
                                                                                                                   ClearFolderTextBlock.Foreground <- new SolidColorBrush(Colors.Red)
                                                                                                                   ClearFolderProgressBar.Visibility <- Visibility.Collapsed
                                                                                                                   FatalError <- true)) |> ignore

    true

//Randomize result dictionary
let RandomizeDictionary (filesList : Dictionary<string, string>) =
    let result = new Dictionary<string, string>()
    let random = new Random()

    let mutable continueLooping = true;
    while filesList.Count > 0 && continueLooping do
        let index = random.Next(0, filesList.Count)
        let currentKeyValuePair = filesList.Take(index).LastOrDefault()

        if String.IsNullOrEmpty(currentKeyValuePair.Key) = false && String.IsNullOrEmpty(currentKeyValuePair.Value) = false then
            result.Add(currentKeyValuePair.Key, currentKeyValuePair.Value)
            filesList.Remove(currentKeyValuePair.Key) |> ignore

        if filesList.Count = 1 then
            let lastKeyValuePair = filesList.LastOrDefault()
            result.Add(lastKeyValuePair.Key, lastKeyValuePair.Value)
            continueLooping <- false

    result

//Copy files from dictionary
let CopyFilesAndClearTags (filesList : Dictionary<string, string>) (cleadId3Tags : bool) =
    let minimumPercentage = 76
    let maximumPercentage = 100

    if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(minimumPercentage, 100)
    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> CopyFilesAndClearTagsTextBlock.Visibility <- Visibility.Visible
                                                                                 CopyFilesAndClearTagsProgressBar.Visibility <- Visibility.Visible
                                                                                 CopyFilesAndClearTagsProgressBar.IsIndeterminate <- false
                                                                                 CopyFilesAndClearTagsProgressBar.Minimum <- 0.0
                                                                                 CopyFilesAndClearTagsProgressBar.Maximum <- Convert.ToDouble(filesList.Count))) |> ignore

    let startedTime = DateTime.Now

    for currentFile in filesList do
        let position = filesList.Keys.ToList().IndexOf(currentFile.Key)
        let mutable extraTime = ""

        if position > 5 then
            let alreadyExceeded = DateTime.Now - startedTime
            let filePerMinute = (double)position / (double)alreadyExceeded.TotalMinutes
            let extraFiles = filesList.Count - position
            let extraTimeInMinutes = (int)((double)extraFiles / filePerMinute)

            if extraTimeInMinutes >= 1 then
                let mutable ending = ""

                let mutable extraTimeInMinutesImproved = extraTimeInMinutes
                while extraTimeInMinutesImproved > 100 do
                    extraTimeInMinutesImproved <- extraTimeInMinutesImproved - 100

                if (extraTimeInMinutesImproved = 0) || (extraTimeInMinutesImproved >= 5 && extraTimeInMinutesImproved <= 20) then
                    ending <- ""
                elif extraTimeInMinutesImproved.ToString().LastOrDefault() = '1' then
                    ending <- "а"
                elif extraTimeInMinutesImproved.ToString().LastOrDefault() = '2' || extraTimeInMinutesImproved.ToString().LastOrDefault() = '3' || extraTimeInMinutesImproved.ToString().LastOrDefault() = '4' then
                    ending <- "ы"
                else
                    ending <- ""

                extraTime <- " (Осталось приблизительно " + extraTimeInMinutes.ToString("F0") + " минут" + ending + ")"
            else
                let extraTimeInSeconds = (int)((double)extraFiles / filePerMinute * 60.0)
                let mutable ending = ""

                if (extraTimeInSeconds = 0) || (extraTimeInSeconds >= 5 && extraTimeInSeconds <= 20) then
                    ending <- ""
                elif extraTimeInSeconds.ToString().LastOrDefault() = '1' then
                    ending <- "а"
                elif extraTimeInSeconds.ToString().LastOrDefault() = '2' || extraTimeInSeconds.ToString().LastOrDefault() = '3' || extraTimeInSeconds.ToString().LastOrDefault() = '4' then
                    ending <- "ы"
                else
                    ending <- ""

                extraTime <- " (Осталось приблизительно " + extraTimeInSeconds.ToString("F0") + " секунд" + ending + ")"

        let extraTimeString = extraTime
        Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> CopyFilesAndClearTagsTextBlock.Text <- "Копирование файлов... (" + position.ToString() + " из " + filesList.Count.ToString() + ")" + extraTimeString
                                                                                     CopyFilesAndClearTagsProgressBar.Value <- Convert.ToDouble(position)))|> ignore

        let percentage = (int)((double)position / (double)filesList.Count * (double)(maximumPercentage - minimumPercentage)) + minimumPercentage
        if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(percentage, 100)

        let sourcePath = currentFile.Key
        let targetPath = currentFile.Value

        let directoryName = Path.GetDirectoryName(targetPath)
        if (Directory.Exists(directoryName) = false) then
            Directory.CreateDirectory(directoryName) |> ignore

        if File.Exists(sourcePath) && Directory.Exists(directoryName) then
            File.Copy(sourcePath, targetPath, true)

            let fileExtension = Path.GetExtension(sourcePath).ToLowerInvariant()
            if cleadId3Tags && File.Exists(targetPath) && fileExtension = ".mp3" then
                let currentFileInfo = new FileInfo(targetPath)
                let fileAttributes = currentFileInfo.Attributes

                if fileAttributes &&& FileAttributes.ReadOnly = FileAttributes.ReadOnly then
                    let newAttributes = fileAttributes - FileAttributes.ReadOnly;
                    currentFileInfo.Attributes <- newAttributes
                
                try
                    let u = new UltraID3()
                    u.Read(targetPath)

                    let id3v1Tags = u.ID3v1Tag
                    if String.IsNullOrEmpty(id3v1Tags.Album) = false then id3v1Tags.Album <- String.Empty
                    if String.IsNullOrEmpty(id3v1Tags.Artist) = false then id3v1Tags.Artist <- String.Empty
                    if String.IsNullOrEmpty(id3v1Tags.Comments) = false then id3v1Tags.Comments <- String.Empty
                    id3v1Tags.Genre <- Convert.ToByte(0);
                    if String.IsNullOrEmpty(id3v1Tags.Title) = false then id3v1Tags.Title <- String.Empty
                    id3v1Tags.SetTrackNum(Convert.ToString(1))
                    id3v1Tags.SetYear((DateTime.Now.Year).ToString())

                    let id3v2Tags = u.ID3v2Tag
                    if id3v2Tags.Frames.Count > 0 then
                        id3v2Tags.Frames.Clear()

                        (*
                        if String.IsNullOrEmpty(id3v2Tags.Album) = false then id3v2Tags.Album <- String.Empty
                        if String.IsNullOrEmpty(id3v2Tags.Artist) = false then id3v2Tags.Artist <- String.Empty
                        if String.IsNullOrEmpty(id3v2Tags.Comments) = false then id3v2Tags.Comments <- String.Empty
                        if String.IsNullOrEmpty(id3v2Tags.Title) = false then id3v2Tags.Title <- String.Empty
                        if id3v2Tags.IsCover = true then id3v2Tags.IsCover <- false
                        if id3v2Tags.IsRemix = true then id3v2Tags.IsRemix <- false
                        if id3v2Tags.TrackNum.HasValue then id3v2Tags.SetTrackNum(Convert.ToString(1))
                        if id3v2Tags.TrackCount.HasValue then id3v2Tags.SetTrackCount(Convert.ToString(1))
                        if id3v2Tags.Year.HasValue then id3v2Tags.SetYear((DateTime.Now.Year).ToString())

                        let enumValues = Enum.GetValues(typedefof<ID3v2FrameTypes>).Cast<ID3v2FrameTypes>().ToList()
                        for currentEnumValue in enumValues do
                            let currentFramesCollection = id3v2Tags.Frames.GetFrames(currentEnumValue)
                            for currentFrame in currentFramesCollection do
                                id3v2Tags.Frames.Remove(currentFrame)
                        *)

                        (*
                        let bandFramesCollection = id3v2Tags.Frames.GetFrames(ID3v2FrameTypes.ID3v22Band)
                        for bandFrame in bandFramesCollection do
                            id3v2Tags.Frames.Remove(bandFrame)

                        let composersFramesCollection = id3v2Tags.Frames.GetFrames(ID3v2FrameTypes.ID3v22Composers)
                        for composersFrame in composersFramesCollection do
                            id3v2Tags.Frames.Remove(composersFrame)

                        let bandFramesCollection = id3v2Tags.Frames.GetFrames(ID3v2FrameTypes.ID3v22Genre)
                        for bandFrame in bandFramesCollection do
                            id3v2Tags.Frames.Remove(bandFrame)
                        *)

                    u.Write()
                with
                    | :? System.Exception as e -> if (e.Message.Contains("AAA") = false) then
                                                      if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Paused)
                                                      Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> ErrorsListView.Visibility <- Visibility.Visible
                                                                                                                                   let errorMessage = sourcePath + ": " + e.Message
                                                                                                                                   Errors.Add errorMessage
                                                                                                                                   ErrorsListView.Foreground <- new SolidColorBrush(Colors.Red)
                                                                                                                                   CopyFilesAndClearTagsTextBlock.Foreground <- new SolidColorBrush(Colors.Red)
                                                                                                                                   CopyFilesAndClearTagsProgressBar.Foreground <- new SolidColorBrush(Colors.Orange))) |> ignore

    if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressValue(maximumPercentage, 100)
    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> CopyFilesAndClearTagsTextBlock.Text <- "Копирование файлов и очистка ID3 тегов завершены."
                                                                                 CopyFilesAndClearTagsTextBlock.Foreground <- new SolidColorBrush(Colors.Green)
                                                                                 CopyFilesAndClearTagsProgressBar.Visibility <- Visibility.Collapsed)) |> ignore

    true

//Main method with all outer cycle
let StartJob (musicFolder : string) (resultFolder : string) (separateToFolders : int) (mask :string) (cleadId3Tags : bool) (fileFormats : string) (clearFolder : bool) =
    let language = CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName

    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> if TaskbarManager.IsPlatformSupported then TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal)
                                                                                 InitialisationTextBlock.Visibility <- Visibility.Visible
                                                                                 InitialisationProgressBar.Visibility <- Visibility.Visible)) |> ignore

    let mutable chars = new List<string>()
    chars.AddRange(["0"; "1"; "2"; "3"; "4"; "5"; "6"; "7"; "8"; "9"])
    chars.AddRange(["Й"; "Ц"; "У"; "К"; "Е"; "Н"; "Г"; "Ш"; "Щ"; "З"; "Х"; "Ъ"; "Ф"; "Ы"; "В"; "А"; "П"; "Р"; "О"; "Л"; "Д"; "Ж"; "Э"; "Я"; "Ч"; "С"; "М"; "И"; "Т"; "Ь"; "Б"; "Ю"; "Ё"])
    chars.AddRange(["ღ";"ჯ";"უ";"ე";"ზ";"ძ";"პ";"ო";"დ";"ჟ";"ჭ";"ჩ";"ა";"ი";"მ";"ლ";"ტ";"ბ";"ც";"წ";"რ";"ს";"შ";"ფ";"თ";"ქ";"ნ";"ვ";"ხ";"ყ";"გ";"ჰ";"კ"])
    chars.AddRange(["A"; "B"; "C"; "D"; "E"; "F"; "G"; "H"; "I"; "J"; "K"; "L"; "M"; "N"; "O"; "P"; "Q"; "R"; "S"; "T"; "W"; "U"; "W"; "X"; "Y"; "Z"])
    chars.AddRange(["Ε"; "Ρ"; "Τ"; "Υ"; "Θ"; "Ι"; "Ο"; "Π"; "Α"; "Σ"; "Δ"; "Φ"; "Γ"; "Η"; "Ξ"; "Κ"; "Λ"; "Ζ"; "Χ"; "Ψ"; "Ω"; "Β"; "Ν"; "Μ"])
    chars.AddRange(["Š";"Đ";"Č";"Ć";"Ћ";"Њ";"Љ";"Ђ";"Џ"])
    chars.AddRange(["Ü";"Õ";"Ö";"Ä"])
    chars.AddRange(["Ů";"§";"Ú"])
    chars.AddRange(["Ü";"Ä";"Ö"])
    chars.AddRange(["Ő";"É";"Á"])
    chars.AddRange(["Å";"Æ";"Ø"])
    chars.AddRange(["Ó";"ù"])
    chars.AddRange(["Ç";"Ë"])
    chars.AddRange(["Ї";"Є"])
    chars.AddRange(["Ư";"Ơ"])
    chars.AddRange(["Ў";"І"])
    chars.AddRange(["Қ"])
    chars.AddRange(["Ñ"])
    chars <- chars.Distinct().ToList()

    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> InitialisationTextBlock.Text <- "Инициализация завершена."
                                                                                 InitialisationTextBlock.Foreground <- new SolidColorBrush(Colors.Green)
                                                                                 InitialisationProgressBar.Visibility <- Visibility.Collapsed)) |> ignore

    if FatalError = false && clearFolder then
        ClearFolder resultFolder |> ignore

    let toRandomAndCopyFiles = new List<string>()
    if FatalError = false then
        toRandomAndCopyFiles.AddRange(GetInterestingFiles musicFolder resultFolder mask fileFormats)

    let mutable randomNamesDictionary = new Dictionary<string, string>()
    if FatalError = false then
        randomNamesDictionary <- GetRandomNamesDictionary toRandomAndCopyFiles resultFolder separateToFolders mask chars

    if FatalError = false then
        randomNamesDictionary <- RandomizeDictionary randomNamesDictionary

    if FatalError = false then
        CopyFilesAndClearTags randomNamesDictionary cleadId3Tags |> ignore

    Window.Dispatcher.Invoke(Threading.DispatcherPriority.Send, Action(fun () -> if FatalError = false && TaskbarManager.IsPlatformSupported then
                                                                                     TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress)
                                                                                 CloseButton.Content <- "Закрыть окно"
                                                                                 CloseButton.DataContext <- 1)) |> ignore

let ShowWindow (musicFolder : string) (resultFolder : string) (separateToFolders : int) (mask :string) (cleadId3Tags : bool) (fileFormats : string) (clearFolder : bool) =
    Window <- InitialiseWindow

    Task.Factory.StartNew(fun () -> StartJob musicFolder resultFolder separateToFolders mask cleadId3Tags fileFormats clearFolder |> ignore) |> ignore

    Window.ShowDialog() |> ignore