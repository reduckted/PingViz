Imports Microsoft.EntityFrameworkCore
Imports Microsoft.EntityFrameworkCore.Infrastructure


Namespace Migrations

    <DbContext(GetType(Database))>
    Partial Class DatabaseModelSnapshot
        Inherits ModelSnapshot

        Protected Overrides Sub BuildModel(modelBuilder As ModelBuilder)
            modelBuilder.HasAnnotation("ProductVersion", "2.0.1-rtm-125")

            modelBuilder.Entity(
                "PingViz.PingRecord",
                Sub(b)
                    b.Property(Of Integer)("ID").ValueGeneratedOnAdd()
                    b.Property(Of Integer?)("Duration")
                    b.Property(Of Date)("Timestamp")
                    b.HasKey("ID")
                    b.ToTable("PingRecords")
                End Sub
            )
        End Sub

    End Class

End Namespace
