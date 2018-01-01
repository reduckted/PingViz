<RegisterAs(GetType(IUrlOpener))>
Public Class UrlOpener
    Implements IUrlOpener


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
            Debug.WriteLine(ex)
        End Try
    End Sub

End Class
