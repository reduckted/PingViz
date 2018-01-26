Imports OxyPlot
Imports OxyPlot.Axes


Namespace Views

    Public Class DayBoundsAnnotationTests

        <Fact()>
        Public Sub DrawsVerticalLineOnEachDayBoundary()
            Dim plot As PlotModel
            Dim context As Mock(Of IRenderContext)


            context = New Mock(Of IRenderContext)

            context _
                .Setup(Function(x) x.MeasureText(It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of Double), It.IsAny(Of Double))) _
                .Returns(New OxySize(50, 25))

            plot = New PlotModel
            plot.Axes.Add(CreateTimeAxis(#2000-01-01#, #2000-01-04#, TimeSpan.FromDays(0.5), TimeSpan.FromDays(0.25)))
            plot.Axes.Add(CreateDurationAxis())

            plot.Annotations.Add(New DayBoundsAnnotation With {.XAxisKey = "X"})
            DirectCast(plot, IPlotModel).Update(True)

            plot.Annotations(0).Render(context.Object)

            For Each offset In {0, 1, 2, 3}
                Dim x As Double = offset


                context.Verify(
                    Sub(c) c.DrawLine(
                        It.Is(Function(p As IList(Of ScreenPoint)) p(0).X = x AndAlso p(0).Y = 0 AndAlso p(1).X = x AndAlso p(1).Y = 1),
                        It.IsAny(Of OxyColor),
                        1,
                        Nothing,
                        LineJoin.Miter,
                        True
                    ),
                    Times.Once()
                )
            Next offset
        End Sub


        <Fact()>
        Public Sub ShadesEverySecondDay()
            Dim plot As PlotModel
            Dim context As Mock(Of IRenderContext)


            context = New Mock(Of IRenderContext)

            context _
                .Setup(Function(x) x.MeasureText(It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of Double), It.IsAny(Of Double))) _
                .Returns(New OxySize(50, 25))

            plot = New PlotModel
            plot.Axes.Add(CreateTimeAxis(#2000-01-01#, #2000-01-04#, TimeSpan.FromDays(0.5), TimeSpan.FromDays(0.25)))
            plot.Axes.Add(CreateDurationAxis())

            plot.Annotations.Add(New DayBoundsAnnotation With {.XAxisKey = "X"})
            DirectCast(plot, IPlotModel).Update(True)

            plot.Annotations(0).Render(context.Object)

            For Each offset In {1, 3}
                Dim x As Double = offset


                context.Verify(
                    Sub(c) c.DrawRectangle(New OxyRect(x, 0, 1, 1), It.IsAny(Of OxyColor), It.IsAny(Of OxyColor), 0),
                    Times.Once()
                )
            Next offset

            For Each offset In {0, 2, 4}
                Dim x As Double = offset


                context.Verify(
                    Sub(c) c.DrawRectangle(New OxyRect(x, 0, 1, 1), It.IsAny(Of OxyColor), It.IsAny(Of OxyColor), It.IsAny(Of Double)),
                    Times.Never()
                )
            Next offset
        End Sub


        <Fact()>
        Public Sub LabelsEachDayWhenLabelIsSmallerThanDayWidt()
            Dim plot As PlotModel
            Dim context As Mock(Of IRenderContext)


            context = New Mock(Of IRenderContext)

            context _
                .Setup(Function(x) x.MeasureText(It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of Double), It.IsAny(Of Double))) _
                .Returns(New OxySize(1, 25))

            plot = New PlotModel
            plot.Axes.Add(CreateTimeAxis(#2000-01-01#, #2000-01-04#, TimeSpan.FromDays(0.5), TimeSpan.FromDays(0.25), 1000))
            plot.Axes.Add(CreateDurationAxis())

            plot.Annotations.Add(New DayBoundsAnnotation With {.XAxisKey = "X"})
            DirectCast(plot, IPlotModel).Update(True)

            plot.Annotations(0).Render(context.Object)

            For Each d In {#2000-01-01#, #2000-01-02#, #2000-01-03#, #2000-01-04#}
                context.Verify(
                    Sub(c) c.DrawText(
                        It.IsAny(Of ScreenPoint),
                        It.Is(Function(s As String) Date.Parse(s).Equals(d)),
                        It.IsAny(Of OxyColor),
                        It.IsAny(Of String),
                        It.IsAny(Of Double),
                        It.IsAny(Of Double),
                        0,
                        HorizontalAlignment.Left,
                        VerticalAlignment.Top,
                        Nothing
                    ),
                    Times.Once()
                )
            Next d
        End Sub


        Private Shared Function CreateTimeAxis(
                minimum As Date,
                maximum As Date,
                majorStep As TimeSpan,
                minorStep As TimeSpan,
                Optional scale As Double = 1.0
            ) As DateTimeAxis

            Return New MockTimeAxis(minimum, majorStep, minorStep, scale) With {
                .Key = "X",
                .Maximum = DateTimeAxis.ToDouble(maximum),
                .IntervalType = DateTimeIntervalType.Minutes
            }
        End Function


        Private Shared Function CreateDurationAxis() As Axis
            Return New MockDurationAxis With {
                .Key = "Y",
                .Minimum = 0,
                .Maximum = 20,
                .MinorStep = 1,
                .MajorStep = 1
            }
        End Function


        Public Class MockTimeAxis
            Inherits DateTimeAxis


            Public Sub New(
                    minimum As Date,
                    majorStep As TimeSpan,
                    minorStep As TimeSpan,
                    Optional scale As Double = 1.0
                )

                Me.Minimum = ToDouble(minimum)
                Me.MinorStep = ToDouble(ToDateTime(0).Add(minorStep))
                Me.MajorStep = ToDouble(ToDateTime(0).Add(majorStep))
                ActualMajorStep = Me.MajorStep
                ActualMinorStep = Me.MinorStep
                SetTransform(scale, Me.Minimum)
            End Sub

        End Class


        Public Class MockDurationAxis
            Inherits LinearAxis


            Public Sub New()
                SetTransform(1, 0)
            End Sub

        End Class

    End Class

End Namespace
