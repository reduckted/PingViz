Public Class DateTimeProviderTests

    Public Class GetDateTimeMethod

        <Fact()>
        Public Sub ReturnsLocalTime()
            Dim before As Date
            Dim after As Date
            Dim actual As Date


            before = Date.Now
            actual = (New DateTimeProvider).GetDateTime()
            after = Date.Now

            Assert.True(before <= actual)
            Assert.True(after >= actual)
        End Sub

    End Class

End Class
