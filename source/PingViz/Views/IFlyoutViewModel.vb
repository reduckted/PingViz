Imports MahApps.Metro.Controls


Namespace Views

    Public Interface IFlyoutViewModel

        ReadOnly Property Header As String


        ReadOnly Property Position As Position


        Sub OnOpened()


        Sub OnClosed()

    End Interface

End Namespace
