Imports PingViz
Imports Quartz


<RegisterAs(GetType(PingJob))>
Public Class PingJob
    Implements IJob


    Private Shared ReadOnly Timeout As TimeSpan = TimeSpan.FromSeconds(Application.PingInterval.TotalSeconds / 2)


    Private ReadOnly cgPinger As IPinger
    Private ReadOnly cgSettingsManager As ISettingsManager
    Private ReadOnly cgDateTimeProvider As IDateTimeProvider
    Private ReadOnly cgDatabase As IDatabase
    Private ReadOnly cgResultEmitter As IPingResultEmitter
    Private ReadOnly cgErrorHandler As IErrorHandler


    Public Sub New(
            pinger As IPinger,
            settingsManager As ISettingsManager,
            dateTimeProvider As IDateTimeProvider,
            database As IDatabase,
            resultEmitter As IPingResultEmitter,
            errorHandler As IErrorHandler
        )

        If pinger Is Nothing Then
            Throw New ArgumentNullException(NameOf(pinger))
        End If

        If settingsManager Is Nothing Then
            Throw New ArgumentNullException(NameOf(settingsManager))
        End If

        If dateTimeProvider Is Nothing Then
            Throw New ArgumentNullException(NameOf(dateTimeProvider))
        End If

        If database Is Nothing Then
            Throw New ArgumentNullException(NameOf(database))
        End If

        If resultEmitter Is Nothing Then
            Throw New ArgumentNullException(NameOf(resultEmitter))
        End If

        If errorHandler Is Nothing Then
            Throw New ArgumentNullException(NameOf(errorHandler))
        End If

        cgPinger = pinger
        cgSettingsManager = settingsManager
        cgDateTimeProvider = dateTimeProvider
        cgDatabase = database
        cgResultEmitter = resultEmitter
        cgErrorHandler = errorHandler
    End Sub


    Public Async Function Execute(context As IJobExecutionContext) As Task _
        Implements IJob.Execute

        Try
            Dim result As PingResult
            Dim record As PingRecord


            ' We probably won't execute on the *exact* time that we want,
            ' so get the current time and round it down to the nearest interval.
            ' This will probably only move the timestamp back by a second or two.
            result.Timestamp = cgDateTimeProvider.GetDateTime().RoundDown(Application.PingInterval)

            result.Duration = Await cgPinger.PingAsync(cgSettingsManager.PingAddress, Timeout, context.CancellationToken)

            record = New PingRecord With {
                .Timestamp = result.Timestamp,
                .Duration = result.Duration.ToMilliseconds()
            }

            cgDatabase.PingRecords.Add(record)
            Await cgDatabase.SaveChangesAsync()

            cgResultEmitter.Emit(result)

        Catch ex As TaskCanceledException
            ' Suppress this.

        Catch ex As Exception
            cgErrorHandler.Handle($"Failed to record a ping: {ex.Message}")
        End Try
    End Function

End Class
