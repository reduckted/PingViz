Imports Microsoft.EntityFrameworkCore

Public Class HistoryProviderTests

    Public Class Constructor

        <Fact()>
        Public Sub RejectsNullDatabase()
            Assert.Throws(Of ArgumentNullException)(Function() New HistoryProvider(Nothing))
        End Sub

    End Class


    Public Class GetPingsAsyncMethod
        Implements IDisposable


        Private ReadOnly cgDatabaseOptions As DbContextOptions(Of MockDatabase)


        Public Sub New()
            cgDatabaseOptions = (New DbContextOptionsBuilder(Of MockDatabase)).UseInMemoryDatabase("HistoryProvider.GetPingsAsync").Options
        End Sub


        <Fact()>
        Public Async Function RetriesPingsAtStartDateTime() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-02-03 13:14:49#})
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-02-03 13:14:50#})
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-02-03 13:14:51#})
                Await database.SaveChangesAsync()
            End Using

            Using database = New MockDatabase(cgDatabaseOptions)
                Dim provider As HistoryProvider
                Dim results As List(Of PingResult)


                provider = New HistoryProvider(database)

                results = (Await provider.GetPingsAsync(#2001-02-03 13:14:50#, #2002-01-01#)).ToList()

                Assert.Equal(
                    {#2001-02-03 13:14:50#, #2001-02-03 13:14:51#},
                    results.Select(Function(x) x.Timestamp)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function RetriesPingsAtEndDateTime() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-02-03 13:14:49#})
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-02-03 13:14:50#})
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-02-03 13:14:51#})
                Await database.SaveChangesAsync()
            End Using

            Using database = New MockDatabase(cgDatabaseOptions)
                Dim provider As HistoryProvider
                Dim results As List(Of PingResult)


                provider = New HistoryProvider(database)

                results = (Await provider.GetPingsAsync(#2000-01-01#, #2001-02-03 13:14:50#)).ToList()

                Assert.Equal(
                    {#2001-02-03 13:14:49#, #2001-02-03 13:14:50#},
                    results.Select(Function(x) x.Timestamp)
                )
            End Using
        End Function


        <Fact()>
        Public Async Function ConvertsNonNullDurationToMilliseconds() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-01-01#, .Duration = 12345})
                Await database.SaveChangesAsync()
            End Using

            Using database = New MockDatabase(cgDatabaseOptions)
                Dim provider As HistoryProvider
                Dim results As List(Of PingResult)


                provider = New HistoryProvider(database)

                results = (Await provider.GetPingsAsync(#2000-01-01#, #2002-01-01#)).ToList()

                Assert.Equal(1, results.Count)
                Assert.Equal(TimeSpan.FromMilliseconds(12345), results(0).Duration)
            End Using
        End Function


        <Fact()>
        Public Async Function ConvertsNullDurationToNullTimeSpan() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-01-01#, .Duration = Nothing})
                Await database.SaveChangesAsync()
            End Using

            Using database = New MockDatabase(cgDatabaseOptions)
                Dim provider As HistoryProvider
                Dim results As List(Of PingResult)


                provider = New HistoryProvider(database)

                results = (Await provider.GetPingsAsync(#2000-01-01#, #2002-01-01#)).ToList()

                Assert.Equal(1, results.Count)
                Assert.Null(results(0).Duration)
            End Using
        End Function


        <Fact()>
        Public Async Function OrdersResultsInAscendingOrderByDateTime() As Task
            Using database = New MockDatabase(cgDatabaseOptions)
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-04-01#, .Duration = 1})
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-02-01#, .Duration = 2})
                database.PingRecords.Add(New PingRecord With {.Timestamp = #2001-03-01#, .Duration = 3})
                Await database.SaveChangesAsync()
            End Using

            Using database = New MockDatabase(cgDatabaseOptions)
                Dim provider As HistoryProvider
                Dim results As List(Of PingResult)


                provider = New HistoryProvider(database)

                results = (Await provider.GetPingsAsync(#2000-01-01#, #2002-01-01#)).ToList()

                Assert.Equal(
                    {#2001-02-01#, #2001-03-01#, #2001-04-01#},
                    results.Select(Function(x) x.Timestamp)
                )
            End Using
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
