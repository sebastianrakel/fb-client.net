Module GlobalFunctions
    Public Function GetDateFromUnixTimestamp(ByVal unixTimestamp As String) As Date
        Dim dtDateTime As DateTime = New DateTime(1970, 1, 1, 0, 0, 0, 0)
        dtDateTime = dtDateTime.AddSeconds(unixTimestamp).ToLocalTime()
        Return dtDateTime
    End Function
End Module
