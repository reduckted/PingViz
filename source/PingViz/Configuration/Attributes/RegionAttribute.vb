<AttributeUsage(AttributeTargets.Class)>
Public Class RegionAttribute
    Inherits Attribute


    Public Sub New(name As String)
        Me.Name = name
    End Sub


    Public ReadOnly Property Name As String

End Class
