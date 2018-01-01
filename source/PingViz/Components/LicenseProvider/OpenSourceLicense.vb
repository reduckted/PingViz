Public Class OpenSourceLicense

    Public Sub New(
            name As String,
            url As String,
            text As String
        )

        Me.Name = name
        Me.Url = url
        Me.Text = text
    End Sub


    Public ReadOnly Property Name As String


    Public ReadOnly Property Url As String


    Public ReadOnly Property Text As String

End Class
