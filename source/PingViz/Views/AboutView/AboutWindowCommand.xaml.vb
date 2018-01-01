Imports Prism.Commands
Imports Prism.Regions


Namespace Views

    <ViewSortHint("2")>
    <Region(Regions.WindowCommands)>
    Public Class AboutWindowCommand

        Public Sub New(regionManager As IRegionManager)
            InitializeComponent()

            OpenAboutButton.Command = New DelegateCommand(
                Sub() regionManager.RequestNavigate(Regions.Flyouts, AboutView.Uri)
            )
        End Sub

    End Class

End Namespace
