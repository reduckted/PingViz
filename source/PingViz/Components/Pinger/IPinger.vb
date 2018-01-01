Imports System.Threading


Public Interface IPinger

    Function PingAsync(
            address As String,
            timeout As TimeSpan,
            cancellationToken As CancellationToken
        ) As Task(Of TimeSpan?)

End Interface
