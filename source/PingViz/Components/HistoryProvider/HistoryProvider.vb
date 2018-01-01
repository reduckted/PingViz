Imports Microsoft.EntityFrameworkCore


<RegisterAs(GetType(IHistoryProvider))>
Public Class HistoryProvider
    Implements IHistoryProvider


    Private ReadOnly cgDatabase As IDatabase


    Public Sub New(database As IDatabase)
        If database Is Nothing Then
            Throw New ArgumentNullException(NameOf(database))
        End If

        cgDatabase = database
    End Sub


    Public Async Function GetPingsAsync(
                startDateTime As Date,
                endDateTime As Date
            ) As Task(Of IEnumerable(Of PingResult)) _
            Implements IHistoryProvider.GetPingsAsync

        Dim records As IEnumerable(Of (Timestamp As Date, Duration As Integer?))


        records = Await (
            From record In cgDatabase.PingRecords
            Where record.Timestamp <= endDateTime
            Where record.Timestamp >= startDateTime
            Order By record.Timestamp
            Select (record.Timestamp, record.Duration)
        ).ToListAsync()

        Return records.Select(AddressOf CreatePingResult)
    End Function


    Private Shared Function CreatePingResult(record As (Timestamp As Date, Duration As Integer?)) As PingResult
        Dim result As PingResult


        result = New PingResult With {
            .Timestamp = record.Timestamp
        }

        If record.Duration.HasValue Then
            result.Duration = TimeSpan.FromMilliseconds(record.Duration.Value)
        End If

        Return result
    End Function

End Class
