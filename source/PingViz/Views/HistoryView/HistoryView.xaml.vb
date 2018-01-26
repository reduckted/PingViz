Imports OxyPlot
Imports Prism.Regions
Imports System.Windows.Controls.Primitives


Namespace Views

    <Region(Regions.Tabs)>
    <ViewSortHint("1")>
    Public Class HistoryView

        Public Sub New()
            InitializeComponent()

            Chart.Controller = New PlotController
            Chart.Controller.UnbindAll()
            Chart.Controller.BindMouseWheel(PlotCommands.ZoomWheel)
            Chart.Controller.BindMouseEnter(PlotCommands.HoverPointsOnlyTrack)
            Chart.Controller.BindMouseDown(OxyMouseButton.Left, PlotCommands.PanAt)
            Chart.Controller.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.Shift, PlotCommands.ZoomRectangle)
            Chart.Controller.BindTouchDown(PlotCommands.PanZoomByTouch)
            Chart.Controller.BindTouchDown(PlotCommands.SnapTrackTouch)
        End Sub


        Private Sub OpenJumpToDatePopup(
                sender As Object,
                e As RoutedEventArgs
            )

            JumpToDatePopup.PlacementTarget = ControlPanel
            JumpToDatePopup.Placement = PlacementMode.Custom
            JumpToDatePopup.CustomPopupPlacementCallback = AddressOf PositionJumpToDatePopup
            JumpToDatePopup.IsOpen = True
        End Sub


        Private Function PositionJumpToDatePopup(
                popupSize As Size,
                targetSize As Size,
                offset As Point
            ) As CustomPopupPlacement()

            Dim left As Double


            left = (targetSize.Width - popupSize.Width) / 2

            Return {New CustomPopupPlacement(New Point(left, -popupSize.Height), PopupPrimaryAxis.Horizontal)}
        End Function


        Private Sub OnCalendarGotMouseCapture(
                sender As Object,
                e As MouseEventArgs
            )

            Dim element As Windows.UIElement


            element = TryCast(e.OriginalSource, Windows.UIElement)

            If (element IsNot Nothing) Then
                element.ReleaseMouseCapture()
            End If
        End Sub

    End Class

End Namespace
