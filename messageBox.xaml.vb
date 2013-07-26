Public Class messageBox
    Public Shared Function ShowBox(ByVal text As String, Optional ByVal buttons As MessageBoxButton = MessageBoxButton.OK) As Boolean
        Dim msgBox As New messageBox

        msgBox.SetText(text)
        msgBox.SetButtons(buttons)
        Return msgBox.ShowDialog()
    End Function

    Public Shared Sub ShowBox(ByVal ex As Exception)
        Dim stringBuilder As New System.Text.StringBuilder
        Dim msgBox As New messageBox

        With stringBuilder
            .AppendLine(String.Format("Message ---{0}", ex.Message))
            .AppendLine(String.Format("HelpLink ---{0}", ex.HelpLink))
            .AppendLine(String.Format("Source ---{0}", ex.Source))
            .AppendLine(String.Format("StackTrace ---{0}", ex.StackTrace))
            .AppendLine(String.Format("TargetSite ---{0}", ex.TargetSite))
        End With

        msgBox.SetText(stringBuilder.ToString)
        msgBox.btnCopyToClipboard.Visibility = Windows.Visibility.Visible
        msgBox.ShowDialog()

        System.Windows.Threading.Dispatcher.Run()
    End Sub

    Public Sub SetText(ByVal text As String)
        MessageTextBox.Text = text
    End Sub

    Public Sub SetButtons(buttons As MessageBoxButton)
        Select Case buttons
            Case MessageBoxButton.YesNo
                btnOk.Visibility = Windows.Visibility.Hidden
                btnYes.Visibility = Windows.Visibility.Visible
                btnNo.Visibility = Windows.Visibility.Visible
            Case MessageBoxButton.OK
                btnOk.Visibility = Windows.Visibility.Visible
                btnYes.Visibility = Windows.Visibility.Hidden
                btnNo.Visibility = Windows.Visibility.Hidden
        End Select
    End Sub

    Private Sub btnOk_Click(sender As Object, e As RoutedEventArgs) Handles btnOk.Click
        Try
            Me.Close()
        Catch ex As Exception
            messageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub btnYes_Click(sender As Object, e As RoutedEventArgs) Handles btnYes.Click
        Try
            Me.DialogResult = True
            Me.Close()
        Catch ex As Exception
            messageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub btnNo_Click(sender As Object, e As RoutedEventArgs) Handles btnNo.Click
        Try
            Me.DialogResult = False
            Me.Close()
        Catch ex As Exception
            messageBox.ShowBox(ex)
        End Try
    End Sub

    Private Sub btnCopyToClipboard_Click(sender As Object, e As RoutedEventArgs) Handles btnCopyToClipboard.Click
        Try
            Clipboard.SetText(MessageTextBox.Text.Trim)
        Catch ex As Exception
            messageBox.ShowBox(ex)
        End Try
    End Sub
End Class
