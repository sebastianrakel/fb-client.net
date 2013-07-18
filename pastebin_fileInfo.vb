Public Class pastebin_fileInfo
    Private _ID As String
    Public ReadOnly Property ID As String
        Get
            Return _ID
        End Get
    End Property

    Private _Filename As String
    Public ReadOnly Property Filename As String
        Get
            Return _Filename
        End Get
    End Property

    Private _MIMEType As String
    Public ReadOnly Property MIMEType As String
        Get
            Return _MIMEType
        End Get
    End Property

    Private _Filesize As String
    Public ReadOnly Property Filesize As String
        Get
            Return _Filesize
        End Get
    End Property

    Private _Hash As String
    Public ReadOnly Property Hash As String
        Get
            Return _Hash
        End Get
    End Property

    Private _Filedate As String
    Public ReadOnly Property Filedate As String
        Get
            Return _Filedate
        End Get
    End Property

    Public Sub New(ByVal tableString As String)
        ReadTableString(tableString)
    End Sub

    Private Sub ReadTableString(ByVal tableString As String)
        Dim fields As List(Of String)

        fields = tableString.Split("|"c).ToList

        'split fields: ID     | Filename                             | Mimetype           | Date       | Hash                             | Size     

        _ID = fields(0).Trim
        _Filename = fields(1).Trim
        _MIMEType = fields(2).Trim
        _Filedate = fields(3).Trim
        _Hash = fields(4).Trim
        _Filesize = fields(5).Trim
    End Sub
End Class

