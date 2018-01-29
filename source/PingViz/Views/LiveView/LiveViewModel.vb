Imports OxyPlot
Imports OxyPlot.Axes
Imports OxyPlot.Series
Imports ReactiveUI
Imports System.Reactive.Concurrency
Imports System.Reactive.Linq


Namespace Views

    Public Class LiveViewModel
        Inherits PlotViewModelBase


        Private Shared ReadOnly Timeframe As TimeSpan = TimeSpan.FromMinutes(3)
        Private Shared ReadOnly ScrollFrequency As TimeSpan = TimeSpan.FromMilliseconds(500)


        Private ReadOnly cgResultsSource As IPingResultSource
        Private ReadOnly cgHistoryProvider As IHistoryProvider
        Private ReadOnly cgDateTimeProvider As IDateTimeProvider
        Private ReadOnly cgErrorHandler As IErrorHandler
        Private ReadOnly cgCurrent As ObservableAsPropertyHelper(Of Integer?)


        Public Sub New(
                scheduler As IScheduler,
                resultsSource As IPingResultSource,
                historyProvider As IHistoryProvider,
                dateTimeProvider As IDateTimeProvider,
                errorHandler As IErrorHandler
            )

            MyBase.New(scheduler)

            If resultsSource Is Nothing Then
                Throw New ArgumentNullException(NameOf(resultsSource))
            End If

            If historyProvider Is Nothing Then
                Throw New ArgumentNullException(NameOf(historyProvider))
            End If

            If dateTimeProvider Is Nothing Then
                Throw New ArgumentNullException(NameOf(dateTimeProvider))
            End If

            If errorHandler Is Nothing Then
                Throw New ArgumentNullException(NameOf(errorHandler))
            End If

            cgResultsSource = resultsSource
            cgHistoryProvider = historyProvider
            cgDateTimeProvider = dateTimeProvider
            cgErrorHandler = errorHandler

            ConfigureTimeAxis()

            cgCurrent = resultsSource _
                .Results _
                .Select(Function(x) x.Duration.ToMilliseconds()) _
                .ToProperty(Me, NameOf(Current))

            Use(cgCurrent)
        End Sub


        Private Sub ConfigureTimeAxis()
            TimeAxis.MajorStep = DateTimeAxis.ToDouble(DateTimeAxis.ToDateTime(0).Add(Application.PingInterval))

            SetTimeAxisRange()
        End Sub


        Protected Overrides Async Function LoadAsync() As Task
            Dim earliest As Date
            Dim latest As Date


            ' Load any existing data for the period that we will initially show.
            latest = DateTimeAxis.ToDateTime(TimeAxis.Maximum).RoundDown(Application.PingInterval)
            earliest = latest.Subtract(Timeframe)

            Try
                AddDataPoints(
                    Await cgHistoryProvider.GetPingsAsync(earliest, latest),
                    Application.PingInterval
                )

            Catch ex As Exception
                cgErrorHandler.Handle($"Failed to load previous pings: {ex.Message}")
            End Try

            ' Now that the graph has been loaded with the
            ' initial data, start watching for new results.
            Use(cgResultsSource.Results.ObserveOn(Scheduler).Subscribe(AddressOf AddPingResult))

            ' And we can now start scrolling the graph.
            Use(
                Observable.Timer(
                    cgDateTimeProvider.GetDateTime().RoundDown(TimeSpan.FromSeconds(1)),
                    ScrollFrequency,
                    Scheduler
                ).Subscribe(Sub() UpdateGraphForCurrentTime())
            )

            Plot.InvalidatePlot(True)
        End Function


        Private Sub AddPingResult(result As PingResult)
            AddDataPoint(result, Application.PingInterval)
            Plot.InvalidatePlot(True)
        End Sub


        Private Sub UpdateGraphForCurrentTime()
            Dim pinLastPointToMax As Boolean
            Dim lastPoint As DataPoint


            ' Check if the very last point in the graph is pinned to 
            ' the maximum value of the x-axis. If it is, then we will 
            ' need to update that point after we change the x-axis.
            If CurrentSeries IsNot Nothing Then
                lastPoint = CurrentSeries.Points(CurrentSeries.Points.Count - 1)
                pinLastPointToMax = (lastPoint.X = TimeAxis.Maximum)
            End If

            SetTimeAxisRange()

            ' Update the last point if it's supposed
            ' to be pinned to the edge of the graph.
            If pinLastPointToMax Then
                CurrentSeries.Points(CurrentSeries.Points.Count - 1) = New DataPoint(TimeAxis.Maximum, lastPoint.Y)
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
                    If series.Points(i).X > TimeAxis.Minimum Then
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


        Private Sub SetTimeAxisRange()
            Dim now As Date


            now = cgDateTimeProvider.GetDateTime().RoundDown(ScrollFrequency)

            TimeAxis.Minimum = DateTimeAxis.ToDouble(now.Subtract(Timeframe))
            TimeAxis.Maximum = DateTimeAxis.ToDouble(now)
        End Sub


        Public ReadOnly Property Title As String
            Get
                Return "Live"
            End Get
        End Property


        Public ReadOnly Property Current As Integer?
            Get
                Return cgCurrent.Value
            End Get
        End Property

    End Class

End Namespace
