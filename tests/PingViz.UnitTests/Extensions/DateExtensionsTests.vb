Public Class DateExtensionsTests

    Public Class RoundDownMethod

        <Theory()>
        <MemberData(NameOf(GetRoundsDownCorrectlyData))>
        Public Sub RoundsDownCorrectly(
                time As Date,
                seconds As Double,
                expected As Date
            )

            Dim actual As Date


            actual = time.RoundDown(TimeSpan.FromSeconds(seconds))

            Assert.Equal(expected, actual)
        End Sub


        Public Shared Iterator Function GetRoundsDownCorrectlyData() As IEnumerable(Of Object())
            For i = 0 To 14
                Yield {#2017-01-02 12:19:00#.AddSeconds(i), 15, #2017-01-02 12:19:00#}
            Next i

            For i = 10 To 19
                Yield {#2017-01-02 12:19:00#.AddSeconds(i), 10, #2017-01-02 12:19:10#}
            Next i

            For i = 20 To 24
                Yield {#2017-01-02 12:19:00#.AddSeconds(i), 5, #2017-01-02 12:19:20#}
            Next i

            For i = 30 To 31
                Yield {#2017-01-02 12:19:00#.AddSeconds(i), 2, #2017-01-02 12:19:30#}
            Next i

            For i = 40 To 50
                Yield {#2017-01-02 12:19:00#.AddSeconds(i).AddMilliseconds(1), 1, #2017-01-02 12:19:00#.AddSeconds(i)}
            Next i

            For i = 0 To 400 Step 100
                Yield {#2017-01-02 12:19:00#.AddMilliseconds(i), 0.5, #2017-01-02 12:19:00#}
            Next i

            For i = 500 To 900 Step 100
                Yield {#2017-01-02 12:19:00#.AddMilliseconds(i), 0.5, #2017-01-02 12:19:00#.AddMilliseconds(500)}
            Next i
        End Function

    End Class

End Class
