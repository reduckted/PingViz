Imports Microsoft.EntityFrameworkCore
Imports Microsoft.EntityFrameworkCore.Infrastructure
Imports Microsoft.EntityFrameworkCore.Migrations


Namespace Migrations

    <DbContext(GetType(Database))>
    <Migration("20171228014130_History")>
    Partial Class History

        Protected Overrides Sub BuildTargetModel(modelBuilder As ModelBuilder)
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
