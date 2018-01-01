Namespace NotifyIcon

    Public Interface IMenuItem
        Inherits ICommand


        ReadOnly Property IsStartOfGroup As Boolean


        ReadOnly Property Header As String


        ReadOnly Property Order As Integer

    End Interface

End Namespace
