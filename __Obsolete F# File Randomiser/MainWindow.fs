module MainWindow

open System
open System.IO
open System.Windows
open System.Windows.Controls
open System.Windows.Markup
open System.Windows.Shapes
open System.Windows.Media
open System.Windows.Forms
open System.Windows.Controls
open System.Threading
open System.Linq
open System.Globalization
open System.Diagnostics
open System.Text
open System.Text.RegularExpressions
open System.Collections
open System.Collections.Generic
open System.Collections.Specialized
open System.Collections.ObjectModel

let mutable GoFurther = false
let mutable MusicFolder  = ""
let mutable ResultFolder = ""
let mutable ClearFolder = true
let mutable SeparateToFolders = 100
let mutable Mask = "R[$(Rand)]"
let mutable ClearId3Tags = true
let mutable FileFormats = "mp3, wma"

let mutable OkButtonEnabled1 = true;
let mutable OkButtonEnabled2 = true;
let mutable OkButtonEnabled3 = true;

let InitialiseDefaultFolders =
    let fixedDrives = DriveInfo.GetDrives().Where(fun(value : DriveInfo) -> value.DriveType = DriveType.Fixed)
    MusicFolder <- Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FileRandomiser sources")
    for currentDrive in fixedDrives do
        let suggestedPath = Path.Combine(currentDrive.RootDirectory.FullName, "Music")
        if (Directory.Exists(suggestedPath)) then
            MusicFolder <- suggestedPath

    let removebleDrives = DriveInfo.GetDrives().Where(fun(value : DriveInfo) -> value.DriveType = DriveType.Removable)
    if removebleDrives.Count() = 0 then
        ResultFolder <- Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FileRandomiser results")
    elif removebleDrives.Count() = 1 then
        let drive = removebleDrives.FirstOrDefault()
        ResultFolder <- drive.RootDirectory.FullName

    if (Directory.Exists(MusicFolder) = false) then
        try
            Directory.CreateDirectory(MusicFolder) |> ignore
        with
            | :? System.Exception as e -> MusicFolder <- Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FileRandomiser sources")
    
    if (Directory.Exists(ResultFolder) = false) then
        try
           Directory.CreateDirectory(ResultFolder) |> ignore
        with
            | :? System.Exception as e -> ResultFolder <- Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "FileRandomiser results")

    true

let InitialiseWindow =
    InitialiseDefaultFolders |> ignore

    //Main Window
    let window = new Window(Topmost=true)
    window.WindowStartupLocation <- WindowStartupLocation.CenterScreen
    window.Title <- "File Randomiser v1.15 - Конфигурация"
    window.Width <- 640.0
    window.MinWidth <- 640.0
    window.MaxWidth <- 640.0
    window.Height <- 393.0
    window.MinHeight <- 393.0
    window.MaxHeight <- 393.0

    //Main Buttons
    let resultButton1 = new Button()
    resultButton1.Content <- "Just do it!"
    resultButton1.Margin <- new Thickness(5.0, 5.0, 0.0, 5.0)
    resultButton1.Width <- 305.0
    resultButton1.Height <- 40.0
    resultButton1.FontSize <- 25.0
    resultButton1.Click.Add(fun _ -> GoFurther <- true
                                     window.Close() |> ignore)

    let resultButton2 = new Button()
    resultButton2.Content <- "Выход"
    resultButton2.Margin <- new Thickness(5.0, 5.0, 5.0, 5.0)
    resultButton2.Width <- 305.0
    resultButton2.Height <- 40.0
    resultButton2.FontSize <- 14.0
    resultButton2.Click.Add(fun _ -> GoFurther <- false
                                     window.Close() |> ignore)

    let resultStackPanel = new StackPanel()
    resultStackPanel.Orientation <- Orientation.Horizontal
    resultStackPanel.Children.Add(resultButton1) |> ignore
    resultStackPanel.Children.Add(resultButton2) |> ignore

    //MusicFolder Edit Field
    let tbk1 = new TextBlock()
    tbk1.Text <- "Исходная папка с музыкой:"
    tbk1.FontSize <- 14.0
    tbk1.Margin <- new Thickness(6.0, 5.0, 5.0, 0.0)

    let tbx1 = new TextBox()
    tbx1.Text <- MusicFolder
    tbx1.FontSize <- 16.0
    tbx1.Margin <- new Thickness(5.0, 5.0, 5.0, 5.0)
    tbx1.Width <- 580.0
    tbx1.TextChanged.Add(fun _ -> if Directory.Exists(tbx1.Text) then
                                      tbx1.Foreground <- new SolidColorBrush(Colors.Black)
                                      OkButtonEnabled1 <- true
                                  else
                                      tbx1.Foreground <- new SolidColorBrush(Colors.Orange)
                                      OkButtonEnabled1 <- false
                                  
                                  resultButton1.IsEnabled <- OkButtonEnabled1 && OkButtonEnabled2 && OkButtonEnabled3)

    let btn1 = new Button()
    btn1.Content <- "..."
    btn1.Margin <- new Thickness(0.0, 5.0, 5.0, 5.0)
    btn1.Width <- 30.0
    let btn1Dialog = new FolderBrowserDialog();
    btn1Dialog.Description <- "Выбор исходной папки, из которой будет производиться копирование"
    btn1Dialog.SelectedPath <- MusicFolder;
    btn1.Click.Add(fun _ -> btn1Dialog.ShowDialog() |> ignore
                            MusicFolder <- btn1Dialog.SelectedPath
                            tbx1.Clear()
                            tbx1.AppendText(btn1Dialog.SelectedPath))

    let spf1 = new StackPanel()
    spf1.Orientation <- Orientation.Horizontal
    spf1.Children.Add(tbx1) |> ignore
    spf1.Children.Add(btn1) |> ignore

    //ResultFolder Edit Field
    let tbk2 = new TextBlock()
    tbk2.Text <- "Выходная папка:"
    tbk2.FontSize <- 14.0
    tbk2.Margin <- new Thickness(6.0, 5.0, 5.0, 0.0)

    let tbx2 = new TextBox()
    tbx2.Text <- ResultFolder
    tbx2.FontSize <- 16.0
    tbx2.Margin <- new Thickness(5.0, 5.0, 5.0, 5.0)
    tbx2.Width <- 580.0
    tbx2.TextChanged.Add(fun _ -> if Directory.Exists(tbx2.Text) then
                                      tbx2.Foreground <- new SolidColorBrush(Colors.Black)
                                      OkButtonEnabled2 <- true
                                  else
                                      tbx2.Foreground <- new SolidColorBrush(Colors.Orange)
                                      OkButtonEnabled2 <- false
                                      
                                  resultButton1.IsEnabled <- OkButtonEnabled1 && OkButtonEnabled2 && OkButtonEnabled3)

    let btn2 = new Button()
    btn2.Content <- "..."
    btn2.Margin <- new Thickness(0.0, 5.0, 5.0, 5.0)
    btn2.Width <- 30.0
    let btn2Dialog = new FolderBrowserDialog();
    btn2Dialog.Description <- "Выбор целевой папки, в которую будет производиться копирование"
    btn2Dialog.SelectedPath <- ResultFolder;
    btn2.Click.Add(fun _ -> btn2Dialog.ShowDialog() |> ignore
                            ResultFolder <- btn2Dialog.SelectedPath
                            tbx2.Clear()
                            tbx2.AppendText(btn2Dialog.SelectedPath))

    let spf2 = new StackPanel()
    spf2.Orientation <- Orientation.Horizontal
    spf2.Children.Add(tbx2) |> ignore
    spf2.Children.Add(btn2) |> ignore

    //Clear folder before start
    let clearFolderCheckBox1 = new CheckBox()
    clearFolderCheckBox1.Content <- "Очистить папку перед копированием файлов"
    clearFolderCheckBox1.FontSize <- 14.0
    clearFolderCheckBox1.Margin <- new Thickness(5.0, 5.0, 5.0, 5.0)
    clearFolderCheckBox1.IsChecked <- new Nullable<bool>(true)
    clearFolderCheckBox1.Click.Add(fun _ -> ClearFolder <- Convert.ToBoolean(clearFolderCheckBox1.IsChecked))

    //Checking to separate files by folders
    let checking1EditBox1 = new TextBox()
    checking1EditBox1.Text <- "100"
    checking1EditBox1.Width <- 70.0
    checking1EditBox1.FontSize <- 14.0
    checking1EditBox1.Margin <- new Thickness(5.0, 3.0, 0.0, 5.0)
    checking1EditBox1.TextChanged.Add(fun _ -> let mutable x = 100
                                               let succeededParse = Int32.TryParse(checking1EditBox1.Text, ref x)
                                               let succeededValue = if succeededParse then Convert.ToInt32(checking1EditBox1.Text) > 0 else false
                                               let succeeded = succeededParse && succeededValue
                                               if succeeded then
                                                   SeparateToFolders <- Convert.ToInt32(checking1EditBox1.Text)
                                               else
                                                   checking1EditBox1.Text <- Convert.ToString(SeparateToFolders))

    let checkBox1 = new CheckBox()
    checkBox1.Content <- "Разбить на подпапки по"
    checkBox1.FontSize <- 14.0
    checkBox1.Margin <- new Thickness(5.0, 5.0, 0.0, 5.0)
    checkBox1.IsChecked <- new Nullable<bool>(true)
    checkBox1.Click.Add(fun _ -> if Convert.ToBoolean(checkBox1.IsChecked) then
                                     SeparateToFolders <- Convert.ToInt32(checking1EditBox1.Text)
                                     checking1EditBox1.IsEnabled <- true
                                 else
                                     SeparateToFolders <- Int32.MaxValue
                                     checking1EditBox1.IsEnabled <- false)

    let checkingEndingTextBlock1 = new TextBlock()
    checkingEndingTextBlock1.Text <- "файлов"
    checkingEndingTextBlock1.FontSize <- 14.0
    checkingEndingTextBlock1.Margin <- new Thickness(5.0, 5.0, 5.0, 5.0)

    let spf3 = new StackPanel()
    spf3.Orientation <- Orientation.Horizontal
    spf3.Children.Add(checkBox1) |> ignore
    spf3.Children.Add(checking1EditBox1) |> ignore
    spf3.Children.Add(checkingEndingTextBlock1) |> ignore

    //Mask of randomised names
    let maskEndingTextBlock1 = new TextBlock()
    maskEndingTextBlock1.Text <- "Маска имен файлов:"
    maskEndingTextBlock1.FontSize <- 14.0
    maskEndingTextBlock1.Margin <- new Thickness(5.0, 5.0, 5.0, 5.0)
    
    let maskEditBox1 = new TextBox()
    maskEditBox1.Text <- "R[$(Rand)]"
    maskEditBox1.Width <- 473.0
    maskEditBox1.FontSize <- 14.0
    maskEditBox1.Margin <- new Thickness(5.0, 3.0, 0.0, 5.0)
    maskEditBox1.TextChanged.Add(fun _ -> if maskEditBox1.Text.Contains("$(Rand)") then
                                              Mask <- maskEditBox1.Text
                                              maskEditBox1.Foreground <- new SolidColorBrush(Colors.Black)
                                              OkButtonEnabled3 <- true
                                          else
                                              resultButton1.IsEnabled <- false
                                              maskEditBox1.Foreground <- new SolidColorBrush(Colors.Red)
                                              OkButtonEnabled3 <- false
                                              
                                          resultButton1.IsEnabled <- OkButtonEnabled1 && OkButtonEnabled2 && OkButtonEnabled3)

    let spf4 = new StackPanel()
    spf4.Orientation <- Orientation.Horizontal
    spf4.Children.Add(maskEndingTextBlock1) |> ignore
    spf4.Children.Add(maskEditBox1) |> ignore

    //Clear ID3 tags
    let clearTagsCheckBox1 = new CheckBox()
    clearTagsCheckBox1.Content <- "Чистить ID3 теги для MP3 файлов"
    clearTagsCheckBox1.FontSize <- 14.0
    clearTagsCheckBox1.Margin <- new Thickness(5.0, 5.0, 5.0, 5.0)
    clearTagsCheckBox1.IsChecked <- new Nullable<bool>(true)
    clearTagsCheckBox1.Click.Add(fun _ -> ClearId3Tags <- Convert.ToBoolean(clearTagsCheckBox1.IsChecked))

    //File Formats
    let fileFormatsLabel = new TextBlock()
    fileFormatsLabel.Text <- "Форматы копируемых файлов:"
    fileFormatsLabel.FontSize <- 14.0
    fileFormatsLabel.Margin <- new Thickness(6.0, 5.0, 5.0, 0.0)

    let fileFormatsTextBox = new TextBox()
    fileFormatsTextBox.Text <- FileFormats
    fileFormatsTextBox.FontSize <- 16.0
    fileFormatsTextBox.Margin <- new Thickness(5.0, 5.0, 5.0, 5.0)
    fileFormatsTextBox.TextChanged.Add(fun _ -> FileFormats <- fileFormatsTextBox.Text)

    let fileFormatsStackPanel = new StackPanel()
    fileFormatsStackPanel.Orientation <- Orientation.Vertical
    fileFormatsStackPanel.Children.Add(fileFormatsLabel) |> ignore
    fileFormatsStackPanel.Children.Add(fileFormatsTextBox) |> ignore

    //Main Window
    let mainPanel = new StackPanel()
    mainPanel.Orientation <- Orientation.Vertical
    mainPanel.Children.Add(tbk1) |> ignore
    mainPanel.Children.Add(spf1) |> ignore
    mainPanel.Children.Add(tbk2) |> ignore
    mainPanel.Children.Add(spf2) |> ignore
    mainPanel.Children.Add(clearFolderCheckBox1) |> ignore
    mainPanel.Children.Add(spf3) |> ignore
    mainPanel.Children.Add(spf4) |> ignore
    mainPanel.Children.Add(clearTagsCheckBox1) |> ignore
    mainPanel.Children.Add(fileFormatsStackPanel) |> ignore
    mainPanel.Children.Add(resultStackPanel) |> ignore
    window.Content <- mainPanel

    window