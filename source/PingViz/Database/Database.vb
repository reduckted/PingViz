Imports Microsoft.EntityFrameworkCore
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Logging.Debug
Imports System.IO


<RegisterAs(GetType(IDatabase))>
Public NotInheritable Class Database
    Inherits DbContext
    Implements IDatabase


    Private Shared ReadOnly LoggerFactory As New LoggerFactory(GetLoggerProviders())


    Private ReadOnly cgDataDirectoryProvider As IDataDirectoryProvider


    Private Shared Iterator Function GetLoggerProviders() As IEnumerable(Of ILoggerProvider)
#If DEBUG Then
        Yield New DebugLoggerProvider(Function(category, level) String.Equals(category, DbLoggerCategory.Database.Command.Name))
#End If
    End Function


    Public Sub New(dataDirectoryProvider As IDataDirectoryProvider)
        cgDataDirectoryProvider = dataDirectoryProvider
    End Sub


    Protected Overrides Sub OnConfiguring(optionsBuilder As DbContextOptionsBuilder)
        If Not optionsBuilder.IsConfigured Then
            optionsBuilder _
                .UseSqlite(GetConnectionString()) _
                .UseLoggerFactory(LoggerFactory)
        End If
    End Sub


    Private Function GetConnectionString() As String
        Dim dir As String


        dir = cgDataDirectoryProvider.GetDirectory()
        Directory.CreateDirectory(dir)

        Return $"Data Source={Path.Combine(dir, "history.db")}"
    End Function


    Public ReadOnly Property PingRecords As DbSet(Of PingRecord) _
        Implements IDatabase.PingRecords

        Get
            Return [Set](Of PingRecord)()
        End Get
    End Property


    Public Overloads Function SaveChangesAsync() As Task _
        Implements IDatabase.SaveChangesAsync

        Return MyBase.SaveChangesAsync()
    End Function


    Public Overloads Sub Dispose() _
        Implements IDisposable.Dispose

        MyBase.Dispose()
    End Sub

End Class
