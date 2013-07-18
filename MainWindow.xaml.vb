Imports SeasideResearch.LibCurlNet
Imports System.Threading
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports Microsoft.Win32
Imports Newtonsoft.Json

Class MainWindow
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            Me.Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(My.Resources.cloud_icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions)

            If Not CheckNetRcFile() Then
                Me.Close()
                Exit Sub
            End If

            SetInfo()
            SetShellExtension()
            ReadParameter()
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub SetShellExtension()
        Dim runningDir As String = IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
        Dim batFileName As String = "fb-client_setExtension.bat"

        If Registry.ClassesRoot.OpenSubKey("*\shell\Paste to Filebin\command") Is Nothing AndAlso IO.File.Exists(runningDir & "\" & batFileName) Then
            Dim procInfo As New ProcessStartInfo()
            procInfo.UseShellExecute = True
            procInfo.FileName = batFileName
            procInfo.WorkingDirectory = runningDir
            procInfo.Verb = "runas"
            Process.Start(procInfo)
        End If
    End Sub

    Private Sub ReadParameter()
        For i As Integer = 0 To Environment.GetCommandLineArgs.Count - 1
            Log_WriteLine(Environment.GetCommandLineArgs(i))
        Next
        For i As Integer = 1 To Environment.GetCommandLineArgs.Count - 1
            If IO.File.Exists(Environment.GetCommandLineArgs(i)) Then
                SetFileInfo(Environment.GetCommandLineArgs(i))
                tpUploadFile.Focus()
            Else
                Select Case Environment.GetCommandLineArgs(i)
                    Case "-systray"

                End Select
            End If
        Next
    End Sub

    Private Function CheckNetRcFile() As Boolean
        Dim netrcFilePath As String

        Environment.SetEnvironmentVariable("HOME", Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"))

        netrcFilePath = Environment.GetEnvironmentVariable("HOME") & "\_netrc"

        If Not IO.File.Exists(netrcFilePath) Then
            Using sw As New IO.StreamWriter(netrcFilePath)
                sw.WriteLine(String.Format("machine {0} login USERNAME password PASSWORD", New Uri(My.Settings.fb_host).Host))
            End Using

            MessageBox.ShowBox("you have to set username and passsword in _netrc file. " & netrcFilePath)
            Return False
        Else
            Return True
        End If
    End Function

    Private Sub SetInfo()
        Dim runProgramm As New Run(String.Format("Programm: {0}", My.Application.Info.Title) & vbCrLf)
        Dim runVersion As New Run(String.Format("Version: {0}.{1}", My.Application.Info.Version.Major, My.Application.Info.Version.Minor) & vbCrLf)
        Dim runSource As New Run("Source: ")
        Dim runSourceLink As New Run("https://github.com/sebastianrakel/fb-client.net")

        Dim hyperLink As New Hyperlink(runSourceLink)
        hyperLink.NavigateUri = New Uri("https://github.com/sebastianrakel/fb-client.net")

        AddHandler hyperLink.RequestNavigate, AddressOf hyperLink_requested

        With Me.InfoTextBlock.Inlines
            .Clear()
            .Add(runProgramm)
            .Add(runVersion)
            .Add(runSource)
            .Add(hyperLink)
        End With

        Me.Title = String.Format("{0} - {1}.{2}", My.Application.Info.Title, My.Application.Info.Version.Major, My.Application.Info.Version.Minor)
    End Sub

    Private Sub UploadFile(ByVal sourceFilePath As String)
        Dim backWorker As New BackgroundWorker()

        If sourceFilePath Is Nothing OrElse sourceFilePath.Length = 0 Then Exit Sub

        AddHandler backWorker.DoWork, AddressOf backWorker_DoWork
        backWorker.RunWorkerAsync(sourceFilePath)

        Me.clipboardLink.Visibility = Windows.Visibility.Hidden
        Me.btnClipboardCopy.Visibility = Windows.Visibility.Hidden
        Me.uploadProgressBar.Visibility = Windows.Visibility.Visible
        Me.labelUploadProgress.Visibility = Windows.Visibility.Visible
    End Sub

    Public Sub OnDebug(ByVal infoType As CURLINFOTYPE, _
        ByVal msg As String, ByVal extraData As Object)
        'only dump received data
        If (infoType = CURLINFOTYPE.CURLINFO_DATA_IN) Then
            If msg.StartsWith("http") Then SetClipboardTextDispatcher(msg)
            Log_WriteLine(msg)
        End If
    End Sub

    Public Function OnProgress(ByVal extraData As Object, _
        ByVal dlTotal As Double, ByVal dlNow As Double, _
        ByVal ulTotal As Double, ByVal ulNow As Double) As Int32

        SetProgressDispatcher(ulNow, ulTotal)
        Return 0
    End Function

    Private Sub Log_Clear()
        Me.LogTextBox.Text = ""
    End Sub

    Private Sub Log_WriteLine(ByVal input As String)
        Dim dispatcherOp As System.Windows.Threading.DispatcherOperation = Me.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, New Action(Sub() Me.LogTextBox.Text &= input & vbCrLf))
    End Sub

    Private Sub btnUploadText_Click(sender As Object, e As RoutedEventArgs) Handles btnUploadText.Click
        Dim pTMPFileName As String

        Try
            If inputTextBox.Text.Trim.Length = 0 Then
                MsgBox("please, input some text!")
                Exit Sub
            End If

            pTMPFileName = System.IO.Path.GetTempPath & "stdin"

            Using sw As New IO.StreamWriter(pTMPFileName)
                sw.Write(inputTextBox.Text)
            End Using

            Me.UploadFile(pTMPFileName)
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub SetClipboardTextDispatcher(ByVal text As String)
        Me.Dispatcher.BeginInvoke(New Action(Of String)(AddressOf SetClipboardText), text)
    End Sub

    Public Sub SetClipboardText(ByVal text As String)
        Me.uploadProgressBar.Visibility = Windows.Visibility.Hidden
        Me.labelUploadProgress.Visibility = Windows.Visibility.Hidden
        Me.clipboardLink.Visibility = Windows.Visibility.Visible
        Me.btnClipboardCopy.Visibility = Windows.Visibility.Visible

        Me.clipboardLink.Text = text.Trim

        Me.clipboardLink.Focus()
        Me.clipboardLink.SelectAll()
    End Sub

    Private Sub btnClipboardCopy_Click(sender As Object, e As RoutedEventArgs) Handles btnClipboardCopy.Click
        Try
            If Me.clipboardLink.Text.Trim.Length = 0 Then Exit Sub

            Clipboard.SetText(Me.clipboardLink.Text)
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub btnSearchUploadFile_Click(sender As Object, e As RoutedEventArgs) Handles btnSearchUploadFile.Click
        Try
            Dim fileDlg As New Microsoft.Win32.OpenFileDialog

            With fileDlg
                .Multiselect = False

                If .ShowDialog() Then
                    SetFileInfo(.FileName)
                    Me.btnUploadFile.Visibility = Windows.Visibility.Visible
                End If
            End With
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub

    Private _UploadFilePath As String

    Private Sub SetFileInfo(ByVal filePath As String)
        _UploadFilePath = filePath

        Dim fileInfo As New IO.FileInfo(filePath)

        Dim stringbuild As New System.Text.StringBuilder()

        With stringbuild
            .AppendLine("Filename: " & fileInfo.Name)
            .AppendLine("Size (bytes): " & fileInfo.Length)
        End With

        Me.inputDragDrop.Content = stringbuild.ToString

        Me.btnUploadFile.Visibility = Windows.Visibility.Visible
    End Sub

    Private Sub SetProgressDispatcher(ByVal value As Integer, ByVal maxValue As Integer)
        Me.Dispatcher.Invoke(New Action(Of Integer())(AddressOf SetProgress), New Integer() {value, maxValue})
    End Sub

    Private Sub SetProgress(ByVal values As Integer())
        Dim percentValue As Integer

        Me.uploadProgressBar.Maximum = 100

        If values(1) = 0 Then Exit Sub

        percentValue = (values(0) * 100) / values(1)

        Me.uploadProgressBar.Value = percentValue
        Me.labelUploadProgress.Content = values(0) & " / " & values(1)
    End Sub

    Private Sub btnUploadFile_Click(sender As Object, e As RoutedEventArgs) Handles btnUploadFile.Click
        Try
            Me.UploadFile(_UploadFilePath)
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub backWorker_DoWork(sender As Object, e As DoWorkEventArgs)
        Try
            Curl.GlobalInit(CURLinitFlag.CURL_GLOBAL_ALL)

            Dim headerlist As New Slist

            ' <form action="http://mybox/cgi-bin/myscript.cgi
            '  method="post" enctype="multipart/form-data">
            Dim mf As MultiPartForm
            mf = New MultiPartForm()

            mf.AddSection(CURLformoption.CURLFORM_COPYNAME, "file", _
               CURLformoption.CURLFORM_FILE, e.Argument, _
               CURLformoption.CURLFORM_END)

            Dim easy As Easy
            easy = New Easy

            Dim df As Easy.DebugFunction
            df = New Easy.DebugFunction(AddressOf OnDebug)
            easy.SetOpt(CURLoption.CURLOPT_DEBUGFUNCTION, df)
            easy.SetOpt(CURLoption.CURLOPT_VERBOSE, True)

            Dim pf As Easy.ProgressFunction
            pf = New Easy.ProgressFunction(AddressOf OnProgress)
            easy.SetOpt(CURLoption.CURLOPT_PROGRESSFUNCTION, pf)

            easy.SetOpt(CURLoption.CURLOPT_URL, My.Settings.fb_host)
            easy.SetOpt(CURLoption.CURLOPT_HTTPPOST, mf)

            headerlist.Append("Expect:")

            easy.SetOpt(CURLoption.CURLOPT_HTTPHEADER, headerlist)
            easy.SetOpt(CURLoption.CURLOPT_NETRC, CURLnetrcOption.CURL_NETRC_REQUIRED)

            easy.SetOpt(CURLoption.CURLOPT_USERAGENT, "fb-client.net")
            easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, False)
            easy.SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, True)

            easy.Perform()
            easy.Cleanup()
            mf.Free()
            Curl.GlobalCleanup()
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub hyperLink_requested(sender As Object, e As RequestNavigateEventArgs)
        Try
            Process.Start(e.Uri.ToString)
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub inputDragDrop_Drop(sender As Object, e As DragEventArgs) Handles inputDragDrop.Drop
        Dim fileNames As List(Of String)

        Try
            fileNames = CType(e.Data.GetData(DataFormats.FileDrop), String()).ToList

            Select Case fileNames.Count
                Case 1
                    SetFileInfo(fileNames(0))
                Case Is > 1
                    MessageBox.ShowBox("at the moment only one file, sorry")
                Case Else
                    MessageBox.ShowBox("somethin went wrong with this file, sorry")
            End Select
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub LoadUploadHistory()
        Try
            Curl.GlobalInit(CURLinitFlag.CURL_GLOBAL_ALL)

            Dim headerlist As New Slist

            Dim easy As Easy
            easy = New Easy

            Dim wf As Easy.WriteFunction
            wf = New Easy.WriteFunction(AddressOf OnWriteData)

            easy.SetOpt(CURLoption.CURLOPT_URL, My.Settings.fb_host & "/file/upload_history?json")

            headerlist.Append("Expect:")

            easy.SetOpt(CURLoption.CURLOPT_HTTPHEADER, headerlist)
            easy.SetOpt(CURLoption.CURLOPT_NETRC, CURLnetrcOption.CURL_NETRC_REQUIRED)

            easy.SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf)

            easy.SetOpt(CURLoption.CURLOPT_USERAGENT, "fb-client.net")
            easy.SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, False)
            easy.SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, True)

            easy.Perform()
            easy.Cleanup()
            Curl.GlobalCleanup()
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub


    Public Function OnWriteData(ByVal buf() As Byte, _
       ByVal size As Int32, ByVal nmemb As Int32, _
       ByVal extraData As Object) As Int32
        ReadContent(System.Text.Encoding.UTF8.GetString(buf))
        Return size * nmemb
    End Function

    Private Sub ReadContent(ByVal content As String)
        Dim pFileInfos() As pastebin_fileInfo

        pFileInfos = JsonConvert.DeserializeObject(Of pastebin_fileInfo())(content)

        historyListView.Items.Clear()

        For Each fileInfo In pFileInfos
            historyListView.Items.Add(fileInfo)
        Next
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Try
            LoadUploadHistory()
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub tpHistory_Loaded(sender As Object, e As RoutedEventArgs) Handles tpHistory.Loaded
        Try
            LoadUploadHistory()
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub historyLink_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
        Try
            Process.Start(e.Uri.ToString)
        Catch ex As Exception
            MessageBox.ShowBox(ex)
        End Try
    End Sub
End Class
