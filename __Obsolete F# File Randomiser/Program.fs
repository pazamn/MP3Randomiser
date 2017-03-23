module Program

open System
open System.IO
open System.Windows
open System.Windows.Forms
open System.Windows.Controls
open System.Windows.Markup
open System.Windows.Shapes
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

open Microsoft.Win32
open Microsoft.WindowsAPICodePack
open Microsoft.WindowsAPICodePack.Shell
open Microsoft.WindowsAPICodePack.Taskbar
open Microsoft.WindowsAPICodePack.Dialogs

[<EntryPoint>]
[<STAThread>]
let main (args: string []) =
    let language = CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName

    let mainWindow = MainWindow.InitialiseWindow
    mainWindow.ShowDialog() |> ignore

    if MainWindow.GoFurther then
        Logic.ShowWindow MainWindow.MusicFolder MainWindow.ResultFolder MainWindow.SeparateToFolders MainWindow.Mask MainWindow.ClearId3Tags MainWindow.FileFormats MainWindow.ClearFolder |> ignore

    1