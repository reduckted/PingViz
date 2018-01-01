<AttributeUsage(AttributeTargets.Class)>
Public Class RegionAdapterAttribute
    Inherits Attribute


    Public Sub New(controlType As Type)
        Me.ControlType = controlType
    End Sub


    Public ReadOnly Property ControlType As Type

End Class
