Namespace NotifyIcon.MenuItems

    Public MustInherit Class ItemBase
        Implements IMenuItem


        Public MustOverride ReadOnly Property IsStartOfGroup As Boolean _
            Implements IMenuItem.IsStartOfGroup


        Public MustOverride ReadOnly Property Header As String _
            Implements IMenuItem.Header


        Public MustOverride ReadOnly Property Order As Integer _
            Implements IMenuItem.Order


        Public Event CanExecuteChanged As EventHandler _
            Implements ICommand.CanExecuteChanged


        Public MustOverride Sub Execute(parameter As Object) _
            Implements ICommand.Execute


        Public Overridable Function CanExecute(parameter As Object) As Boolean _
            Implements ICommand.CanExecute

            Return True
        End Function
    End Class

End Namespace
