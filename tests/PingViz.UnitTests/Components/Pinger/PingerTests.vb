Imports System.Threading


Public Class PingerTests

    Public Class PingAsyncMethod

        <Fact()>
        Public Async Function ReturnsTheElapsedTimeWhenSuccessfulAsync() As Task
            Dim pinger As Pinger
            Dim result As TimeSpan?


            pinger = New Pinger
            result = Await pinger.PingAsync("localhost", TimeSpan.FromSeconds(2), CancellationToken.None)

            Assert.NotNull(result)
        End Function


        <Fact()>
        Public Async Function ReturnsNullWhenUnsuccessful() As Task
            Dim pinger As Pinger
            Dim result As TimeSpan?


            pinger = New Pinger
            result = Await pinger.PingAsync("asdasdasdasd", TimeSpan.FromSeconds(2), CancellationToken.None)

            Assert.Null(result)
        End Function


        <Fact()>
        Public Async Function ThrowsWhenCancelled() As Task
            Dim pinger As Pinger
            Dim cancellationSource As CancellationTokenSource


            cancellationSource = New CancellationTokenSource
            cancellationSource.Cancel()

            pinger = New Pinger

            Await Assert.ThrowsAnyAsync(Of OperationCanceledException)(
                Function() pinger.PingAsync("localhost", TimeSpan.FromSeconds(2), cancellationSource.Token)
            )
        End Function

    End Class

End Class
