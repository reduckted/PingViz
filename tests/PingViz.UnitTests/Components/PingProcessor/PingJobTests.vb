Imports Microsoft.EntityFrameworkCore
Imports Quartz
Imports System.Threading


Public Class PingJobTests

    Public Class Constructor

        <Fact()>
        Public Sub RejectsNullPinger()
            Dim pinger As IPinger
            Dim settingsManager As ISettingsManager
            Dim dateTimeProvider As IDateTimeProvider
            Dim database As IDatabase
            Dim emitter As IPingResultEmitter


            pinger = Nothing
            settingsManager = Mock.Of(Of ISettingsManager)
            dateTimeProvider = Mock.Of(Of IDateTimeProvider)
            database = Mock.Of(Of IDatabase)
            emitter = Mock.Of(Of IPingResultEmitter)

            Assert.Throws(Of ArgumentNullException)(
                Function() New PingJob(pinger, settingsManager, dateTimeProvider, database, emitter)
            )
        End Sub


        <Fact()>
        Public Sub RejectsNullSettingsManager()
            Dim pinger As IPinger
            Dim settingsManager As ISettingsManager
            Dim dateTimeProvider As IDateTimeProvider
            Dim database As IDatabase
            Dim emitter As IPingResultEmitter


            pinger = Mock.Of(Of IPinger)
            settingsManager = Nothing
            dateTimeProvider = Mock.Of(Of IDateTimeProvider)
            database = Mock.Of(Of IDatabase)
            emitter = Mock.Of(Of IPingResultEmitter)

            Assert.Throws(Of ArgumentNullException)(
                Function() New PingJob(pinger, settingsManager, dateTimeProvider, database, emitter)
            )
        End Sub


        <Fact()>
        Public Sub RejectsNullDateTimeProvider()
            Dim pinger As IPinger
            Dim settingsManager As ISettingsManager
            Dim dateTimeProvider As IDateTimeProvider
            Dim database As IDatabase
            Dim emitter As IPingResultEmitter


            pinger = Mock.Of(Of IPinger)
            settingsManager = Mock.Of(Of ISettingsManager)
            dateTimeProvider = Nothing
            database = Mock.Of(Of IDatabase)
            emitter = Mock.Of(Of IPingResultEmitter)

            Assert.Throws(Of ArgumentNullException)(
                Function() New PingJob(pinger, settingsManager, dateTimeProvider, database, emitter)
            )
        End Sub


        <Fact()>
        Public Sub RejectsNullDatabase()
            Dim pinger As IPinger
            Dim settingsManager As ISettingsManager
            Dim dateTimeProvider As IDateTimeProvider
            Dim database As IDatabase
            Dim emitter As IPingResultEmitter


            pinger = Mock.Of(Of IPinger)
            settingsManager = Mock.Of(Of ISettingsManager)
            dateTimeProvider = Mock.Of(Of IDateTimeProvider)
            database = Nothing
            emitter = Mock.Of(Of IPingResultEmitter)

            Assert.Throws(Of ArgumentNullException)(
                Function() New PingJob(pinger, settingsManager, dateTimeProvider, database, emitter)
            )
        End Sub


        <Fact()>
        Public Sub RejectsNullResultEmitter()
            Dim pinger As IPinger
            Dim settingsManager As ISettingsManager
            Dim dateTimeProvider As IDateTimeProvider
            Dim database As IDatabase
            Dim emitter As IPingResultEmitter


            pinger = Mock.Of(Of IPinger)
            settingsManager = Mock.Of(Of ISettingsManager)
            dateTimeProvider = Mock.Of(Of IDateTimeProvider)
            database = Mock.Of(Of IDatabase)
            emitter = Nothing

            Assert.Throws(Of ArgumentNullException)(
                Function() New PingJob(pinger, settingsManager, dateTimeProvider, database, emitter)
            )
        End Sub

    End Class


    Public NotInheritable Class ExecuteMethod
        Implements IDisposable


        Private ReadOnly cgDatabaseOptions As DbContextOptions(Of MockDatabase)


        Public Sub New()
            cgDatabaseOptions = (New DbContextOptionsBuilder(Of MockDatabase)).UseInMemoryDatabase("PingJob.Execute").Options
        End Sub


        <Theory()>
        <MemberData(NameOf(GetRoundsDownToTheNearestTenSecondsData))>
        Public Async Function RoundsDownToTheNearestTenSeconds(
                time As Date,
                expected As Date
            ) As Task

            Using database = New MockDatabase(cgDatabaseOptions)
                Dim job As PingJob
                Dim context As IJobExecutionContext


                context = Mock.Of(Of IJobExecutionContext)

                job = New PingJob(
                    CreateMockPinger(),
                    CreateMockSettingsManager(),
                    CreateMockDateTimeProvider(time:=time),
                    database,
                    Mock.Of(Of IPingResultEmitter)
                )

                Await job.Execute(context)
            End Using

            Using database = New MockDatabase(cgDatabaseOptions)
                Assert.Equal(1, database.PingRecords.Count())
                Assert.Equal(expected, database.PingRecords.First().Timestamp)
            End Using
        End Function


        Public Shared Iterator Function GetRoundsDownToTheNearestTenSecondsData() As IEnumerable(Of Object())
            For base = 0 To 50 Step 10
                For offset = base To base + 9
                    Yield {#2017-01-02 12:19:00#.AddSeconds(offset), #2017-01-02 12:19:00#.AddSeconds(base)}
                Next offset
            Next base
        End Function


        <Fact()>
        Public Async Function RemovesMillisecondsFromTheTimestamp() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                Dim job As PingJob
                Dim context As IJobExecutionContext


                context = Mock.Of(Of IJobExecutionContext)

                job = New PingJob(
                    CreateMockPinger(),
                    CreateMockSettingsManager(),
                    CreateMockDateTimeProvider(time:=#2017-02-03 12:34:00#.AddMilliseconds(123)),
                    database,
                    Mock.Of(Of IPingResultEmitter)
                )

                Await job.Execute(context)
            End Using

            Using database = New MockDatabase(cgDatabaseOptions)
                Assert.Equal(1, database.PingRecords.Count())
                Assert.Equal(#2017-02-03 12:34:00#, database.PingRecords.First().Timestamp)
            End Using
        End Function


        <Fact()>
        Public Async Function RecordsNullDurationWhenPingReturnsNull() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                Dim job As PingJob
                Dim context As IJobExecutionContext


                context = Mock.Of(Of IJobExecutionContext)

                job = New PingJob(
                    CreateMockPinger(duration:=Nothing),
                    CreateMockSettingsManager(),
                    CreateMockDateTimeProvider(),
                    database,
                    Mock.Of(Of IPingResultEmitter)
                )

                Await job.Execute(context)
            End Using

            Using database = New MockDatabase(cgDatabaseOptions)
                Assert.Equal(1, database.PingRecords.Count())
                Assert.Null(database.PingRecords.First().Duration)
            End Using
        End Function


        <Fact()>
        Public Async Function RecordsDurationInMillisecondsWhenPingReturnsNonNull() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                Dim job As PingJob
                Dim context As IJobExecutionContext


                context = Mock.Of(Of IJobExecutionContext)

                job = New PingJob(
                    CreateMockPinger(duration:=TimeSpan.FromMilliseconds(12345)),
                    CreateMockSettingsManager(),
                    CreateMockDateTimeProvider(),
                    database,
                    Mock.Of(Of IPingResultEmitter)
                )

                Await job.Execute(context)
            End Using

            Using database = New MockDatabase(cgDatabaseOptions)
                Assert.Equal(1, database.PingRecords.Count())
                Assert.Equal(12345, database.PingRecords.First().Duration)
            End Using
        End Function


        <Fact()>
        Public Async Function UsesTheAddressFromTheSettings() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                Dim job As PingJob
                Dim context As IJobExecutionContext
                Dim pinger As Mock(Of IPinger)


                pinger = Mock.Get(CreateMockPinger())

                context = Mock.Of(Of IJobExecutionContext)

                job = New PingJob(
                    pinger.Object,
                    CreateMockSettingsManager(address:="foo.bar"),
                    CreateMockDateTimeProvider(),
                    database,
                    Mock.Of(Of IPingResultEmitter)
                )

                Await job.Execute(context)

                pinger.Verify(
                    Function(x) x.PingAsync("foo.bar", It.IsAny(Of TimeSpan), It.IsAny(Of CancellationToken)),
                    Times.Once
                )
            End Using
        End Function


        <Fact()>
        Public Async Function UsesTimeoutOfFiveSeconds() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                Dim job As PingJob
                Dim context As IJobExecutionContext
                Dim pinger As Mock(Of IPinger)


                pinger = Mock.Get(CreateMockPinger())

                context = Mock.Of(Of IJobExecutionContext)

                job = New PingJob(
                    pinger.Object,
                    CreateMockSettingsManager(),
                    CreateMockDateTimeProvider(),
                    database,
                    Mock.Of(Of IPingResultEmitter)
                )

                Await job.Execute(context)

                pinger.Verify(
                    Function(x) x.PingAsync(It.IsAny(Of String), TimeSpan.FromSeconds(5), It.IsAny(Of CancellationToken)),
                    Times.Once
                )
            End Using
        End Function


        <Fact()>
        Public Async Function DoesNotAddRecordWhenPingIsCancelled() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                Dim job As PingJob
                Dim context As IJobExecutionContext
                Dim pinger As Mock(Of IPinger)


                pinger = New Mock(Of IPinger)

                pinger _
                    .Setup(Function(x) x.PingAsync(It.IsAny(Of String), It.IsAny(Of TimeSpan), It.IsAny(Of CancellationToken))) _
                    .ThrowsAsync(New TaskCanceledException)

                context = Mock.Of(Of IJobExecutionContext)

                job = New PingJob(
                    pinger.Object,
                    CreateMockSettingsManager(),
                    CreateMockDateTimeProvider(),
                    database,
                    Mock.Of(Of IPingResultEmitter)
                )

                Await job.Execute(context)
            End Using

            Using database = New MockDatabase(cgDatabaseOptions)
                Assert.Equal(0, database.PingRecords.Count())
            End Using
        End Function


        <Fact()>
        Public Async Function EmitsResultWithNullDurationWhenPingReturnsNull() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                Dim job As PingJob
                Dim context As IJobExecutionContext
                Dim emitter As Mock(Of IPingResultEmitter)
                Dim time As Date


                time = #2001-02-03 04:05#

                emitter = New Mock(Of IPingResultEmitter)
                context = Mock.Of(Of IJobExecutionContext)

                job = New PingJob(
                    CreateMockPinger(duration:=Nothing),
                    CreateMockSettingsManager(),
                    CreateMockDateTimeProvider(time:=time),
                    database,
                    emitter.Object
                )

                Await job.Execute(context)

                emitter.Verify(
                    Sub(e) e.Emit(It.Is(Of PingResult)(Function(r) r.Timestamp = time AndAlso (Not r.Duration.HasValue))),
                    Times.Once
                )
            End Using
        End Function


        <Fact()>
        Public Async Function EmitsResultWithDurationWhenPingReturnsNonNull() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                Dim job As PingJob
                Dim context As IJobExecutionContext
                Dim emitter As Mock(Of IPingResultEmitter)
                Dim time As Date
                Dim duration As TimeSpan?


                time = #2001-02-03 04:05#
                duration = TimeSpan.FromMilliseconds(12345)

                emitter = New Mock(Of IPingResultEmitter)
                context = Mock.Of(Of IJobExecutionContext)

                job = New PingJob(
                    CreateMockPinger(duration:=duration),
                    CreateMockSettingsManager(),
                    CreateMockDateTimeProvider(time:=time),
                    database,
                    emitter.Object
                )

                Await job.Execute(context)

                emitter.Verify(
                    Sub(e) e.Emit(It.Is(Of PingResult)(Function(r) r.Timestamp = time AndAlso r.Duration.Equals(duration))),
                    Times.Once()
                )
            End Using
        End Function


        <Fact()>
        Public Async Function DoesNotEmitResultWhenPingIsCancelled() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                Dim job As PingJob
                Dim context As IJobExecutionContext
                Dim pinger As Mock(Of IPinger)
                Dim emitter As Mock(Of IPingResultEmitter)


                emitter = New Mock(Of IPingResultEmitter)
                pinger = New Mock(Of IPinger)
                context = Mock.Of(Of IJobExecutionContext)

                pinger _
                    .Setup(Function(x) x.PingAsync(It.IsAny(Of String), It.IsAny(Of TimeSpan), It.IsAny(Of CancellationToken))) _
                    .ThrowsAsync(New TaskCanceledException)

                job = New PingJob(
                    pinger.Object,
                    CreateMockSettingsManager(),
                    CreateMockDateTimeProvider(),
                    database,
                    emitter.Object
                )

                Await job.Execute(context)

                emitter.Verify(Sub(x) x.Emit(It.IsAny(Of PingResult)), Times.Never)
            End Using
        End Function


        Private Shared Function CreateMockPinger(Optional duration As TimeSpan? = Nothing) As IPinger
            Dim pinger As Mock(Of IPinger)


            pinger = New Mock(Of IPinger)

            pinger _
                .Setup(Function(x) x.PingAsync(It.IsAny(Of String), It.IsAny(Of TimeSpan), It.IsAny(Of CancellationToken))) _
                .ReturnsAsync(duration)

            Return pinger.Object
        End Function


        Private Shared Function CreateMockSettingsManager(Optional address As String = "localhost") As ISettingsManager
            Dim settings As Mock(Of ISettingsManager)


            settings = New Mock(Of ISettingsManager)
            settings.SetupGet(Function(x) x.PingAddress).Returns(address)

            Return settings.Object
        End Function


        Private Shared Function CreateMockDateTimeProvider(Optional time As Date = Nothing) As IDateTimeProvider
            Dim provider As Mock(Of IDateTimeProvider)


            If time = Nothing Then
                time = #2017-01-01 12:34#
            End If

            provider = New Mock(Of IDateTimeProvider)
            provider.Setup(Function(x) x.GetDateTime()).Returns(time)

            Return provider.Object
        End Function


        Public Sub Dispose() _
            Implements IDisposable.Dispose

            Using database = New MockDatabase(cgDatabaseOptions)
                database.Database.EnsureDeleted()
            End Using
        End Sub


        Private Class MockDatabase
            Inherits DbContext
            Implements IDatabase


            Public Sub New(options As DbContextOptions)
                MyBase.New(options)
            End Sub


            Public ReadOnly Property PingRecords As DbSet(Of PingRecord) _
                Implements IDatabase.PingRecords

                Get
                    Return MyBase.Set(Of PingRecord)
                End Get
            End Property


            Public Overloads Function SaveChangesAsync() As Task _
                Implements IDatabase.SaveChangesAsync

                Return MyBase.SaveChangesAsync()
            End Function

        End Class

    End Class

End Class
