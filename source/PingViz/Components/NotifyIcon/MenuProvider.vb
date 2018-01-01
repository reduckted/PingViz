Namespace NotifyIcon

    <RegisterAs(GetType(IMenuProvider))>
    Public Class MenuProvider
        Implements IMenuProvider


        Private ReadOnly cgMenu As ContextMenu


        Public Sub New(items As IEnumerable(Of IMenuItem))
            If items Is Nothing Then
                Throw New ArgumentNullException(NameOf(items))
            End If

            cgMenu = New ContextMenu

            For Each item In items.OrderBy(Function(x) x.Order)
                If item.IsStartOfGroup AndAlso (cgMenu.Items.Count > 0) Then
                    cgMenu.Items.Add(New Separator)
                End If

                cgMenu.Items.Add(New MenuItem With {.Header = item.Header, .Command = item})
            Next item
        End Sub


        Public Function GetContextMenu() As ContextMenu _
            Implements IMenuProvider.GetContextMenu

            Return cgMenu
        End Function

    End Class

End Namespace
