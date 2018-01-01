Public Class TimeSpanExtensionsTests

    Public Class ToMillisecondsMethod

        <Fact()>
        Public Sub ConvertsNonNullTimeSpanToMilliseconds()
            Dim ts As TimeSpan?


            ts = TimeSpan.FromMilliseconds(12345)

            Assert.Equal(12345, ts.ToMilliseconds())
        End Sub


        <Fact()>
        Public Sub ConvertsNullTimeSpanToNull()
            Dim ts As TimeSpan?


            ts = Nothing

            Assert.Null(ts.ToMilliseconds())
        End Sub


        <Theory()>
        <InlineData(1.4, 1)>
        <InlineData(1.5, 1)>
        <InlineData(1.6, 1)>
        <InlineData(2.4, 2)>
        <InlineData(2.5, 2)>
        <InlineData(2.6, 2)>
        <InlineData(-2.4, -2)>
        <InlineData(-2.5, -2)>
        <InlineData(-2.6, -2)>
        Public Sub TruncatesMillisecondFractions(
                input As Double,
                expected As Integer
            )

            Dim ts As TimeSpan?


            ' Using `FromMilliseconds` seems to round the number of milliseconds
            ' to a whole number, so we will use FromTicks instead.
            ts = TimeSpan.FromTicks(CInt(input * TimeSpan.TicksPerMillisecond))

            Assert.Equal(expected, ts.ToMilliseconds())
        End Sub

    End Class

End Class
