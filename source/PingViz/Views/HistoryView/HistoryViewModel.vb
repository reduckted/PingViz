Imports OxyPlot
Imports OxyPlot.Annotations
Imports OxyPlot.Axes
Imports ReactiveUI
Imports System.Reactive.Concurrency
Imports System.Reactive.Linq


Namespace Views

    Public Class HistoryViewModel
        Inherits PlotViewModelBase


        Private ReadOnly cgHistoryProvider As IHistoryProvider
        Private ReadOnly cgDateTimeProvider As IDateTimeProvider
        Private ReadOnly cgErrorHandler As IErrorHandler
        Private cgStepSize As TimeSpan
        Private cgSelectedDate As Date
        Private cgUpdatingSelectedDate As Boolean


        Public Sub New(
                scheduler As IScheduler,
                historyProvider As IHistoryProvider,
                dateTimeProvider As IDateTimeProvider,
                resultsSource As IPingResultSource,
                errorHandler As IErrorHandler
            )

            MyBase.New(scheduler)

            Dim today As Date


            If historyProvider Is Nothing Then
                Throw New ArgumentNullException(NameOf(historyProvider))
            End If

            If dateTimeProvider Is Nothing Then
                Throw New ArgumentNullException(NameOf(dateTimeProvider))
            End If

            If resultsSource Is Nothing Then
                Throw New ArgumentNullException(NameOf(resultsSource))
            End If

            If errorHandler Is Nothing Then
                Throw New ArgumentNullException(NameOf(errorHandler))
            End If

            cgHistoryProvider = historyProvider
            cgDateTimeProvider = dateTimeProvider
            cgErrorHandler = errorHandler

            MoveBackCommand = ReactiveCommand.Create(AddressOf MoveBackAsync, outputScheduler:=scheduler)
            MoveForwardCommand = ReactiveCommand.Create(AddressOf MoveForwardAsync, outputScheduler:=scheduler)

            Plot.PlotAreaBorderThickness = New OxyThickness(0, 1, 0, 1)

            ConfigureTimeAxis()
            today = cgDateTimeProvider.GetDateTime().Date
            TimeAxis.Minimum = DateTimeAxis.ToDouble(today)
            TimeAxis.Maximum = DateTimeAxis.ToDouble(today.AddDays(1))
            cgSelectedDate = today

            Use(resultsSource.Results.ObserveOn(scheduler).Subscribe(AddressOf OnPingResult))

            ' Listen for changes to the time axis, but throttle the changes 
            ' so that we don't react any more than every half a second.
            Use(
                Observable _
                    .FromEventPattern(Of AxisChangedEventArgs)(TimeAxis, NameOf(TimeAxis.AxisChanged), scheduler) _
                    .SubscribeOn(scheduler) _
                    .ThrottleInWindow(TimeSpan.FromSeconds(0.5), scheduler) _
                    .Subscribe(Sub(x) OnTimeAxisChangedAsync(x.EventArgs))
            )
        End Sub


        Private Sub ConfigureTimeAxis()
            TimeAxis.IsZoomEnabled = True
            TimeAxis.IsPanEnabled = True
        End Sub


        Protected Overrides Async Function LoadAsync() As Task
            ' We need to recalculate the step size now because this is the 
            ' first opportunity we have where the actual min and max of the time 
            ' axis have been set. Those properties are only set once the view 
            ' triggers an update, which means we can't do it any earlier.
            RecalculateStepSize()
            Await PopulatePlotAsync()
        End Function


        Public ReadOnly Property Title As String
            Get
                Return "History"
            End Get
        End Property


        Public Property SelectedDate As Date
            Get
                Return cgSelectedDate
            End Get

            Set
                Dim changing As Boolean


                Value = Value.Date
                changing = Not Date.Equals(cgSelectedDate, Value)

                RaiseAndSetIfChanged(cgSelectedDate, Value)

                If changing AndAlso (Not cgUpdatingSelectedDate) Then
                    MoveTimeAxis(cgSelectedDate.Date.Subtract(GetViewStartDateTime().Date))
                End If
            End Set
        End Property


        Public ReadOnly Property MoveBackCommand As ICommand


        Public ReadOnly Property MoveForwardCommand As ICommand



        Private Async Sub OnTimeAxisChangedAsync(e As AxisChangedEventArgs)
            ' Update the range of the axis to what is actually being shown.
            TimeAxis.Minimum = TimeAxis.ActualMinimum
            TimeAxis.Maximum = TimeAxis.ActualMaximum

            SetSelectedDateFromTimeAxis()
            RecalculateStepSize()
            Await PopulatePlotAsync()
        End Sub


        Private Sub RecalculateStepSize()
            Select Case GetViewEndDateTime().Subtract(GetViewStartDateTime()).TotalHours
                Case > 24
                    cgStepSize = TimeSpan.FromMinutes(30)

                Case >= 12
                    cgStepSize = TimeSpan.FromMinutes(15)

                Case >= 6
                    cgStepSize = TimeSpan.FromMinutes(10)

                Case >= 3
                    cgStepSize = TimeSpan.FromMinutes(5)

                Case >= 1
                    cgStepSize = TimeSpan.FromMinutes(2)

                Case >= 0.5
                    cgStepSize = TimeSpan.FromMinutes(1)

                Case Else
                    cgStepSize = Application.PingInterval

            End Select
        End Sub


        Private Async Function PopulatePlotAsync() As Task
            SetIsLoading(True)

            Try
                Dim results As IEnumerable(Of PingResult)


                results = GroupResults(Await cgHistoryProvider.GetPingsAsync(GetViewStartDateTime(), GetViewEndDateTime()))

                Plot.Annotations.Clear()

                AddDataPoints(results, cgStepSize)
                AddAnnotations()

                Plot.InvalidatePlot(True)

            Catch ex As Exception
                cgErrorHandler.Handle($"Failed to load pings: {ex.Message}")

            Finally
                SetIsLoading(False)
            End Try
        End Function


        Private Function GroupResults(results As IEnumerable(Of PingResult)) As IEnumerable(Of PingResult)
            Return results _
                .GroupBy(Function(x) x.Timestamp.RoundDown(cgStepSize)) _
                .Select(
                    Function(group)
                        If group.Any(Function(x) Not x.Duration.HasValue) Then
                            Return New PingResult With {.Timestamp = group.Key, .Duration = Nothing}
                        Else
                            Return New PingResult With {.Timestamp = group.Key, .Duration = group.Max(Function(x) x.Duration)}
                        End If
                    End Function
                ).ToList()
        End Function


        Private Sub AddAnnotations()
            Dim labelTextColor As OxyColor
            Dim fontFamily As String


            labelTextColor = LoadColor("Gray5")
            fontFamily = LoadFont("DefaultFont")

            Plot.Annotations.Add(
                New TimeAxisLabelAnnotation With {
                    .TextColor = labelTextColor,
                    .Font = fontFamily,
                    .Layer = AnnotationLayer.AboveSeries
                }
            )

            Plot.Annotations.Add(
                New DurationAxisLabelAnnotation With {
                    .YAxisKey = DurationAxis.Key,
                    .TextColor = labelTextColor,
                    .Font = fontFamily,
                    .Layer = AnnotationLayer.AboveSeries
                }
            )

            Plot.Annotations.Add(
                New DayBoundsAnnotation With {
                    .YAxisKey = TimeoutAxis.Key,
                    .TextColor = LoadColor("Gray6"),
                    .Font = fontFamily,
                    .Layer = AnnotationLayer.AboveSeries,
                    .BoundaryColor = LoadColor("Gray9"),
                    .AlternateFillColor = OxyColor.FromArgb(3, 0, 0, 0)
                }
            )
        End Sub


        Private Async Sub OnPingResult(result As PingResult)
            ' As long as the result is within the visible window, we can show 
            ' it in the graph. But, we can't just add a new point for the 
            ' result, because it needs To be grouped according to the step 
            ' size, so we'll need to re-populate the whole chart instead.
            If result.Timestamp >= GetViewStartDateTime() Then
                If result.Timestamp <= GetViewEndDateTime() Then
                    Await PopulatePlotAsync()
                End If
            End If
        End Sub


        Private Sub MoveBackAsync()
            MoveTimeAxis(TimeSpan.FromDays(-1))
        End Sub


        Private Sub MoveForwardAsync()
            MoveTimeAxis(TimeSpan.FromDays(1))
        End Sub


        Private Async Sub MoveTimeAxis(offset As TimeSpan)
            TimeAxis.Minimum = DateTimeAxis.ToDouble(GetViewStartDateTime().Add(offset))
            TimeAxis.Maximum = DateTimeAxis.ToDouble(GetViewEndDateTime().Add(offset))

            ' Reset the axis so that the actual minimum and 
            ' maximum reflect the new minimum and maximum.
            TimeAxis.Reset()

            SetSelectedDateFromTimeAxis()

            Await PopulatePlotAsync()
        End Sub


        Private Function GetViewStartDateTime() As Date
            Return DateTimeAxis.ToDateTime(TimeAxis.ActualMinimum)
        End Function


        Private Function GetViewEndDateTime() As Date
            Return DateTimeAxis.ToDateTime(TimeAxis.ActualMaximum)
        End Function


        Private Sub SetSelectedDateFromTimeAxis()
            cgUpdatingSelectedDate = True
            SelectedDate = GetViewStartDateTime().Date
            cgUpdatingSelectedDate = False
        End Sub

    End Class

End Namespace
