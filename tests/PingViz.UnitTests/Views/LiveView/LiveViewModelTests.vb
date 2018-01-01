Imports Microsoft.Reactive.Testing
Imports OxyPlot
Imports OxyPlot.Axes
Imports OxyPlot.Series
Imports System.Reactive.Linq
Imports System.Reactive.Subjects

Namespace Views

    Public Class LiveViewModelTests

        Public Class IsLoadingProperty
            Inherits TestBase


            <Fact()>
            Public Sub IsInitiallyTrue()
                Using vm = CreateViewModel()
                    Assert.True(vm.IsLoading)
                End Using
            End Sub


            <Fact()>
            Public Async Function IsSetToFalseAfterOnLoadedCalled() As Task
                Using vm = CreateViewModel()
                    Await vm.OnLoadedAsync()
                    Assert.False(vm.IsLoading)
                End Using
            End Function

        End Class


        Public Class CurrentProperty
            Inherits TestBase


            <Fact()>
            Public Sub IsInitiallyNull()
                Using vm = CreateViewModel()
                    Assert.Null(vm.Current)
                End Using
            End Sub


            <Fact()>
            Public Async Function IsSetToLatestResultDuration() As Task
                Dim results As Subject(Of PingResult)


                results = New Subject(Of PingResult)

                Using vm = CreateViewModel(results:=results)
                    Await vm.OnLoadedAsync()

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(123)})
                    Assert.Equal(123, vm.Current)

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = Nothing})
                    Assert.Null(vm.Current)

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(234)})
                    Assert.Equal(234, vm.Current)
                End Using
            End Function


            <Fact()>
            Public Async Function StopsUpdatingCurrentAfterBeingDisposed() As Task
                Dim vm As LiveViewModel
                Dim results As Subject(Of PingResult)


                results = New Subject(Of PingResult)

                vm = CreateViewModel(results:=results)

                Try
                    Await vm.OnLoadedAsync()

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(123)})
                    Assert.Equal(123, vm.Current)

                Finally
                    vm.Dispose()
                End Try

                results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(234)})
                Assert.Equal(123, vm.Current)
            End Function

        End Class


        Public Class PlotProperty
            Inherits TestBase


            <Fact()>
            Public Sub ShowsThePreviousThreeMinutes()
                Using vm = CreateViewModel(dateTime:=#2001-04-05 14:05:30#)
                    Dim axis As Axis


                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())

                    Assert.NotNull(axis)
                    Assert.Equal(#2001-04-05 14:02:30#, DateTimeAxis.ToDateTime(axis.Minimum))
                    Assert.Equal(#2001-04-05 14:05:30#, DateTimeAxis.ToDateTime(axis.Maximum))
                End Using
            End Sub


            <Fact()>
            Public Async Function MovesVisibleRangeForwardAsTimeMovesForward() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date


                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:10:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    Assert.Equal(#2001-01-01 14:07:00#, DateTimeAxis.ToDateTime(axis.Minimum))
                    Assert.Equal(#2001-01-01 14:10:00#, DateTimeAxis.ToDateTime(axis.Maximum))

                    dateTime = dateTime.AddMilliseconds(500)
                    Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks)

                    Assert.Equal(#2001-01-01 14:07:00#.AddMilliseconds(500), DateTimeAxis.ToDateTime(axis.Minimum))
                    Assert.Equal(#2001-01-01 14:10:00#.AddMilliseconds(500), DateTimeAxis.ToDateTime(axis.Maximum))

                    dateTime = dateTime.AddMilliseconds(500)
                    Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks)

                    Assert.Equal(#2001-01-01 14:07:01#, DateTimeAxis.ToDateTime(axis.Minimum))
                    Assert.Equal(#2001-01-01 14:10:01#, DateTimeAxis.ToDateTime(axis.Maximum))
                End Using
            End Function


            <Fact()>
            Public Async Function StopsMovingVisibleRangeForwardWhenDisposed() As Task
                Dim vm As LiveViewModel
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date
                Dim axis As Axis


                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:10:00#

                vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object)

                Try
                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    Assert.Equal(#2001-01-01 14:07:00#, DateTimeAxis.ToDateTime(axis.Minimum))
                    Assert.Equal(#2001-01-01 14:10:00#, DateTimeAxis.ToDateTime(axis.Maximum))

                    dateTime = dateTime.AddMilliseconds(500)
                    Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks)

                    Assert.Equal(#2001-01-01 14:07:00#.AddMilliseconds(500), DateTimeAxis.ToDateTime(axis.Minimum))
                    Assert.Equal(#2001-01-01 14:10:00#.AddMilliseconds(500), DateTimeAxis.ToDateTime(axis.Maximum))

                Finally
                    vm.Dispose()
                End Try

                dateTime = dateTime.AddMilliseconds(500)
                Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks)

                Assert.Equal(#2001-01-01 14:07:00#.AddMilliseconds(500), DateTimeAxis.ToDateTime(axis.Minimum))
                Assert.Equal(#2001-01-01 14:10:00#.AddMilliseconds(500), DateTimeAxis.ToDateTime(axis.Maximum))
            End Function


            <Fact()>
            Public Async Function RoundsCurrentTimeDownToHalfSecondWhenMovingVisibleRangeForward() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date


                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:10:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object)
                    Dim axis As Axis


                    Await vm.OnLoadedAsync()

                    axis = vm.Plot.Axes.FirstOrDefault(Function(x) x.IsHorizontal())
                    Assert.NotNull(axis)

                    dateTime = dateTime.AddMilliseconds(510)
                    Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks)

                    Assert.Equal(#2001-01-01 14:07:00#.AddMilliseconds(500), DateTimeAxis.ToDateTime(axis.Minimum))
                    Assert.Equal(#2001-01-01 14:10:00#.AddMilliseconds(500), DateTimeAxis.ToDateTime(axis.Maximum))

                    dateTime = dateTime.AddMilliseconds(510)
                    Scheduler.AdvanceBy(TimeSpan.FromMilliseconds(500).Ticks)

                    Assert.Equal(#2001-01-01 14:07:01#, DateTimeAxis.ToDateTime(axis.Minimum))
                    Assert.Equal(#2001-01-01 14:10:01#, DateTimeAxis.ToDateTime(axis.Maximum))
                End Using
            End Function


            <Fact()>
            Public Async Function AddsResultsToThePlot() As Task
                Dim results As Subject(Of PingResult)
                Dim series As AreaSeries


                results = New Subject(Of PingResult)

                Using vm = CreateViewModel(dateTime:=#2001-01-01#, results:=results)
                    Await vm.OnLoadedAsync()

                    Assert.Equal(0, vm.Plot.Series.Count)

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 00:01:30#, .Duration = TimeSpan.FromMilliseconds(123)})
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(vm.Plot.Series(0))

                    Assert.Equal(2, series.Points.Count)
                    Assert.Equal(Axis.ToDouble(#2001-01-01 00:01:30#), series.Points(0).X)
                    Assert.Equal(123, series.Points(0).Y)

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 00:01:40#, .Duration = TimeSpan.FromMilliseconds(250)})
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(3, series.Points.Count)
                    Assert.Equal(Axis.ToDouble(#2001-01-01 00:01:40#), series.Points(1).X)
                    Assert.Equal(250, series.Points(1).Y)
                End Using
            End Function


            <Fact()>
            Public Async Function RoundsDownRangeOfPreviousData() As Task
                Dim provider As Mock(Of IHistoryProvider)


                provider = New Mock(Of IHistoryProvider)
                provider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync({})

                Using vm = CreateViewModel(dateTime:=#2001-01-01 14:00:12#, historyProvider:=provider.Object)
                    Await vm.OnLoadedAsync()
                    provider.Verify(Function(x) x.GetPingsAsync(#2001-01-01 13:57:10#, #2001-01-01 14:00:10#), Times.Once)
                End Using
            End Function


            <Fact()>
            Public Async Function AddsPreviousResultsToTheInitialPlot() As Task
                Dim history As List(Of PingResult)
                Dim provider As Mock(Of IHistoryProvider)
                Dim startDateTime As Date
                Dim endDateTime As Date
                Dim d As Date


                history = New List(Of PingResult)
                startDateTime = #2001-01-01 13:57#
                endDateTime = #2001-01-01 14:00#
                d = startDateTime

                Do While d <= endDateTime
                    history.Add(New PingResult With {.Timestamp = d, .Duration = TimeSpan.FromMilliseconds(history.Count + 1)})
                    d = d.AddSeconds(10)
                Loop

                provider = New Mock(Of IHistoryProvider)
                provider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(history)

                Using vm = CreateViewModel(dateTime:=endDateTime, historyProvider:=provider.Object)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    provider.Verify(Function(x) x.GetPingsAsync(startDateTime, endDateTime), Times.Once)

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)

                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:00#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:10#), 2),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:20#), 3),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:30#), 4),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:40#), 5),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:50#), 6),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:00#), 7),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:10#), 8),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:20#), 9),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:30#), 10),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:40#), 11),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:50#), 12),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:00#), 13),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:10#), 14),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:20#), 15),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:30#), 16),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:40#), 17),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:50#), 18),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 19),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 19)
                        },
                        series.Points
                    )
                End Using
            End Function


            <Fact()>
            Public Async Function DoesNotCreateNewSeriesWhenThereIsGapInHistoryAtStartOfRange() As Task
                Dim history As List(Of PingResult)
                Dim provider As Mock(Of IHistoryProvider)
                Dim startDateTime As Date
                Dim endDateTime As Date
                Dim d As Date


                history = New List(Of PingResult)
                startDateTime = #2001-01-01 13:57:10#
                endDateTime = #2001-01-01 14:00#
                d = startDateTime

                Do While d <= endDateTime
                    history.Add(New PingResult With {.Timestamp = d, .Duration = TimeSpan.FromMilliseconds(history.Count + 1)})
                    d = d.AddSeconds(10)
                Loop

                provider = New Mock(Of IHistoryProvider)
                provider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(history)

                Using vm = CreateViewModel(dateTime:=#2001-01-01 14:00#, historyProvider:=provider.Object)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)

                    Assert.Equal(19, series.Points.Count)
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 13:57:10#), 1), series.Points(0))
                End Using
            End Function


            <Fact()>
            Public Async Function CreateNewSeriesWhenThereIsGapInHistoryAtEndOfRange() As Task
                Dim results As Subject(Of PingResult)
                Dim history As List(Of PingResult)
                Dim historyProvider As Mock(Of IHistoryProvider)
                Dim startDateTime As Date
                Dim endDateTime As Date
                Dim d As Date


                results = New Subject(Of PingResult)
                history = New List(Of PingResult)
                startDateTime = #2001-01-01 13:57:00#
                endDateTime = #2001-01-01 13:59:40#
                d = startDateTime

                Do While d <= endDateTime
                    history.Add(New PingResult With {.Timestamp = d, .Duration = TimeSpan.FromMilliseconds(history.Count + 1)})
                    d = d.AddSeconds(10)
                Loop

                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(history)

                Using vm = CreateViewModel(dateTime:=#2001-01-01 14:00#, historyProvider:=historyProvider.Object, results:=results)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    Assert.Equal(1, vm.Plot.Series.Count)

                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(18, series.Points.Count)
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 13:59:40#), 17), series.Points(16))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 13:59:50#), 17), series.Points(17))

                    ' Push a result through to confirm that a new series is started.
                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00#, .Duration = TimeSpan.FromMilliseconds(100)})
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(2, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(1), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(2, series.Points.Count)
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00#), 100), series.Points(0))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00#), 100), series.Points(1))
                End Using
            End Function


            <Fact()>
            Public Async Function DoesNotCreateNewSeriesWhenThereIsGapInHistoryAtEndOfRangeButNewResultClosesGap() As Task
                Dim results As Subject(Of PingResult)
                Dim history As List(Of PingResult)
                Dim historyProvider As Mock(Of IHistoryProvider)
                Dim startDateTime As Date
                Dim endDateTime As Date
                Dim d As Date


                results = New Subject(Of PingResult)
                history = New List(Of PingResult)
                startDateTime = #2001-01-01 13:57:00#
                endDateTime = #2001-01-01 13:59:50#
                d = startDateTime

                Do While d <= endDateTime
                    history.Add(New PingResult With {.Timestamp = d, .Duration = TimeSpan.FromMilliseconds(history.Count + 1)})
                    d = d.AddSeconds(10)
                Loop

                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(history)

                Using vm = CreateViewModel(dateTime:=#2001-01-01 14:00#, historyProvider:=historyProvider.Object, results:=results)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    Assert.Equal(1, vm.Plot.Series.Count)

                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(19, series.Points.Count)
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 13:59:50#), 18), series.Points(17))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 18), series.Points(18))

                    ' Push a result through to confirm that a new series is started.
                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00#, .Duration = TimeSpan.FromMilliseconds(100)})
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(1, vm.Plot.Series.Count)
                    Assert.Equal(20, series.Points.Count)
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 13:59:50#), 18), series.Points(17))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 100), series.Points(18))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 100), series.Points(19))
                End Using
            End Function


            <Fact()>
            Public Async Function CreateNewSeriesWhenThereIsGapInHistoryInMiddleOfRange() As Task
                Dim history As IEnumerable(Of PingResult)
                Dim provider As Mock(Of IHistoryProvider)


                history = {
                    New PingResult With {.Timestamp = #2001-01-01 13:57:00#, .Duration = TimeSpan.FromMilliseconds(1)},
                    New PingResult With {.Timestamp = #2001-01-01 13:57:10#, .Duration = TimeSpan.FromMilliseconds(2)},
                    New PingResult With {.Timestamp = #2001-01-01 13:57:20#, .Duration = TimeSpan.FromMilliseconds(3)},
                    New PingResult With {.Timestamp = #2001-01-01 13:57:30#, .Duration = TimeSpan.FromMilliseconds(4)},
                    New PingResult With {.Timestamp = #2001-01-01 13:57:40#, .Duration = TimeSpan.FromMilliseconds(5)},
                    New PingResult With {.Timestamp = #2001-01-01 13:57:50#, .Duration = TimeSpan.FromMilliseconds(6)},
                    New PingResult With {.Timestamp = #2001-01-01 13:58:00#, .Duration = TimeSpan.FromMilliseconds(7)},
                    New PingResult With {.Timestamp = #2001-01-01 13:58:10#, .Duration = TimeSpan.FromMilliseconds(8)},
                    New PingResult With {.Timestamp = #2001-01-01 13:58:30#, .Duration = TimeSpan.FromMilliseconds(9)},
                    New PingResult With {.Timestamp = #2001-01-01 13:58:40#, .Duration = TimeSpan.FromMilliseconds(10)},
                    New PingResult With {.Timestamp = #2001-01-01 13:58:50#, .Duration = TimeSpan.FromMilliseconds(11)},
                    New PingResult With {.Timestamp = #2001-01-01 13:59:00#, .Duration = TimeSpan.FromMilliseconds(12)},
                    New PingResult With {.Timestamp = #2001-01-01 13:59:40#, .Duration = TimeSpan.FromMilliseconds(13)},
                    New PingResult With {.Timestamp = #2001-01-01 13:59:50#, .Duration = TimeSpan.FromMilliseconds(14)},
                    New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(15)}
                }

                provider = New Mock(Of IHistoryProvider)
                provider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(history)

                Using vm = CreateViewModel(dateTime:=#2001-01-01 14:00#, historyProvider:=provider.Object)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    Assert.Equal(3, vm.Plot.Series.Count)

                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:00#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:10#), 2),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:20#), 3),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:30#), 4),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:40#), 5),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:50#), 6),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:00#), 7),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:10#), 8),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:20#), 8)
                        },
                        series.Points
                    )

                    series = TryCast(vm.Plot.Series(1), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:30#), 9),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:40#), 10),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:58:50#), 11),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:00#), 12),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:10#), 12)
                        },
                        series.Points
                    )

                    series = TryCast(vm.Plot.Series(2), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:40#), 13),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:50#), 14),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 15),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 15)
                        },
                        series.Points
                    )
                End Using
            End Function


            <Fact()>
            Public Async Function RemovesUnnecessaryPointsAfterMovingVisibleRangeForward() As Task
                Dim history As IEnumerable(Of PingResult)
                Dim historyProvider As Mock(Of IHistoryProvider)
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date


                history = {
                    New PingResult With {.Timestamp = #2001-01-01 13:57:00#, .Duration = TimeSpan.FromMilliseconds(1)},
                    New PingResult With {.Timestamp = #2001-01-01 13:57:10#, .Duration = TimeSpan.FromMilliseconds(2)},
                    New PingResult With {.Timestamp = #2001-01-01 13:57:20#, .Duration = TimeSpan.FromMilliseconds(3)},
                    New PingResult With {.Timestamp = #2001-01-01 13:57:30#, .Duration = TimeSpan.FromMilliseconds(4)},
                    New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(5)}
                }

                historyProvider = New Mock(Of IHistoryProvider)
                historyProvider.Setup(Function(x) x.GetPingsAsync(It.IsAny(Of Date), It.IsAny(Of Date))).ReturnsAsync(history)

                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:00:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object, historyProvider:=historyProvider.Object)
                    Await vm.OnLoadedAsync()

                    ' 14:00:00
                    Assert.Equal(2, vm.Plot.Series.Count)
                    Assert.Equal(5, TryCast(vm.Plot.Series(0), AreaSeries)?.Points.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(1), AreaSeries)?.Points.Count)

                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:00#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:10#), 2),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:20#), 3),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:30#), 4),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:40#), 4)
                        },
                        TryCast(vm.Plot.Series(0), AreaSeries)?.Points
                    )

                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 5),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 5)
                        },
                        TryCast(vm.Plot.Series(1), AreaSeries)?.Points
                    )

                    dateTime = dateTime.AddSeconds(5)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    ' 14:00:05
                    Assert.Equal(2, vm.Plot.Series.Count)
                    Assert.Equal(5, TryCast(vm.Plot.Series(0), AreaSeries)?.Points.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(1), AreaSeries)?.Points.Count)

                    ' 14:00:10
                    dateTime = dateTime.AddSeconds(5)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(2, vm.Plot.Series.Count)
                    Assert.Equal(4, TryCast(vm.Plot.Series(0), AreaSeries)?.Points.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(1), AreaSeries)?.Points.Count)

                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:10#), 2),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:20#), 3),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:30#), 4),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:40#), 4)
                        },
                        TryCast(vm.Plot.Series(0), AreaSeries)?.Points
                    )

                    dateTime = dateTime.AddSeconds(5)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    ' 14:00:15
                    Assert.Equal(2, vm.Plot.Series.Count)
                    Assert.Equal(4, TryCast(vm.Plot.Series(0), AreaSeries)?.Points.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(1), AreaSeries)?.Points.Count)

                    dateTime = dateTime.AddSeconds(5)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    ' 14:00:20
                    Assert.Equal(2, vm.Plot.Series.Count)
                    Assert.Equal(3, TryCast(vm.Plot.Series(0), AreaSeries)?.Points.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(1), AreaSeries)?.Points.Count)

                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:20#), 3),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:30#), 4),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:40#), 4)
                        },
                        TryCast(vm.Plot.Series(0), AreaSeries)?.Points
                    )

                    dateTime = dateTime.AddSeconds(5)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    ' 14:00:25
                    Assert.Equal(2, vm.Plot.Series.Count)
                    Assert.Equal(3, TryCast(vm.Plot.Series(0), AreaSeries)?.Points.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(1), AreaSeries)?.Points.Count)

                    dateTime = dateTime.AddSeconds(5)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    ' 14:00:30
                    Assert.Equal(2, vm.Plot.Series.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(0), AreaSeries)?.Points.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(1), AreaSeries)?.Points.Count)

                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:30#), 4),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:57:40#), 4)
                        },
                        TryCast(vm.Plot.Series(0), AreaSeries)?.Points
                    )

                    dateTime = dateTime.AddSeconds(5)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    ' 14:00:35
                    Assert.Equal(2, vm.Plot.Series.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(0), AreaSeries)?.Points.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(1), AreaSeries)?.Points.Count)

                    dateTime = dateTime.AddSeconds(5)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    ' 14:00:40
                    Assert.Equal(1, vm.Plot.Series.Count)
                    Assert.Equal(2, TryCast(vm.Plot.Series(0), AreaSeries)?.Points.Count)

                    Assert.Equal(
                        New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 5),
                        TryCast(vm.Plot.Series(0), AreaSeries)?.Points(0)
                    )
                End Using
            End Function


            <Fact()>
            Public Async Function AddsPointAtMaximumTimeAfterLatestResult() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date
                Dim results As Subject(Of PingResult)


                results = New Subject(Of PingResult)

                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:00:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object, results:=results)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(1)})
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(2, series.Points.Count)

                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1), series.Points(0))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1), series.Points(1))

                    ' Fast-forward time and confirm that the last
                    ' point is still equal to the maximum time.
                    dateTime = dateTime.AddMinutes(1)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1), series.Points(0))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:01:00#), 1), series.Points(1))

                    ' Fast-forward so that the first point is less than the minimum time.
                    ' Confirm that the last point is still equal to the maximum time.
                    dateTime = dateTime.AddMinutes(3)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1), series.Points(0))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:04:00#), 1), series.Points(1))
                End Using
            End Function


            <Fact()>
            Public Async Function AddsPointAtMaximumTimeWhenLastHistoryRecordIsAtMaximum() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date
                Dim history As IEnumerable(Of PingResult)


                history = {New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(1)}}

                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:00:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object, history:=history)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(2, series.Points.Count)

                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1), series.Points(0))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1), series.Points(1))

                    ' Fast-forward time and confirm that the last
                    ' point is still equal to the maximum time.
                    dateTime = dateTime.AddMinutes(1)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1), series.Points(0))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:01:00#), 1), series.Points(1))
                End Using
            End Function


            <Fact()>
            Public Async Function AddsPointAtMaximumTimeWhenLastHistoryRecordIsAtRoundedDownMaximum() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date
                Dim history As IEnumerable(Of PingResult)


                history = {New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(1)}}

                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:00:02#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object, history:=history)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(2, series.Points.Count)

                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1), series.Points(0))
                    Assert.Equal(New DataPoint(Axis.ToDouble(#2001-01-01 14:00:02#), 1), series.Points(1))
                End Using
            End Function


            <Fact()>
            Public Async Function StartsNewSeriesWhenGapInResults() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date
                Dim results As Subject(Of PingResult)


                results = New Subject(Of PingResult)

                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:00:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object, results:=results)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(1)})
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1)
                        },
                        series.Points
                    )

                    ' Fast-forward time and add a new result.
                    dateTime = dateTime.AddMinutes(1)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:01:00#, .Duration = TimeSpan.FromMilliseconds(2)})
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(2, vm.Plot.Series.Count)

                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:10#), 1)
                        },
                        series.Points
                    )

                    series = TryCast(vm.Plot.Series(1), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:01:00#), 2),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:01:00#), 2)
                        },
                        series.Points
                    )
                End Using
            End Function


            <Fact()>
            Public Async Function StartsNewSeriesWhenTimeoutsStart() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date
                Dim results As Subject(Of PingResult)


                results = New Subject(Of PingResult)

                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:00:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object, results:=results)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(100)})
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 100),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 100)
                        },
                        series.Points
                    )

                    ' Add a timeout.
                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:10#, .Duration = Nothing})
                    dateTime = dateTime.AddSeconds(10)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(2, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(1), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:10#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:10#), 1)
                        },
                        series.Points
                    )

                    Assert.NotEqual(
                        DirectCast(vm.Plot.Series(0), AreaSeries).YAxisKey,
                        DirectCast(vm.Plot.Series(1), AreaSeries).YAxisKey
                    )
                End Using
            End Function


            <Fact()>
            Public Async Function UsesSameSeriesWhenTimeoutsContinue() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date
                Dim results As Subject(Of PingResult)


                results = New Subject(Of PingResult)

                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:00:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object, results:=results)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(100)})
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 100),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 100)
                        },
                        series.Points
                    )

                    ' Add a timeout.
                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:10#, .Duration = Nothing})
                    dateTime = dateTime.AddSeconds(10)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(2, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(1), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:10#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:10#), 1)
                        },
                        series.Points
                    )

                    ' Add another timeout.
                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:20#, .Duration = Nothing})
                    dateTime = dateTime.AddSeconds(10)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(2, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(1), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:10#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:20#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:20#), 1)
                        },
                        series.Points
                    )
                End Using
            End Function


            <Fact()>
            Public Async Function StartsNewSeriesWhenTimeoutsEnd() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date
                Dim results As Subject(Of PingResult)


                results = New Subject(Of PingResult)

                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:00:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object, results:=results)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:00#, .Duration = TimeSpan.FromMilliseconds(100)})
                    Scheduler.AdvanceBy(1)

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 100),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 100)
                        },
                        series.Points
                    )

                    ' Add a timeout.
                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:10#, .Duration = Nothing})
                    dateTime = dateTime.AddSeconds(10)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(2, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(1), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:10#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:10#), 1)
                        },
                        series.Points
                    )

                    ' Add a non-timeout result.
                    results.OnNext(New PingResult With {.Timestamp = #2001-01-01 14:00:20#, .Duration = TimeSpan.FromMilliseconds(25)})
                    dateTime = dateTime.AddSeconds(10)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(3, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(1), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:10#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:20#), 1)
                        },
                        series.Points
                    )

                    Assert.Equal(3, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(2), AreaSeries)
                    Assert.NotNull(series)
                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:20#), 25),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:20#), 25)
                        },
                        series.Points
                    )

                    Assert.NotEqual(
                        DirectCast(vm.Plot.Series(0), AreaSeries).YAxisKey,
                        DirectCast(vm.Plot.Series(1), AreaSeries).YAxisKey
                    )

                    Assert.Equal(
                        DirectCast(vm.Plot.Series(0), AreaSeries).YAxisKey,
                        DirectCast(vm.Plot.Series(2), AreaSeries).YAxisKey
                    )
                End Using
            End Function


            <Fact()>
            Public Async Function DoesNotMoveLastPointThatIsBeforeMaximumWhenMovingVisibleRange() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date
                Dim history As IEnumerable(Of PingResult)


                history = {
                    New PingResult With {.Timestamp = #2001-01-01 13:59:00#, .Duration = TimeSpan.FromMilliseconds(1)}
                }

                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:00:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object, history:=history)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)

                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:00#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:10#), 1)
                        },
                        series.Points
                    )

                    ' Fast-forward time and confirm that the 
                    ' last point is the same as what it was.
                    dateTime = dateTime.AddMinutes(1)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:00#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:10#), 1)
                        },
                        series.Points
                    )
                End Using
            End Function


            <Fact()>
            Public Async Function MovesLastPointThatIsAtMaximumWhenMovingVisibleRange() As Task
                Dim dateTimeProvider As Mock(Of IDateTimeProvider)
                Dim dateTime As Date
                Dim history As IEnumerable(Of PingResult)


                history = {
                    New PingResult With {.Timestamp = #2001-01-01 13:59:50#, .Duration = TimeSpan.FromMilliseconds(1)}
                }

                dateTimeProvider = New Mock(Of IDateTimeProvider)
                dateTimeProvider.Setup(Function(x) x.GetDateTime()).Returns(Function() dateTime)

                dateTime = #2001-01-01 14:00:00#

                Using vm = CreateViewModel(dateTimeProvider:=dateTimeProvider.Object, history:=history)
                    Dim series As AreaSeries


                    Await vm.OnLoadedAsync()

                    Assert.Equal(1, vm.Plot.Series.Count)
                    series = TryCast(vm.Plot.Series(0), AreaSeries)
                    Assert.NotNull(series)

                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:50#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:00:00#), 1)
                        },
                        series.Points
                    )

                    ' Fast-forward time and confirm that the 
                    ' last point moved to the new maximum.
                    dateTime = dateTime.AddMinutes(1)
                    Scheduler.AdvanceTo(dateTime.Ticks)

                    Assert.Equal(
                        {
                            New DataPoint(Axis.ToDouble(#2001-01-01 13:59:50#), 1),
                            New DataPoint(Axis.ToDouble(#2001-01-01 14:01:00#), 1)
                        },
                        series.Points
                    )
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
                ) As LiveViewModel

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

                Return New LiveViewModel(resultSource.Object, historyProvider, dateTimeProvider, Scheduler)
            End Function

        End Class

    End Class

End Namespace
