Namespace NotifyIcon.MenuItems

    <RegisterAs(GetType(IMenuItem))>
    Public Class ExitItem
        Inherits ItemBase


        Public Overrides ReadOnly Property IsStartOfGroup As Boolean
            Get
                Return True
            End Get
        End Property


        Public Overrides ReadOnly Property Header As String
            Get
                Return "Exit"
            End Get
        End Property


        Public Overrides ReadOnly Property Order As Integer
            Get
                Return 10000
            End Get
        End Property


        Public Overrides Sub Execute(parameter As Object)
            Windows.Application.Current.Shutdown()
        End Sub

    End Class

End Namespace
