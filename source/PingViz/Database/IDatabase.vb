Imports Microsoft.EntityFrameworkCore


Public Interface IDatabase
    Inherits IDisposable


    ReadOnly Property PingRecords As DbSet(Of PingRecord)


    Function SaveChangesAsync() As Task

End Interface
