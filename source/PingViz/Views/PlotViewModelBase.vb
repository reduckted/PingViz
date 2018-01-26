Imports OxyPlot
Imports OxyPlot.Axes
Imports OxyPlot.Series
Imports System.Reactive.Concurrency


Namespace Views

    Public Class PlotViewModelBase
        Inherits ViewModelBase


        Public Const TimeoutAxisKey As String = "TimeoutAxis"
        Public Const DurationAxisKey As String = "DurationAxis"


        Private cgCurrentSeries As AreaSeries
        Private cgLastResultTimestamp As Date?


        Public Sub New(scheduler As IScheduler)
            MyBase.New(scheduler)

            Plot = New PlotModel With {
                .PlotAreaBorderColor = LoadColor("AccentColor"),
                .PlotAreaBorderThickness = New OxyThickness(0, 1, 0, 0),
                .Padding = New OxyThickness(0)
            }

            TimeAxis = CreateTimeAxis()
            DurationAxis = CreateDurationAxis()
            TimeoutAxis = CreateTimeoutAxis()

            Plot.Axes.Add(TimeAxis)
            Plot.Axes.Add(DurationAxis)
            Plot.Axes.Add(TimeoutAxis)
        End Sub


        Private Shared Function CreateTimeAxis() As DateTimeAxis
            Dim axis As DateTimeAxis


            axis = New DateTimeAxis With {
                .Position = AxisPosition.Bottom,
                .IsZoomEnabled = False,
                .IsPanEnabled = False,
                .MinimumPadding = 0,
                .MaximumPadding = 0,
                .AxisTickToLabelDistance = 0,
                .LabelFormatter = Function(x) String.Empty,
                .TickStyle = TickStyle.None,
                .TextColor = LoadColor("Gray5")
            }

            SetGridLines(axis)

            Return axis
        End Function


        Private Shared Function CreateDurationAxis() As LinearAxis
            Dim axis As LinearAxis


            axis = New LinearAxis With {
                .Key = DurationAxisKey,
                .Position = AxisPosition.Left,
                .IsZoomEnabled = False,
                .IsPanEnabled = False,
                .Minimum = 0,
                .MinimumPadding = 0,
                .MaximumPadding = 0.1,
                .AxisTickToLabelDistance = 0,
                .LabelFormatter = Function(x) String.Empty,
                .TickStyle = TickStyle.None,
                .TextColor = LoadColor("Gray5")
            }

            SetGridLines(axis)

            Return axis
        End Function


        Public Shared Function CreateTimeoutAxis() As Axis
            Dim axis As LinearAxis


            axis = New LinearAxis With {
                .Key = TimeoutAxisKey,
                .Position = AxisPosition.Left,
                .IsZoomEnabled = False,
                .IsPanEnabled = False,
                .IsAxisVisible = False,
                .Minimum = 0,
                .Maximum = 1,
                .MinimumPadding = 0,
                .MaximumPadding = 0
            }

            Return axis
        End Function


        Private Shared Sub SetGridLines(axis As Axis)
            axis.MajorGridlineStyle = LineStyle.Solid
            axis.MajorGridlineColor = LoadColor("Gray10")
        End Sub


        Protected Sub AddDataPoints(
                results As IEnumerable(Of PingResult),
                stepSize As TimeSpan
            )

            Plot.Series.Clear()
            cgCurrentSeries = Nothing
            cgLastResultTimestamp = Nothing

            For Each result In results
                AddDataPoint(result, stepSize)
            Next result

            If cgLastResultTimestamp.HasValue Then
                Dim nextPossibleTimestamp As Date
                Dim latestPointInRange As Date


                latestPointInRange = DateTimeAxis.ToDateTime(TimeAxis.Maximum).RoundDown(stepSize)

                ' Given the timestamp of the last result, work out what 
                ' the earliest timestamp of the next point could be.
                nextPossibleTimestamp = cgLastResultTimestamp.Value.Add(stepSize)

                ' If there will definitely be a gap between the last point 
                ' we added and the first possible live result, then extend
                ' the last point to where the next point could have been.
                ' If not, then extend the last point to the end of the graph.
                If cgLastResultTimestamp.HasValue AndAlso (nextPossibleTimestamp < latestPointInRange) Then
                    ExtendLastPoint(nextPossibleTimestamp)
                Else
                    ExtendLastPoint(TimeAxis.Maximum)
                End If
            End If
        End Sub


        Protected Sub AddDataPoint(
                result As PingResult,
                stepSize As TimeSpan
            )

            Dim point As DataPoint
            Dim key As String


            ' Work out what type of series the point needs to be put in.
            key = If(result.Duration.HasValue, DurationAxisKey, TimeoutAxisKey)

            If ShouldStartNewSeries(result, key, stepSize) Then
                ' Because we are ending the current series, we need to 
                ' make sure the point for the last result continues for
                ' the period that the point represents. So if there is 
                ' a last result, extend its point by an interval.
                If cgLastResultTimestamp.HasValue Then
                    ExtendLastPoint(cgLastResultTimestamp.Value.Add(Application.PingInterval))
                End If

                cgCurrentSeries = CreateSeries(key)
                Plot.Series.Add(cgCurrentSeries)
            End If

            point = ConvertToDataPoint(result)

            If IsLoading Then
                ' The initial results are being loaded, which means 
                ' there isn't an extra point that we need to deal with. 
                ' We can just add the new point to the end of the list.
                cgCurrentSeries.Points.Add(point)

            ElseIf cgCurrentSeries.Points.Count > 0 Then
                ' We are not loading the initial results, and there are already points
                ' in the series which means there is an extra point at the end of the list.
                ' We can replace that extra point with the new point, then add a new extra
                ' point after it so that the line continues to the edge of the graph.
                cgCurrentSeries.Points(cgCurrentSeries.Points.Count - 1) = point
                cgCurrentSeries.Points.Add(New DataPoint(TimeAxis.Maximum, point.Y))

            Else
                ' We're not loading the initial results, but we are starting a 
                ' new series, so there isn't an extra point already in the series.
                ' We can add the new point to the list, then add an extra point
                ' so that the line continues to the edge of the graph.
                cgCurrentSeries.Points.Add(point)
                cgCurrentSeries.Points.Add(New DataPoint(TimeAxis.Maximum, point.Y))
            End If

            cgLastResultTimestamp = result.Timestamp
        End Sub


        Private Function ShouldStartNewSeries(
                result As PingResult,
                yAxisKey As String,
                stepSize As TimeSpan
            ) As Boolean

            ' If there is no current series, then we need to start one.
            If cgCurrentSeries Is Nothing Then
                Return True
            End If

            ' If the last result (which which will exist if there's a 
            ' current series) is more than one interval away from the new 
            ' result, then the new result needs to be put in a new series.
            If cgLastResultTimestamp.Value.Add(stepSize) < result.Timestamp Then
                Return True
            End If

            ' If the key for the y-axis that the result belongs to is different
            ' to that of the current series, then we need to start a new
            ' series. If not, then it can be added to the same series.
            Return Not String.Equals(cgCurrentSeries.YAxisKey, yAxisKey)
        End Function


        Private Shared Function CreateSeries(key As String) As AreaSeries
            Dim series As AreaSeries


            series = New AreaSeries With {.YAxisKey = key}

            If String.Equals(key, DurationAxisKey) Then
                series.StrokeThickness = 1
                series.Fill = OxyColor.FromAColor(16, LoadColor("AccentColor"))
                series.Color = LoadColor("AccentColor")

            Else
                series.StrokeThickness = 0
                series.Fill = OxyColor.FromUInt32(&H60DB000CUI)
            End If

            Return series
        End Function


        Private Shared Function ConvertToDataPoint(result As PingResult) As DataPoint
            Dim value As Double


            value = result.Duration.ToMilliseconds().GetValueOrDefault(1)

            Return DateTimeAxis.CreateDataPoint(result.Timestamp, value)
        End Function


        Private Sub ExtendLastPoint(extendTo As Date)
            ExtendLastPoint(DateTimeAxis.ToDouble(extendTo))
        End Sub


        Private Sub ExtendLastPoint(extendTo As Double)
            If cgCurrentSeries IsNot Nothing Then
                Dim point As DataPoint


                point = New DataPoint(extendTo, cgCurrentSeries.Points(cgCurrentSeries.Points.Count - 1).Y)

                If IsLoading Then
                    ' The initial results are being loaded, which means 
                    ' there isn't an extra point in the series. The 
                    ' last point is the one that we need to extend.
                    cgCurrentSeries.Points.Add(point)

                Else
                    ' We've finished loading the initial results, which means there
                    ' is an extra point in the series. That extra point (it's the 
                    ' last point) can be replaced, rather than adding a new point.
                    cgCurrentSeries.Points(cgCurrentSeries.Points.Count - 1) = point
                End If
            End If
        End Sub


        Public ReadOnly Property Plot As PlotModel


        Protected ReadOnly Property TimeAxis As DateTimeAxis


        Protected ReadOnly Property DurationAxis As Axis


        Protected ReadOnly Property TimeoutAxis As Axis


        Protected ReadOnly Property CurrentSeries As AreaSeries
            Get
                Return cgCurrentSeries
            End Get
        End Property


        Protected Shared Function LoadColor(name As String) As OxyColor
            Dim app As Windows.Application


            ' The application and its resources probably
            ' aren't available while running unit tests.
            app = Windows.Application.Current

            If app IsNot Nothing Then
                Dim obj As Object


                obj = app.TryFindResource(name)

                If obj IsNot Nothing Then
                    Dim color As Color


                    color = DirectCast(obj, Color)

                    Return OxyColor.FromArgb(color.A, color.R, color.G, color.B)
                End If
            End If

            Return Nothing
        End Function


        Protected Shared Function LoadFont(name As String) As String
            Dim app As Windows.Application


            ' The application and its resources probably
            ' aren't available while running unit tests.
            app = Windows.Application.Current

            If app IsNot Nothing Then
                Dim obj As Object


                obj = app.TryFindResource(name)

                If obj IsNot Nothing Then
                    Dim ff As FontFamily


                    ff = DirectCast(obj, FontFamily)

                    Return ff.FamilyNames.First().Value
                End If
            End If

            Return Nothing
        End Function

    End Class

End Namespace
