<RegisterAs(GetType(IUrlOpener))>
Public Class UrlOpener
    Implements IUrlOpener


    Private ReadOnly cgErrorHandler As IErrorHandler


    Public Sub New(errorHandler As IErrorHandler)
        cgErrorHandler = errorHandler
    End Sub


    Public Sub Open(url As String) _
        Implements IUrlOpener.Open

        Dim info As ProcessStartInfo


        info = New ProcessStartInfo With {
            .Verb = "open",
            .UseShellExecute = True,
            .FileName = url
        }

        Try
            Using Process.Start(info)
                ' Just dispose of the process.
            End Using

        Catch ex As Exception
            cgErrorHandler.Handle($"Failed to open URL: {ex.Message}")
        End Try
    End Sub

End Class
