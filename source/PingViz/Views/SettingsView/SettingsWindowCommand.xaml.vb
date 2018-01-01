Imports Prism.Commands
Imports Prism.Regions


Namespace Views

    <ViewSortHint("1")>
    <Region(Regions.WindowCommands)>
    Public Class SettingsWindowCommand

        Public Sub New(regionManager As IRegionManager)
            InitializeComponent()

            OpenSettingsButton.Command = New DelegateCommand(
                Sub() regionManager.RequestNavigate(Regions.Flyouts, SettingsView.Uri)
            )
        End Sub

    End Class

End Namespace
