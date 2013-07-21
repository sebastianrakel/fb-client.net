Imports Microsoft.Win32

Module GlobalFunctions
    Public Function GetDateFromUnixTimestamp(ByVal unixTimestamp As String) As Date
        Dim dtDateTime As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
        dtDateTime = dtDateTime.AddSeconds(unixTimestamp).ToLocalTime()
        Return dtDateTime
    End Function

    Public Sub SetShellExtension()
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

    Public Function CheckNetRcFile() As Boolean
        Dim netrcFilePath As String

        Environment.SetEnvironmentVariable("HOME", Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%"))

        netrcFilePath = Environment.GetEnvironmentVariable("HOME") & "\_netrc"

        If Not IO.File.Exists(netrcFilePath) Then
            Using sw As New IO.StreamWriter(netrcFilePath)
                sw.WriteLine(String.Format("machine {0} login USERNAME password PASSWORD", New Uri(My.Settings.fb_host).Host))
            End Using

            messageBox.ShowBox("you have to set username and passsword in _netrc file. " & netrcFilePath)
            Return False
        Else
            Return True
        End If
    End Function

End Module

