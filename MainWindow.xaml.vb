Imports SeasideResearch.LibCurlNet
Imports System.Threading
Imports System.ComponentModel
Imports System.Collections.ObjectModel
Imports Microsoft.Win32

Class MainWindow
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        Try
            Me.Icon = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(My.Resources.cloud_icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions)

            SetInfo()
        Catch ex As Exception
            messagebox.ShowBox(ex)
        End Try
    End Sub

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
        Dim pUploadThread As New Thread(AddressOf UploadThread)

        If sourceFilePath Is Nothing OrElse sourceFilePath.Length = 0 Then Exit Sub

        pUploadThread.SetApartmentState(ApartmentState.STA)
        pUploadThread.Start(sourceFilePath)


        'Dim backWorker As New BackgroundWorker()

        'If sourceFilePath Is Nothing OrElse sourceFilePath.Length = 0 Then Exit Sub

        'AddHandler backWorker.DoWork, AddressOf backWorker_DoWork
        'backWorker.RunWorkerAsync(sourceFilePath)

        Me.clipboardLink.Visibility = Windows.Visibility.Hidden
        Me.btnClipboardCopy.Visibility = Windows.Visibility.Hidden
        Me.uploadProgressBar.Visibility = Windows.Visibility.Visible
        Me.labelUploadProgress.Visibility = Windows.Visibility.Visible
    End Sub

    Private Sub OnDebug(ByVal infoType As CURLINFOTYPE, ByVal msg As String, ByVal extraData As Object)
        'only dump received data
        If (infoType = CURLINFOTYPE.CURLINFO_DATA_IN) Then
            If msg.StartsWith("http") Then SetClipboardTextDispatcher(msg)
            Log_WriteLine(msg)
        End If
    End Sub

    Private Function OnProgress(ByVal extraData As Object, _
                                ByVal dlTotal As Double, ByVal dlNow As Double, _
                                ByVal ulTotal As Double, ByVal ulNow As Double) As Int32

        SetProgressDispatcher(ulNow, ulTotal)
        Return 0
    End Function

    Private Sub Log_Clear()
        Me.LogTextBox.Text = ""
    End Sub

    Public Sub Log_WriteLine(ByVal input As String)
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
            messagebox.ShowBox(ex)
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
            messagebox.ShowBox(ex)
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
            messagebox.ShowBox(ex)
        End Try
    End Sub

    Private _UploadFilePath As String

    Public Sub SetFileInfo(ByVal filePath As String)
        _UploadFilePath = filePath

        Dim fileInfo As New IO.FileInfo(filePath)

        Dim stringbuild As New System.Text.StringBuilder()

        With stringbuild
            .AppendLine("Filename: " & fileInfo.Name)
            .AppendLine("Size (bytes): " & fileInfo.Length)
        End With

        Me.inputDragDrop.Content = stringbuild.ToString

        Me.btnUploadFile.Visibility = Windows.Visibility.Visible
        tpUploadFile.Focus()
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
            messagebox.ShowBox(ex)
        End Try
    End Sub

    Private Sub UploadThread(ByVal uploadFileFullPath As String)
        Try
            FileBin.UploadFile(uploadFileFullPath, New Easy.DebugFunction(AddressOf OnDebug), New Easy.ProgressFunction(AddressOf OnProgress))
        Catch ex As Exception
            messagebox.ShowBox(ex)
        End Try
    End Sub

    Private Sub hyperLink_requested(sender As Object, e As RequestNavigateEventArgs)
        Try
            Process.Start(e.Uri.ToString)
        Catch ex As Exception
            messageBox.ShowBox(ex)
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
                    messagebox.ShowBox("at the moment only one file, sorry")
                Case Else
                    messagebox.ShowBox("somethin went wrong with this file, sorry")
            End Select
        Catch ex As Exception
            messagebox.ShowBox(ex)
        End Try
    End Sub
    Private Sub LoadUploadHistory()
        historyListView.Items.Clear()

        For Each fileInfo In GetUploadHistory()
            historyListView.Items.Add(fileInfo)
        Next
    End Sub

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        Try
            LoadUploadHistory()
        Catch ex As Exception
            messagebox.ShowBox(ex)
        End Try
    End Sub

    Private Sub historyLink_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
        Try
            Process.Start(e.Uri.ToString)
        Catch ex As Exception
            messagebox.ShowBox(ex)
        End Try
    End Sub
End Class
