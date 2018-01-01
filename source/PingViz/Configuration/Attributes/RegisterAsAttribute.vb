<AttributeUsage(AttributeTargets.Class, AllowMultiple:=True)>
Public Class RegisterAsAttribute
    Inherits Attribute


    Public Sub New(type As Type)
        Types = {type}
    End Sub


    Public Sub New(types() As Type)
        Me.Types = types
    End Sub


    Public ReadOnly Property Types As IEnumerable(Of Type)


    Public Property SingleInstance As Boolean

End Class
