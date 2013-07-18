Public Class MessageBox
    Public Shared Sub ShowBox(ByVal text As String)
        Dim msgBox As New MessageBox

        msgBox.SetText(text)
        msgBox.ShowDialog()
    End Sub

    Public Shared Sub ShowBox(ByVal ex As Exception)
        ShowBox(ex.Message)
    End Sub

    Public Sub SetText(ByVal text As String)
        MessageTextBox.Text = text
    End Sub

    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click
        Try
            Me.Close()
        Catch ex As Exception
            ShowBox(ex)
        End Try
    End Sub
End Class
