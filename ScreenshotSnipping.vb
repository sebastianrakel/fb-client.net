Imports System
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

Public Class ScreenshotSnipping
    Inherits System.Windows.Forms.Form

    Public Shared Function Snip() As Image
        Dim rc = Screen.PrimaryScreen.Bounds
        Using bmp As New Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
            Using gr As Graphics = Graphics.FromImage(bmp)
                gr.CopyFromScreen(0, 0, 0, 0, bmp.Size)
            End Using
            Using snipper = New ScreenshotSnipping(bmp)
                If snipper.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                    Return snipper.Image
                End If
            End Using
            Return Nothing
        End Using
    End Function

    Private Shared Function GetImageCrop(ByVal SourceImage As Bitmap, CropArea As Rectangle, Optional WithCorrection As Boolean = False) As Bitmap
        Dim pNewBmp As Bitmap
        If WithCorrection Then
            ' X und Y korrigieren
            If CropArea.X < 0 Then CropArea.X = 0
            If CropArea.Y < 0 Then CropArea.Y = 0
            ' keine Höhe oder Breite mit 0
            If CropArea.X > SourceImage.Width Then Return Nothing
            If CropArea.Y > SourceImage.Height Then Return Nothing
            ' Breite und Höhe korrigieren
            If CropArea.Width < 0 Then CropArea.Width = SourceImage.Width
            If CropArea.Height < 0 Then CropArea.Height = SourceImage.Height
            If CropArea.X + CropArea.Width > SourceImage.Width Then CropArea.Width = SourceImage.Width - CropArea.X
            If CropArea.Y + CropArea.Height > SourceImage.Height Then CropArea.Height = SourceImage.Height - CropArea.Y
            ' keine Höhe oder Breite mit 0 nach Korrektur
            If CropArea.Width = 0 Then Return Nothing
            If CropArea.Height = 0 Then Return Nothing
        End If
        ' Bildausschnitt erzeugen
        pNewBmp = New Bitmap(CropArea.Width, CropArea.Height, Imaging.PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(pNewBmp)
            g.DrawImage(SourceImage, New Rectangle(0, 0, CropArea.Width, CropArea.Height), CropArea, GraphicsUnit.Pixel)
        End Using
        ' Originalauflösung setzen
        pNewBmp.SetResolution(SourceImage.HorizontalResolution, SourceImage.VerticalResolution)
        Return pNewBmp
    End Function


    Public Sub New(ByVal screenShot As Image)
        Me.BackgroundImage = screenShot
        Me.ShowInTaskbar = False
        Me.FormBorderStyle = FormBorderStyle.None
        Me.WindowState = FormWindowState.Maximized
        Me.DoubleBuffered = True
    End Sub
    Public Property Image() As Image
        Get
            Return m_Image
        End Get
        Set(ByVal value As Image)
            m_Image = value
        End Set
    End Property
    Private m_Image As Image
    Private rcSelect As New Rectangle()
    Private pntStart As Point
    Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)
        ' Start the snip on mouse down
        If e.Button <> MouseButtons.Left Then
            Return
        End If
        pntStart = e.Location
        rcSelect = New Rectangle(e.Location, New Size(0, 0))
        Me.Invalidate()
    End Sub
    Protected Overrides Sub OnMouseMove(ByVal e As MouseEventArgs)
        ' Modify the selection on mouse move
        If e.Button <> MouseButtons.Left Then
            Return
        End If
        Dim x1 As Integer = Math.Min(e.X, pntStart.X)
        Dim y1 As Integer = Math.Min(e.Y, pntStart.Y)
        Dim x2 As Integer = Math.Max(e.X, pntStart.X)
        Dim y2 As Integer = Math.Max(e.Y, pntStart.Y)
        rcSelect = New Rectangle(x1, y1, x2 - x1, y2 - y1)
        Me.Invalidate()
    End Sub
    Protected Overrides Sub OnMouseUp(ByVal e As MouseEventArgs)
        ' Complete the snip on mouse-up
        If rcSelect.Width <= 0 OrElse rcSelect.Height <= 0 Then
            Return
        End If
        Me.Image = GetImageCrop(CType(Me.BackgroundImage, Bitmap), rcSelect)
        'Image = New Bitmap(rcSelect.Width, rcSelect.Height)
        'Using gr As Graphics = Graphics.FromImage(Image)
        '    gr.DrawImage(Me.BackgroundImage, New Rectangle(0, 0, Image.Width, Image.Height), rcSelect, GraphicsUnit.Pixel)
        '    gr.Save()
        '    gr.Flush()
        'End Using
        DialogResult = DialogResult.OK
    End Sub
    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        ' Draw the current selection
        Using br As Brush = New SolidBrush(Color.FromArgb(120, Color.White))
            Dim x1 As Integer = rcSelect.X
            Dim x2 As Integer = rcSelect.X + rcSelect.Width
            Dim y1 As Integer = rcSelect.Y
            Dim y2 As Integer = rcSelect.Y + rcSelect.Height
            e.Graphics.FillRectangle(br, New Rectangle(0, 0, x1, Me.Height))
            e.Graphics.FillRectangle(br, New Rectangle(x2, 0, Me.Width - x2, Me.Height))
            e.Graphics.FillRectangle(br, New Rectangle(x1, 0, x2 - x1, y1))
            e.Graphics.FillRectangle(br, New Rectangle(x1, y2, x2 - x1, Me.Height - y2))
        End Using
        Using pen As New Pen(Color.Red, 2)
            e.Graphics.DrawRectangle(pen, rcSelect)
        End Using
    End Sub
    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, ByVal keyData As Keys) As Boolean
        ' Allow canceling the snip with the Escape key
        If keyData = Keys.Escape Then
            Me.DialogResult = DialogResult.Cancel
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function
End Class