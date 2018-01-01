Imports Microsoft.EntityFrameworkCore.Migrations


Namespace Migrations

    Partial Class History
        Inherits Migration

        Protected Overrides Sub Up(migrationBuilder As MigrationBuilder)
            migrationBuilder.CreateTable(
                name:="PingRecords",
                columns:=Function(table) New With {
                    .ID = table.Column(Of Integer)(nullable:=False).Annotation("Sqlite:Autoincrement", True),
                    .Duration = table.Column(Of Integer)(nullable:=True),
                    .Timestamp = table.Column(Of Date)(nullable:=False)
                },
                constraints:=Sub(table) table.PrimaryKey("PK_PingRecords", Function(x) x.ID)
            )
        End Sub


        Protected Overrides Sub Down(migrationBuilder As MigrationBuilder)
            migrationBuilder.DropTable(name:="PingRecords")
        End Sub

    End Class

End Namespace
