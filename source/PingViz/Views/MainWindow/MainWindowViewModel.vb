Namespace Views

    Public Class MainWindowViewModel

        Public ReadOnly Property Title As String
            Get
                Return My.Application.Info.Title
            End Get
        End Property

    End Class

End Namespace
