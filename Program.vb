Imports System.Windows.Forms
Imports SeasideResearch.LibCurlNet
Imports System.Threading

Module Program
    Private WithEvents _notifyIcon As New NotifyIcon
    Private WithEvents _mainWindow As MainWindow

    Private WithEvents _App As Application

    Private _SystrayMode As Boolean

    Public Sub Main(ByVal args As String())
        Dim fileList As List(Of String)

        Try
            _notifyIcon.Icon = My.Resources.cloud_icon
            _notifyIcon.Visible = True

            SetShellExtension()

            BuildUpContext()

            _App = New Application
            _App.ShutdownMode = ShutdownMode.OnExplicitShutdown
            If Not CheckNetRcFile() Then
                _App.Shutdown()
                Exit Sub
            Else
                fileList = ReadParameter()

                If Not _SystrayMode Then
                    ShowFBWindow()
                    If fileList IsNot Nothing AndAlso fileList.Count > 0 Then _mainWindow.SetFileInfo(fileList(0))
                Else
                    If fileList IsNot Nothing AndAlso fileList.Count > 0 Then UploadFile(fileList(0))
                End If

                _App.Run()
            End If
        Catch ex As Exception
            messageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub BuildUpContext()
        Dim pContextMenu As New ContextMenuStrip, _
            pTMPContextMenuItem As ToolStripItem

        pTMPContextMenuItem = pContextMenu.Items.Add("Show")
        AddHandler pTMPContextMenuItem.Click, AddressOf ShowFBWindow_Click

        pTMPContextMenuItem = pContextMenu.Items.Add("-")

        pTMPContextMenuItem = pContextMenu.Items.Add("Take Screenshot")
        AddHandler pTMPContextMenuItem.Click, AddressOf TakeScreenshot_Click

        pTMPContextMenuItem = pContextMenu.Items.Add("-")

        pTMPContextMenuItem = pContextMenu.Items.Add("Quit")
        AddHandler pTMPContextMenuItem.Click, AddressOf CloseApplication_Click

        _notifyIcon.ContextMenuStrip = pContextMenu
    End Sub

    Private Function ReadParameter() As List(Of String)
        Dim retList As New List(Of String)

        For i As Integer = 1 To Environment.GetCommandLineArgs.Count - 1
            If i > 0 Then
                If IO.File.Exists(Environment.GetCommandLineArgs(i)) Then
                    retList.Add(Environment.GetCommandLineArgs(i))
                Else
                    Select Case Environment.GetCommandLineArgs(i)
                        Case "-systray"
                            _SystrayMode = True
                    End Select
                End If
            End If
        Next

        Return retList
    End Function

    Public Sub ShowFBWindow()
        If _mainWindow Is Nothing OrElse Not _mainWindow.IsLoaded Then _mainWindow = New MainWindow

        _mainWindow.Show()
    End Sub

    Private Sub _notifyIcon_DoubleClick(sender As Object, e As EventArgs) Handles _notifyIcon.DoubleClick
        Try
            ShowFBWindow()
        Catch ex As Exception
            messageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub TakeScreenshot_Click(sender As Object, e As EventArgs)
        Dim pTMPScreenshotImage As System.Drawing.Image, _
            pTMPPath As String

        Try
            pTMPScreenshotImage = ScreenshotSnipping.Snip()

            If pTMPScreenshotImage IsNot Nothing Then
                pTMPPath = System.IO.Path.GetTempPath & "screenshot.png"

                pTMPScreenshotImage.Save(pTMPPath)

                UploadFile(pTMPPath)

                'If IO.File.Exists(pTMPPath) Then IO.File.Delete(pTMPPath)
            End If
        Catch ex As Exception
            messageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub CloseApplication_Click(sender As Object, e As EventArgs)
        Try
            _notifyIcon.Visible = False
            _App.Shutdown()
        Catch ex As Exception
            messageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub ShowFBWindow_Click(sender As Object, e As EventArgs)
        Try
            ShowFBWindow()
        Catch ex As Exception
            messageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub UploadFile(ByVal fullFilePath As String)
        Dim pUploadThread As New Thread(AddressOf UploadThread)
        pUploadThread.SetApartmentState(ApartmentState.STA)
        pUploadThread.Start(fullFilePath)

    End Sub

    Private Sub UploadThread(ByVal fullFilePath As String)
        _notifyIcon.ShowBalloonTip(3000, "start upload file", "upload of file started", ToolTipIcon.Info)

        FileBin.UploadFile(fullFilePath, New Easy.DebugFunction(AddressOf OnDebug), Nothing)
    End Sub

    Private Sub OnDebug(ByVal infoType As CURLINFOTYPE, ByVal msg As String, ByVal extraData As Object)
        'only dump received data
        If (infoType = CURLINFOTYPE.CURLINFO_DATA_IN) Then
            If msg.StartsWith("http") Then
                Clipboard.SetText(msg)
                _notifyIcon.ShowBalloonTip(3000, "Upload finished", msg & vbCrLf & "URL copied to clipboard", ToolTipIcon.Info)
            End If
        End If
    End Sub

    Private Sub _mainWindow_Closing(sender As Object, e As System.ComponentModel.CancelEventArgs) Handles _mainWindow.Closing
        Try
            If My.Settings.showSystrayInfo Then
                If Not messageBox.ShowBox("fb-client.NET keeps open in systray. Show this message again?", MessageBoxButton.YesNo) Then
                    My.Settings.showSystrayInfo = False
                    My.Settings.Save()
                End If
            End If
        Catch ex As Exception
            messageBox.ShowBox(ex)
        End Try
    End Sub
End Module
