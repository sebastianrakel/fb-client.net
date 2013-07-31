Imports SeasideResearch.LibCurlNet


Public Module FileBin
    Private Function GetUseragentIdentifier() As String
        Return "fb-client.net"
    End Function

    Public Sub UploadFile(ByVal fileNameFullPath As String, ByVal debugDelegate As Easy.DebugFunction, ByVal progressDelegate As Easy.ProgressFunction)
        'must happend first
        Curl.GlobalInit(CURLinitFlag.CURL_GLOBAL_ALL)

        Dim headerlist As New Slist
        Dim easy As New Easy
        Dim mf As New MultiPartForm

        mf.AddSection(CURLformoption.CURLFORM_COPYNAME, "file", _
           CURLformoption.CURLFORM_FILE, fileNameFullPath, _
           CURLformoption.CURLFORM_END)

        With easy
            If debugDelegate IsNot Nothing Then
                .SetOpt(CURLoption.CURLOPT_DEBUGFUNCTION, debugDelegate)
                .SetOpt(CURLoption.CURLOPT_VERBOSE, True)
            End If

            If progressDelegate IsNot Nothing Then
                .SetOpt(CURLoption.CURLOPT_PROGRESSFUNCTION, progressDelegate)
            End If

            .SetOpt(CURLoption.CURLOPT_URL, My.Settings.fb_host)
            .SetOpt(CURLoption.CURLOPT_HTTPPOST, mf)

            headerlist.Append("Expect:")

            .SetOpt(CURLoption.CURLOPT_HTTPHEADER, headerlist)
            .SetOpt(CURLoption.CURLOPT_NETRC, CURLnetrcOption.CURL_NETRC_REQUIRED)

            .SetOpt(CURLoption.CURLOPT_USERAGENT, GetUseragentIdentifier)
            .SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, False)
            .SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, True)

            .Perform()
            .Cleanup()
        End With

        mf.Free()
        Curl.GlobalCleanup()
    End Sub

    Public Function GetUploadHistory() As List(Of pastebin_fileInfo)
        'must happend first
        Curl.GlobalInit(CURLinitFlag.CURL_GLOBAL_ALL)

        _CompleteHistoryJSONOutput = ""

        Dim easy As New Easy
        Dim headerlist As New Slist
        Dim wf As Easy.WriteFunction

        wf = New Easy.WriteFunction(AddressOf OnWriteData)

        With easy
            .SetOpt(CURLoption.CURLOPT_URL, My.Settings.fb_host & "/file/upload_history?json")

            headerlist.Append("Expect:")

            .SetOpt(CURLoption.CURLOPT_HTTPHEADER, headerlist)
            .SetOpt(CURLoption.CURLOPT_NETRC, CURLnetrcOption.CURL_NETRC_REQUIRED)

            .SetOpt(CURLoption.CURLOPT_BUFFERSIZE, 8 * 8192)
            .SetOpt(CURLoption.CURLOPT_WRITEFUNCTION, wf)

            .SetOpt(CURLoption.CURLOPT_USERAGENT, GetUseragentIdentifier)
            .SetOpt(CURLoption.CURLOPT_SSL_VERIFYPEER, False)
            .SetOpt(CURLoption.CURLOPT_FOLLOWLOCATION, True)

            .Perform()
            .Cleanup()
        End With

        Curl.GlobalCleanup()


        If _CompleteHistoryJSONOutput.Length > 0 Then
            Return Jayrock.Json.Conversion.JsonConvert.Import(Of pastebin_fileInfo())(_CompleteHistoryJSONOutput).ToList()
        Else
            Return Nothing
        End If
    End Function

    Dim _CompleteHistoryJSONOutput As String

    Public Function OnWriteData(ByVal buf() As Byte, _
                                  ByVal size As Int32, ByVal nmemb As Int32, _
                                  ByVal extraData As Object) As Int32


        _CompleteHistoryJSONOutput &= System.Text.Encoding.UTF8.GetString(buf)

        Return size * nmemb
    End Function
End Module

Public Class pastebin_fileInfo
    Private _ID As String
    Public Property ID As String
        Get
            Return _ID
        End Get
        Set(value As String)
            _ID = value
        End Set
    End Property

    Private _Filename As String
    Public Property Filename As String
        Get
            Return _Filename
        End Get
        Set(value As String)
            _Filename = value
        End Set
    End Property

    Private _MIMEType As String
    Public Property MIMEType As String
        Get
            Return _MIMEType
        End Get
        Set(value As String)
            _MIMEType = value
        End Set
    End Property

    Private _Filesize As String
    Public Property Filesize As String
        Get
            Return _Filesize
        End Get
        Set(value As String)
            _Filesize = value
        End Set
    End Property

    Private _Hash As String
    Public Property Hash As String
        Get
            Return _Hash
        End Get
        Set(value As String)
            _Hash = value
        End Set
    End Property

    Private _date As String
    Public Property [Date] As String
        Get
            Return GetDateFromUnixTimestamp(_date)
        End Get
        Set(value As String)
            _date = value
        End Set
    End Property

    Public ReadOnly Property Link As String
        Get
            Return My.Settings.fb_host & "/" & Me.ID
        End Get
    End Property

    Public Sub New()

    End Sub
End Class

