Imports System.Net.NetworkInformation
Imports System.Threading


<RegisterAs(GetType(IPinger))>
Public Class Pinger
    Implements IPinger


    Public Async Function PingAsync(
            address As String,
            timeout As TimeSpan,
            cancellationToken As CancellationToken
        ) As Task(Of TimeSpan?) _
        Implements IPinger.PingAsync

        Dim cancelled As TaskCompletionSource(Of PingReply)


        cancelled = New TaskCompletionSource(Of PingReply)

        Using cancellationToken.Register(Sub() cancelled.TrySetCanceled())
            Try
                Dim timer As Stopwatch
                Dim reply As PingReply


                timer = Stopwatch.StartNew()

                reply = Await Task.WhenAny(
                    (New Ping).SendPingAsync(address, CInt(timeout.TotalMilliseconds)),
                    cancelled.Task
                ).Unwrap()

                timer.Stop()

                If reply.Status = IPStatus.Success Then
                    Return timer.Elapsed
                End If

            Catch ex As PingException
                ' The address is invalid, or something 
                ' like that. Treat this like a timeout.
            End Try

            Return Nothing
        End Using
    End Function

End Class
