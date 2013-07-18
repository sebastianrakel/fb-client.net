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

    Private Sub ReadTableString(ByVal tableString As String)
        Dim fields As List(Of String)

        fields = tableString.Split("|"c).ToList

        'split fields: ID     | Filename                             | Mimetype           | Date       | Hash                             | Size     

        _ID = fields(0).Trim
        _Filename = fields(1).Trim
        _MIMEType = fields(2).Trim
        _date = fields(3).Trim
        _Hash = fields(4).Trim
        _Filesize = fields(5).Trim
    End Sub
End Class

