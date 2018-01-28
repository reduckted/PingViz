Imports OxyPlot
Imports Prism.Regions


Namespace Views

    <Region(Regions.Tabs)>
    <ViewSortHint("0")>
    Public Class LiveView

        Public Sub New()
            InitializeComponent()

            Chart.Controller = New PlotController
            Chart.Controller.UnbindAll()
            Chart.Controller.BindMouseEnter(PlotCommands.HoverPointsOnlyTrack)
        End Sub

    End Class

End Namespace
