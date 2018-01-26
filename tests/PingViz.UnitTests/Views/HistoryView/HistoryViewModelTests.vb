Imports Microsoft.Reactive.Testing
Imports OxyPlot
Imports OxyPlot.Axes
Imports OxyPlot.Series
Imports System.Reactive.Linq
Imports System.Reactive.Subjects

Namespace Views

    Public Class HistoryViewModelTests

        Public Class PlotProperty
            Inherits TestBase


            <Fact()>
            Public Async Function InitiallyShowsTheCurrentDay() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync({})

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())

                    Assert.NotNull(axis)
                    Assert.Equal(#2001-04-05#, DateTimeAxis.ToDateTime(axis.Minimum))
                    Assert.Equal(#2001-04-06#, DateTimeAxis.ToDateTime(axis.Maximum))

                    Await vm.OnLoadedAsync()

                    historyProvider.Verify(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#), Times.Once)
                End Using
            End Function


            <Theory()>
            <InlineData(48, 30)>
            <InlineData(24, 15)>
            <InlineData(12, 15)>
            <InlineData(6, 10)>
            <InlineData(3, 5)>
            <InlineData(2, 2)>
            <InlineData(1, 2)>
            <InlineData(0.5, 1)>
            Public Async Function GroupsResults(
                    visibleHours As Double,
                    stepSize As Integer
                ) As Task

                Dim historyProvider As Mock(Of IHistoryProvider)
                Dim pings As List(Of PingResult)
                Dim expected As List(Of Date)


                pings = New List(Of PingResult)
                expected = New List(Of Date)

                For i = 0 To CInt(visibleHours * 60)
                    pings.Add(New PingResult With {.Timestamp = #2001-04-05#.AddMinutes(i), .Duration = TimeSpan.FromMilliseconds(i)})
                Next i

                For i = 0 To CInt(visibleHours * 60) Step stepSize
                    expected.Add(#2001-04-05#.AddMinutes(i))
                Next i

                ' There's always an extra point added at the end to ensure that 
                ' the last series continues to the end of the viewable range.
                expected.Add(#2001-04-05#.AddHours(visibleHours))

                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(pings)

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis
                    Dim endDateTime As Date
                    Dim points As List(Of DataPoint)


                    endDateTime = #2001-04-05#.AddHours(visibleHours)

                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    ' Zoom in to the specific number of hours. This an async event, but since
                    ' we return the results synchronously, we should be OK to continue without
                    ' waiting for anything (because we have nothing to wait for).
                    axis.Zoom(DateTimeAxis.ToDouble(#2001-04-05#), DateTimeAxis.ToDouble(endDateTime))
                    Scheduler.AdvanceBy(1)

                    ' Get the points on the graph and confirm that first point has the highest value from the results.
                    points = DirectCast(vm.Plot.Series(0), AreaSeries).Points
                    Assert.Equal(expected, points.Select(Function(x) DateTimeAxis.ToDateTime(x.X)))
                End Using
            End Function


            <Fact()>
            Public Async Function ShowsTimeoutWhenTimeoutOccurredInStepSize() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)
                Dim pings As List(Of PingResult)


                pings = New List(Of PingResult) From {
                    New PingResult With {.Timestamp = #2001-04-05 00:00:00#, .Duration = TimeSpan.FromMilliseconds(10)},
                    New PingResult With {.Timestamp = #2001-04-05 00:00:10#, .Duration = TimeSpan.FromMilliseconds(30)},
                    New PingResult With {.Timestamp = #2001-04-05 00:00:20#, .Duration = Nothing},
                    New PingResult With {.Timestamp = #2001-04-05 00:00:30#, .Duration = TimeSpan.FromMilliseconds(50)},
                    New PingResult With {.Timestamp = #2001-04-05 00:00:40#, .Duration = TimeSpan.FromMilliseconds(5)},
                    New PingResult With {.Timestamp = #2001-04-05 00:00:50#, .Duration = TimeSpan.FromMilliseconds(15)},
                    New PingResult With {.Timestamp = #2001-04-05 00:01:00#, .Duration = TimeSpan.FromMilliseconds(9)}
                }

                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(pings)

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    ' Confirm that the series belongs to the timeout axis.
                    Assert.Equal(PlotViewModelBase.TimeoutAxisKey, DirectCast(vm.Plot.Series(0), AreaSeries).YAxisKey)

                    ' Confirm that the first point shows a timeout.
                    Assert.Equal(1, DirectCast(vm.Plot.Series(0), AreaSeries).Points(0).Y)
                End Using
            End Function


            <Fact()>
            Public Async Function ShowsLargestDurationThatOccurredInStepSize() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)
                Dim pings As List(Of PingResult)


                pings = New List(Of PingResult) From {
                    New PingResult With {.Timestamp = #2001-04-05 00:00:00#, .Duration = TimeSpan.FromMilliseconds(10)},
                    New PingResult With {.Timestamp = #2001-04-05 00:00:10#, .Duration = TimeSpan.FromMilliseconds(30)},
                    New PingResult With {.Timestamp = #2001-04-05 00:00:20#, .Duration = TimeSpan.FromMilliseconds(20)},
                    New PingResult With {.Timestamp = #2001-04-05 00:00:30#, .Duration = TimeSpan.FromMilliseconds(50)},
                    New PingResult With {.Timestamp = #2001-04-05 00:00:40#, .Duration = TimeSpan.FromMilliseconds(5)},
                    New PingResult With {.Timestamp = #2001-04-05 00:00:50#, .Duration = TimeSpan.FromMilliseconds(15)},
                    New PingResult With {.Timestamp = #2001-04-05 00:01:00#, .Duration = TimeSpan.FromMilliseconds(9)}
                }

                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(pings)

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    ' Confirm that the series belongs to the duration axis.
                    Assert.Equal(PlotViewModelBase.DurationAxisKey, DirectCast(vm.Plot.Series(0), AreaSeries).YAxisKey)

                    ' Get the points on the graph.
                    Assert.Equal(50, DirectCast(vm.Plot.Series(0), AreaSeries).Points(0).Y)
                End Using
            End Function


            <Fact()>
            Public Async Function HasAnnotationsForAxes() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync({})

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Await vm.OnLoadedAsync()

                    Assert.Equal(3, vm.Plot.Annotations.Count)
                    Assert.Contains(vm.Plot.Annotations, Function(x) TypeOf x Is DayBoundsAnnotation)
                    Assert.Contains(vm.Plot.Annotations, Function(x) TypeOf x Is TimeAxisLabelAnnotation)
                    Assert.Contains(vm.Plot.Annotations, Function(x) TypeOf x Is DurationAxisLabelAnnotation)
                End Using
            End Function


            <Fact()>
            Public Async Function StartsNewSeriesWhenGapInData() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)
                Dim pings As List(Of PingResult)


                pings = New List(Of PingResult) From {
                    New PingResult With {.Timestamp = #2001-04-05 00:00:00#, .Duration = TimeSpan.FromMilliseconds(10)},
                    New PingResult With {.Timestamp = #2001-04-05 01:00:00#, .Duration = TimeSpan.FromMilliseconds(30)}
                }

                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(pings)

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())

                    Assert.NotNull(axis)
                    Assert.Equal(#2001-04-05#, DateTimeAxis.ToDateTime(axis.Minimum))
                    Assert.Equal(#2001-04-06#, DateTimeAxis.ToDateTime(axis.Maximum))

                    Await vm.OnLoadedAsync()

                    historyProvider.Verify(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#), Times.Once)

                    Assert.Equal(2, vm.Plot.Series.Count)
                    Assert.Equal(#2001-04-05 00:00:00#, DateTimeAxis.ToDateTime(DirectCast(vm.Plot.Series(0), AreaSeries).Points(0).X))
                    Assert.Equal(#2001-04-05 01:00:00#, DateTimeAxis.ToDateTime(DirectCast(vm.Plot.Series(1), AreaSeries).Points(0).X))
                End Using
            End Function


            <Fact()>
            Public Async Function RepopualtesWhenTimeAxisChanges() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-05#, .Duration = TimeSpan.FromMilliseconds(10)}}
                )

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-06#, #2001-04-07#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-06#, .Duration = TimeSpan.FromMilliseconds(20)}}
                )

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    Assert.Equal(DateTimeAxis.CreateDataPoint(#2001-04-05#, 10), DirectCast(vm.Plot.Series(0), AreaSeries).Points(0))

                    ' Zoom to the new time range and confirm that the graph shows the new data.
                    axis.Zoom(DateTimeAxis.ToDouble(#2001-04-06#), DateTimeAxis.ToDouble(#2001-04-07#))
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(DateTimeAxis.CreateDataPoint(#2001-04-06#, 20), DirectCast(vm.Plot.Series(0), AreaSeries).Points(0))
                End Using
            End Function


            <Fact()>
            Public Async Function ThrottlesChangesToAxis() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)

                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync({})

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-05#, .Duration = TimeSpan.FromMilliseconds(10)}}
                )

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-05 12:00#, #2001-04-06 12:00#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-05 12:00#, .Duration = TimeSpan.FromMilliseconds(15)}}
                )

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-06#, #2001-04-07#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-06#, .Duration = TimeSpan.FromMilliseconds(20)}}
                )

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    Assert.Equal(
                        DateTimeAxis.CreateDataPoint(#2001-04-05#, 10),
                        DirectCast(vm.Plot.Series(0), AreaSeries).Points(0)
                    )

                    ' Zoom to a different range and move the scheduler forward 
                    ' because the first change should trigger immediately.
                    axis.Zoom(DateTimeAxis.ToDouble(#2001-04-05 12:00#), DateTimeAxis.ToDouble(#2001-04-06 12:00#))
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(
                        DateTimeAxis.CreateDataPoint(#2001-04-05 12:00#, 15),
                        DirectCast(vm.Plot.Series(0), AreaSeries).Points(0)
                    )

                    ' Now zoom to another range. Move the scheduler forward by 
                    ' a little bit and confirm that the data has not changed.
                    axis.Zoom(DateTimeAxis.ToDouble(#2001-04-06#), DateTimeAxis.ToDouble(#2001-04-07#))
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(
                        DateTimeAxis.CreateDataPoint(#2001-04-05 12:00#, 15),
                        DirectCast(vm.Plot.Series(0), AreaSeries).Points(0)
                    )

                    ' Now move the scheduler forward buy half a second
                    ' and confirm that the data has been updated.
                    Scheduler.AdvanceBy(TimeSpan.FromSeconds(0.5).Ticks)

                    Assert.Equal(
                        DateTimeAxis.CreateDataPoint(#2001-04-06#, 20),
                        DirectCast(vm.Plot.Series(0), AreaSeries).Points(0)
                    )
                End Using
            End Function


            <Fact()>
            Public Async Function AddsNewResultsWhenResultIsInVisibleWindow() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)
                Dim results As Subject(Of PingResult)


                results = New Subject(Of PingResult)

                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync({})

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object, results:=results)
                    Await vm.OnLoadedAsync()

                    historyProvider.Verify(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#), Times.Once)

                    results.OnNext(New PingResult With {.Timestamp = #2001-04-05 14:06#, .Duration = TimeSpan.FromMilliseconds(1)})
                    Scheduler.AdvanceBy(1)

                    ' The graph should be re-populated because the new result might 
                    ' need to be grouped with other results within the same step range.
                    historyProvider.Verify(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#), Times.Exactly(2))
                End Using
            End Function


            <Fact()>
            Public Async Function DoesNotAddNewResultsWhenResultIsOutsideVisibleWindow() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)
                Dim results As Subject(Of PingResult)


                results = New Subject(Of PingResult)

                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync({})

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object, results:=results)
                    Await vm.OnLoadedAsync()

                    historyProvider.Verify(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#), Times.Once)

                    results.OnNext(New PingResult With {.Timestamp = #2001-04-04 23:59#, .Duration = TimeSpan.FromMilliseconds(1)})
                    Scheduler.AdvanceBy(1)

                    historyProvider.Verify(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#), Times.Once)
                End Using
            End Function


            <Fact()>
            Public Async Function StopsUpdatingWithNewResultsAfterBeingDisposed() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)
                Dim results As Subject(Of PingResult)


                results = New Subject(Of PingResult)

                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync({})

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object, results:=results)
                    Await vm.OnLoadedAsync()

                    historyProvider.Verify(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#), Times.Once)
                End Using

                results.OnNext(New PingResult With {.Timestamp = #2001-04-05 14:06#, .Duration = TimeSpan.FromMilliseconds(1)})
                Scheduler.AdvanceBy(1)

                historyProvider.Verify(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#), Times.Once)
            End Function

        End Class


        Public Class MoveBackCommandProperty
            Inherits TestBase


            <Fact()>
            Public Async Function MovesTheVisibleRangeByTwentyFourHoursIntoThePast() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-05#, .Duration = TimeSpan.FromMilliseconds(10)}}
                )

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-04#, #2001-04-05#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-04#, .Duration = TimeSpan.FromMilliseconds(20)}}
                )

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    Assert.Equal(DateTimeAxis.CreateDataPoint(#2001-04-05#, 10), DirectCast(vm.Plot.Series(0), AreaSeries).Points(0))

                    vm.MoveBackCommand.Execute(Nothing)

                    Assert.Equal(DateTimeAxis.CreateDataPoint(#2001-04-04#, 20), DirectCast(vm.Plot.Series(0), AreaSeries).Points(0))
                End Using
            End Function

        End Class


        Public Class MoveForwardCommandProperty
            Inherits TestBase


            <Fact()>
            Public Async Function MovesTheVisibleRangeByTwentyFourHoursIntoTheFuture() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-05#, .Duration = TimeSpan.FromMilliseconds(10)}}
                )

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-06#, #2001-04-07#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-06#, .Duration = TimeSpan.FromMilliseconds(20)}}
                )

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    Assert.Equal(DateTimeAxis.CreateDataPoint(#2001-04-05#, 10), DirectCast(vm.Plot.Series(0), AreaSeries).Points(0))

                    vm.MoveForwardCommand.Execute(Nothing)

                    Assert.Equal(DateTimeAxis.CreateDataPoint(#2001-04-06#, 20), DirectCast(vm.Plot.Series(0), AreaSeries).Points(0))
                End Using
            End Function

        End Class


        Public Class SelectedDateProperty
            Inherits TestBase


            <Fact()>
            Public Async Function MovesTheVisibleRangeToStartAtTheNewDateAndKeepTheOriginalLength() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)

                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync({})

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-05 19:00#, #2001-04-06 02:00#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-05 19:00#, .Duration = TimeSpan.FromMilliseconds(10)}}
                )

                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-01 19:00#, #2001-04-02 02:00#)).ReturnsAsync(
                    {New PingResult With {.Timestamp = #2001-04-01 19:00#, .Duration = TimeSpan.FromMilliseconds(20)}}
                )

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    ' Zoom into a range that is not 24 hours.
                    axis.Zoom(DateTimeAxis.ToDouble(#2001-04-05 19:00#), DateTimeAxis.ToDouble(#2001-04-06 02:00#))
                    Scheduler.AdvanceBy(1)

                    ' Check that the axis is correct.
                    Assert.Equal(DateTimeAxis.ToDateTime(axis.ActualMinimum), #2001-04-05 19:00#)
                    Assert.Equal(DateTimeAxis.ToDateTime(axis.ActualMaximum), #2001-04-06 02:00#)

                    ' And check that the chart was populated with the range.
                    Assert.Equal(
                        DateTimeAxis.CreateDataPoint(#2001-04-05 19:00#, 10),
                        DirectCast(vm.Plot.Series(0), AreaSeries).Points(0)
                    )

                    ' Jump to a different date.
                    vm.SelectedDate = #2001-04-01#
                    Scheduler.AdvanceBy(1)

                    ' Check that the axis is correct.
                    Assert.Equal(DateTimeAxis.ToDateTime(axis.ActualMinimum), #2001-04-01 19:00#)
                    Assert.Equal(DateTimeAxis.ToDateTime(axis.ActualMaximum), #2001-04-02 02:00#)

                    ' And check that the chart was populated with the new range.
                    Assert.Equal(
                        DateTimeAxis.CreateDataPoint(#2001-04-01 19:00#, 20),
                        DirectCast(vm.Plot.Series(0), AreaSeries).Points(0)
                    )
                End Using
            End Function


            <Fact()>
            Public Async Function IgnoresTheTimePortionOfTheGivenDate() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync({})

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    ' Zoom into a range that is not 24 hours.
                    axis.Zoom(DateTimeAxis.ToDouble(#2001-04-05 19:00#), DateTimeAxis.ToDouble(#2001-04-06 02:00#))
                    Scheduler.AdvanceBy(1)

                    ' Check that the axis is correct.
                    Assert.Equal(DateTimeAxis.ToDateTime(axis.ActualMinimum), #2001-04-05 19:00#)
                    Assert.Equal(DateTimeAxis.ToDateTime(axis.ActualMaximum), #2001-04-06 02:00#)

                    ' Jump to a different date.
                    vm.SelectedDate = #2001-04-01 05:00#
                    Scheduler.AdvanceBy(1)

                    ' Check that the axis was moved to the new date, and 
                    ' that the time portion of the given date had no impact.
                    Assert.Equal(DateTimeAxis.ToDateTime(axis.ActualMinimum), #2001-04-01 19:00#)
                    Assert.Equal(DateTimeAxis.ToDateTime(axis.ActualMaximum), #2001-04-02 02:00#)
                End Using
            End Function


            <Fact()>
            Public Async Function UpdatesWhenMovingTheVisibleRangeForward() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#)).ReturnsAsync({})

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Await vm.OnLoadedAsync()

                    Assert.Equal(#2001-04-05#, vm.SelectedDate)

                    vm.MoveForwardCommand.Execute(Nothing)

                    Assert.Equal(#2001-04-06#, vm.SelectedDate)
                End Using
            End Function


            <Fact()>
            Public Async Function UpdatesWhenMovingTheVisibleRangeBackwards() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#)).ReturnsAsync({})

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Await vm.OnLoadedAsync()

                    Assert.Equal(#2001-04-05#, vm.SelectedDate)

                    vm.MoveBackCommand.Execute(Nothing)

                    Assert.Equal(#2001-04-04#, vm.SelectedDate)
                End Using
            End Function


            <Fact()>
            Public Async Function UpdatesWhenTimeAxisChanges() As Task
                Dim historyProvider As Mock(Of IHistoryProvider)


                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(#2001-04-05#, #2001-04-06#)).ReturnsAsync({})

                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#, historyProvider:=historyProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    Assert.Equal(#2001-04-05#, vm.SelectedDate)

                    ' Zoom to the new time range and confirm that the graph shows the new data.
                    axis.Zoom(DateTimeAxis.ToDouble(#2001-04-06#), DateTimeAxis.ToDouble(#2001-04-07#))
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(#2001-04-06#, vm.SelectedDate)
                End Using
            End Function

        End Class


        Public MustInherit Class TestBase

            Protected Property Scheduler As TestScheduler


            Protected Function CreateViewModel(
                    Optional results As IObservable(Of PingResult) = Nothing,
                    Optional history As IEnumerable(Of PingResult) = Nothing,
                    Optional historyProvider As IHistoryProvider = Nothing,
                    Optional dateTime As Date = Nothing,
                    Optional dateTimeProvider As IDateTimeProvider = Nothing
                ) As HistoryViewModel

                Dim vm As HistoryViewModel
                Dim resultSource As Mock(Of IPingResultSource)


                If results Is Nothing Then
                    results = Observable.Empty(Of PingResult)
                End If

                resultSource = New Mock(Of IPingResultSource)
                resultSource.SetupGet(Function(x) x.Results).Returns(results)

                If historyProvider Is Nothing Then
                    Dim mock As Mock(Of IHistoryProvider)


                    If history Is Nothing Then
                        history = {}
                    End If

                    mock = New Mock(Of IHistoryProvider)
                    mock.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(history)

                    historyProvider = mock.Object
                End If

                If dateTimeProvider Is Nothing Then
                    Dim mock As Mock(Of IDateTimeProvider)


                    If dateTime = Date.MinValue Then
                        dateTime = #2001-01-01#
                    End If

                    mock = New Mock(Of IDateTimeProvider)
                    mock.Setup(Function(x) x.GetDateTime()).Returns(dateTime)

                    dateTimeProvider = mock.Object
                End If

                Scheduler = New TestScheduler
                Scheduler.AdvanceTo(dateTimeProvider.GetDateTime().Ticks)

                vm = New HistoryViewModel(Scheduler, historyProvider, dateTimeProvider, resultSource.Object)

                ' Update the plot, because that is what the 
                ' WPF view would do when it is loaded. This
                ' causes the "ActualX" properties to be updated.
                DirectCast(vm.Plot, IPlotModel).Update(True)

                ' Tick the scheduler so that the event handlers are hooked up.
                ' For some reason this seems to require two ticks instead of just one.
                Scheduler.AdvanceBy(2)

                Return vm
            End Function

        End Class

    End Class

End Namespace
