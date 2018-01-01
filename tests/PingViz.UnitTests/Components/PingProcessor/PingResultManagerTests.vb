Public Class PingResultManagerTests

    <Fact()>
    Public Sub EmitsResult()
        Dim manager As PingResultManager
        Dim results As List(Of PingResult)


        results = New List(Of PingResult)

        manager = New PingResultManager
        manager.Results.Subscribe(Sub(x) results.Add(x))

        manager.Emit(New PingResult With {.Timestamp = #2001-01-01 1:10#, .Duration = TimeSpan.FromSeconds(1)})
        manager.Emit(New PingResult With {.Timestamp = #2002-02-02 2:20#, .Duration = Nothing})

        Assert.Equal(2, results.Count)

        Assert.Equal(#2001-01-01 1:10#, results(0).Timestamp)
        Assert.Equal(TimeSpan.FromSeconds(1), results(0).Duration)

        Assert.Equal(#2002-02-02 2:20#, results(1).Timestamp)
        Assert.Null(results(1).Duration)
    End Sub

End Class
