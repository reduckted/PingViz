<RegisterAs(GetType(IDateTimeProvider))>
Public Class DateTimeProvider
    Implements IDateTimeProvider


    Public Function GetDateTime() As Date _
        Implements IDateTimeProvider.GetDateTime

        Return Date.Now
    End Function

End Class
