Imports Prism.Regions


Namespace Views

    Class MainWindow

        Public Sub New(regionManager As IRegionManager)
            InitializeComponent()

            If regionManager Is Nothing Then
                Throw New ArgumentNullException(NameOf(regionManager))
            End If

            ' The RegionManager attached properties don't work when set via XAML because
            ' the objects are ouside the logical tree, and cannot find the parent that
            ' contains the region manager. We need to set these properties via code.
            SetRegionManager(FlyoutsRegion, Regions.Flyouts, regionManager)
            SetRegionManager(WindowCommandsRegion, Regions.WindowCommands, regionManager)
        End Sub


        Private Sub SetRegionManager(
                element As FrameworkElement,
                name As String,
                manager As IRegionManager
            )

            RegionManager.SetRegionName(element, name)
            RegionManager.SetRegionManager(element, manager)
        End Sub

    End Class

End Namespace
