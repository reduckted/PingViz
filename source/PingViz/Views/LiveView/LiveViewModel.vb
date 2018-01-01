Imports OxyPlot
Imports OxyPlot.Axes
Imports OxyPlot.Series
Imports ReactiveUI
Imports System.Reactive.Concurrency
Imports System.Reactive.Linq


Namespace Views

    Public Class LiveViewModel
        Inherits ViewModelBase


        Private Const TimeoutAxisKey As String = "TimeoutAxis"
        Private Const DurationAxisKey As String = "DurationAxis"


        Private Shared ReadOnly Timeframe As TimeSpan = TimeSpan.FromMinutes(3)
        Private Shared ReadOnly ScrollFrequency As TimeSpan = TimeSpan.FromMilliseconds(500)


        Private ReadOnly cgResultsSource As IPingResultSource
        Private ReadOnly cgHistoryProvider As IHistoryProvider
        Private ReadOnly cgDateTimeProvider As IDateTimeProvider
        Private ReadOnly cgScheduler As IScheduler
        Private ReadOnly cgXAxis As Axis
        Private ReadOnly cgCurrent As ObservableAsPropertyHelper(Of Integer?)
        Private cgSeries As AreaSeries
        Private cgLastResultTimestamp As Date?
        Private cgIsLoading As Boolean


        Public Sub New(
                resultsSource As IPingResultSource,
                historyProvider As IHistoryProvider,
                dateTimeProvider As IDateTimeProvider,
                scheduler As IScheduler
            )

            If resultsSource Is Nothing Then
                Throw New ArgumentNullException(NameOf(resultsSource))
            End If

            If historyProvider Is Nothing Then
                Throw New ArgumentNullException(NameOf(historyProvider))
            End If

            If dateTimeProvider Is Nothing Then
                Throw New ArgumentNullException(NameOf(dateTimeProvider))
            End If

            If scheduler Is Nothing Then
                Throw New ArgumentNullException(NameOf(scheduler))
            End If

            cgResultsSource = resultsSource
            cgHistoryProvider = historyProvider
            cgDateTimeProvider = dateTimeProvider
            cgScheduler = scheduler

            cgIsLoading = True

            ' Hold onto the x-axis because we need to update it as time moves forward.
            ' We don't need to hold on to the y-axis because it doesn't change.
            cgXAxis = CreateXAxis()

            Plot = New PlotModel

            ConfigurePlot()
            Plot.Axes.Add(cgXAxis)
            Plot.Axes.Add(CreateYAxisForDuration())
            Plot.Axes.Add(CreateYAxisForTimeout())

            cgCurrent = resultsSource _
                .Results _
                .Select(Function(x) x.Duration.ToMilliseconds()) _
                .ToProperty(Me, NameOf(Current))

            Use(cgCurrent)
        End Sub


        Private Sub ConfigurePlot()
            Plot.PlotAreaBorderColor = LoadColor("AccentColor")
            Plot.PlotAreaBorderThickness = New OxyThickness(0, 1, 0, 0)
            Plot.Padding = New OxyThickness(0)
        End Sub


        Private Function CreateXAxis() As Axis
            Dim axis As Axis


            axis = New DateTimeAxis With {
                .Position = AxisPosition.Bottom,
                .IsZoomEnabled = False,
                .IsPanEnabled = False,
                .IsAxisVisible = True,
                .TickStyle = TickStyle.None,
                .MinimumPadding = 0,
                .MaximumPadding = 0,
                .TitleColor = OxyColors.Transparent,
                .TextColor = LoadColor("GraphTextColor"),
                .MajorStep = DateTimeAxis.ToDouble(DateTimeAxis.ToDateTime(0).Add(Application.PingInterval)),
                .AxisTickToLabelDistance = 0,
                .LabelFormatter = Function(x) String.Empty
            }

            SetGridLines(axis)
            SetXAxisRange(axis)

            Return axis
        End Function


        Private Shared Function CreateYAxisForDuration() As Axis
            Dim axis As Axis


            axis = New LinearAxis With {
                .Key = DurationAxisKey,
                .Position = AxisPosition.Left,
                .IsZoomEnabled = False,
                .IsPanEnabled = False,
                .TickStyle = TickStyle.None,
                .Minimum = 0,
                .MinimumPadding = 0,
                .AxisTickToLabelDistance = 0,
                .LabelFormatter = Function(x) String.Empty
            }

            SetGridLines(axis)

            Return axis
        End Function


        Private Shared Function CreateYAxisForTimeout() As Axis
            Dim axis As Axis


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

            SetGridLines(axis)

            Return axis
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
                series.Fill = LoadColor("TimeoutFillColor")
                series.Fill = OxyColor.FromUInt32(&H60DB000CUI)
            End If

            Return series
        End Function


        Private Shared Sub SetGridLines(axis As Axis)
            axis.MajorGridlineStyle = LineStyle.Solid
            axis.MajorGridlineColor = LoadColor("Gray9")
        End Sub


        Public Overrides Async Function OnLoadedAsync() As Task
            Dim earliest As Date
            Dim latest As Date


            ' Load any existing data for the period that we will initially show.
            latest = cgDateTimeProvider.GetDateTime().RoundDown(Application.PingInterval)
            earliest = latest.Subtract(Timeframe)

            For Each result In Await cgHistoryProvider.GetPingsAsync(earliest, latest)
                AddDataPoint(result)
            Next result

            If cgLastResultTimestamp.HasValue Then
                Dim nextPossibleTimestamp As Date


                ' Given the timestamp of the last result, work out what 
                ' the earliest timestamp of the next point could be.
                nextPossibleTimestamp = cgLastResultTimestamp.Value.Add(Application.PingInterval)

                ' If there will definitely be a gap between the last point 
                ' we added and the first possible live result, then extend
                ' the last point to where the next point could have been.
                ' If not, then extend the last point to the end of the graph.
                If cgLastResultTimestamp.HasValue AndAlso (nextPossibleTimestamp < latest) Then
                    ExtendLastPoint(nextPossibleTimestamp)
                Else
                    ExtendLastPoint(cgXAxis.Maximum)
                End If
            End If

            ' Now that the graph has been loaded with the
            ' initial data, start watching for new results.
            Use(cgResultsSource.Results.ObserveOn(cgScheduler).Subscribe(AddressOf AddPingResult))

            ' And we can now start scrolling the graph.
            Use(
                Observable.Timer(
                    cgDateTimeProvider.GetDateTime().RoundDown(TimeSpan.FromSeconds(1)),
                    ScrollFrequency,
                    cgScheduler
                ).Subscribe(Sub() UpdateGraphForCurrentTime())
            )

            IsLoading = False
        End Function


        Private Sub AddPingResult(result As PingResult)
            AddDataPoint(result)
            Plot.InvalidatePlot(True)
        End Sub


        Private Sub AddDataPoint(result As PingResult)
            Dim point As DataPoint
            Dim key As String


            ' Work out what type of series the point needs to be put in.
            key = If(result.Duration.HasValue, DurationAxisKey, TimeoutAxisKey)

            If ShouldStartNewSeries(result, key) Then
                ' Because we are ending the current series, we need to 
                ' make sure the point for the last result continues for
                ' the period that the point represents. So if there is 
                ' a last result, extend its point by an interval.
                If cgLastResultTimestamp.HasValue Then
                    ExtendLastPoint(cgLastResultTimestamp.Value.Add(Application.PingInterval))
                End If

                cgSeries = CreateSeries(key)
                Plot.Series.Add(cgSeries)
            End If

            point = ConvertToDataPoint(result)

            If IsLoading Then
                ' The initial results are being loaded, which means 
                ' there isn't an extra point that we need to deal with. 
                ' We can just add the new point to the end of the list.
                cgSeries.Points.Add(point)

            ElseIf cgSeries.Points.Count > 0 Then
                ' We are not loading the initial results, and there are already points
                ' in the series which means there is an extra point at the end of the list.
                ' We can replace that extra point with the new point, then add a new extra
                ' point after it so that the line continues to the edge of the graph.
                cgSeries.Points(cgSeries.Points.Count - 1) = point
                cgSeries.Points.Add(New DataPoint(cgXAxis.Maximum, point.Y))

            Else
                ' We're not loading the initial results, but we are starting a 
                ' new series, so there isn't an extra point already in the series.
                ' We can add the new point to the list, then add an extra point
                ' so that the line continues to the edge of the graph.
                cgSeries.Points.Add(point)
                cgSeries.Points.Add(New DataPoint(cgXAxis.Maximum, point.Y))
            End If

            cgLastResultTimestamp = result.Timestamp
        End Sub


        Private Function ShouldStartNewSeries(
                result As PingResult,
                yAxisKey As String
            ) As Boolean

            ' If there is no current series, then we need to start one.
            If cgSeries Is Nothing Then
                Return True
            End If

            ' If the last result (which which will exist if there's a 
            ' current series) is more than one interval away from the new 
            ' result, then the new result needs to be put in a new series.
            If cgLastResultTimestamp.Value.Add(Application.PingInterval) < result.Timestamp Then
                Return True
            End If

            ' If the key for the y-axis that the result belongs to is different
            ' to that of the current series, then we need to start a new
            ' series. If not, then it can be added to the same series.
            Return Not String.Equals(cgSeries.YAxisKey, yAxisKey)
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
            If cgSeries IsNot Nothing Then
                Dim point As DataPoint


                point = New DataPoint(extendTo, cgSeries.Points(cgSeries.Points.Count - 1).Y)

                If IsLoading Then
                    ' The initial results are being loaded, which means 
                    ' there isn't an extra point in the series. The 
                    ' last point is the one that we need to extend.
                    cgSeries.Points.Add(point)

                Else
                    ' We've finished loading the initial results, which means there
                    ' is an extra point in the series. That extra point (it's the 
                    ' last point) can be replaced, rather than adding a new point.
                    cgSeries.Points(cgSeries.Points.Count - 1) = point
                End If
            End If
        End Sub


        Private Sub UpdateGraphForCurrentTime()
            Dim pinLastPointToMax As Boolean
            Dim lastPoint As DataPoint


            ' Check if the very last point in the graph is pinned to 
            ' the maximum value of the x-axis. If it is, then we will 
            ' need to update that point after we change the x-axis.
            If cgSeries IsNot Nothing Then
                lastPoint = cgSeries.Points(cgSeries.Points.Count - 1)
                pinLastPointToMax = (lastPoint.X = cgXAxis.Maximum)
            End If

            SetXAxisRange(cgXAxis)

            ' Update the last point if it's supposed
            ' to be pinned to the edge of the graph.
            If pinLastPointToMax Then
                cgSeries.Points(cgSeries.Points.Count - 1) = New DataPoint(cgXAxis.Maximum, lastPoint.Y)
            End If

            ' Some points may now be below the minimum
            ' and are no longer needed. Trim them off now.
            If Plot.Series.Count > 0 Then
                TrimRedundantPoints()
            End If

            ' Update the plot.
            Plot.InvalidatePlot(True)
        End Sub


        Private Sub TrimRedundantPoints()
            Do
                Dim series As AreaSeries
                Dim firstPointAboveMin As Integer


                series = DirectCast(Plot.Series(0), AreaSeries)
                firstPointAboveMin = -1

                For i = 0 To series.Points.Count - 1
                    If series.Points(i).X > cgXAxis.Minimum Then
                        firstPointAboveMin = i
                        Exit For
                    End If
                Next i

                If firstPointAboveMin < 0 Then
                    ' There are no points in this series that are above the 
                    ' minimum, which means the entire series won't be visible.
                    ' We can remove this series, as long as it's not the only series.
                    ' If it's the only series, then we can leave it and stop trimming.
                    If Plot.Series.Count > 1 Then
                        Plot.Series.RemoveAt(0)
                    Else
                        Exit Do
                    End If

                ElseIf firstPointAboveMin = 0 Then
                    ' The first point above the minimum is the first point,
                    ' which means every point is visible. We don't need to
                    ' trim anything from this series, and since the points
                    ' across all series will be ascending, we don't need to
                    ' look at any more series.
                    Exit Do

                Else
                    ' We need to keep the point immediately _before_ the first point
                    ' above the minimum. This ensures that there will be a line coming
                    ' from the left edge of the graph to that point above the minimum.
                    ' Everything else before it can be removed.
                    series.Points.RemoveRange(0, firstPointAboveMin - 1)

                    ' Because we found a point that
                    ' we kept, we can stop trimming.
                    Exit Do
                End If
            Loop
        End Sub


        Private Shared Sub TrimSeries(
                series As AreaSeries,
                minimum As Double
            )

            Dim firstVisibleIndex As Integer = -1


            ' Find the index of the first point
            ' that will be visible in the graph.
            For i = 0 To series.Points.Count - 1
                If series.Points(i).X >= minimum Then
                    firstVisibleIndex = i
                    Exit For
                End If
            Next i

            If firstVisibleIndex < 0 Then
                ' No points are visible, so they can all be removed.
                series.Points.Clear()

            Else
                ' We need to keep one point _before_ the first visible
                ' point. If we remove the last invisible point, then we also
                ' remove the line that joins it to the first visible point.
                If firstVisibleIndex > 1 Then
                    series.Points.RemoveRange(0, firstVisibleIndex - 1)
                End If
            End If
        End Sub


        Private Sub SetXAxisRange(axis As Axis)
            Dim now As Date


            now = cgDateTimeProvider.GetDateTime().RoundDown(ScrollFrequency)

            axis.Minimum = DateTimeAxis.ToDouble(now.Subtract(Timeframe))
            axis.Maximum = DateTimeAxis.ToDouble(now)
        End Sub


        Public ReadOnly Property Title As String
            Get
                Return "Live"
            End Get
        End Property


        Public ReadOnly Property Plot As PlotModel


        Public Property IsLoading As Boolean
            Get
                Return cgIsLoading
            End Get

            Private Set
                RaiseAndSetIfChanged(cgIsLoading, Value)
            End Set
        End Property


        Public ReadOnly Property Current As Integer?
            Get
                Return cgCurrent.Value
            End Get
        End Property


        Private Shared Function LoadColor(name As String) As OxyColor
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

    End Class

End Namespace
