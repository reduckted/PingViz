Imports OxyPlot
Imports OxyPlot.Axes


Namespace Views

    Public Class TimeAxisLabelAnnotationTests

        <Fact()>
        Public Sub DoesNotRenderIfThereIsNoMajorStep()
            Dim plot As PlotModel
            Dim context As Mock(Of IRenderContext)


            context = New Mock(Of IRenderContext)

            plot = New PlotModel
            plot.Axes.Add(CreateTimeAxis(#2001-01-01#, #2001-01-02#, Nothing, TimeSpan.FromHours(1)))
            plot.Axes.Add(CreateDurationAxis())

            plot.Annotations.Add(New TimeAxisLabelAnnotation With {.XAxisKey = "X"})
            DirectCast(plot, IPlotModel).Update(True)

            plot.Annotations(0).Render(context.Object)

            context.VerifyNoOtherCalls()
        End Sub


        <Fact()>
        Public Sub DoesNotRenderIfThereIsNoMinorStep()
            Dim plot As PlotModel
            Dim context As Mock(Of IRenderContext)


            context = New Mock(Of IRenderContext)

            plot = New PlotModel
            plot.Axes.Add(CreateTimeAxis(#2001-01-01#, #2001-01-02#, TimeSpan.FromHours(1), Nothing))
            plot.Axes.Add(CreateDurationAxis())

            plot.Annotations.Add(New TimeAxisLabelAnnotation With {.XAxisKey = "X"})
            DirectCast(plot, IPlotModel).Update(True)

            plot.Annotations(0).Render(context.Object)

            context.VerifyNoOtherCalls()
        End Sub


        <Fact()>
        Public Sub DrawsTextForEachMajorTick()
            Dim plot As PlotModel
            Dim context As Mock(Of IRenderContext)


            context = New Mock(Of IRenderContext)

            plot = New PlotModel
            plot.Axes.Add(CreateTimeAxis(#2001-01-01 09:00#, #2001-01-01 10:00#, TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(5)))
            plot.Axes.Add(CreateDurationAxis())

            plot.Annotations.Add(New TimeAxisLabelAnnotation With {.XAxisKey = "X"})
            DirectCast(plot, IPlotModel).Update(True)

            plot.Annotations(0).Render(context.Object)

            For Each label In {"09:00", "09:10", "09:20", "09:30", "09:40", "09:50", "10:00"}
                context.Verify(
                    Sub(x) x.DrawText(
                        It.IsAny(Of ScreenPoint),
                        label,
                        It.IsAny(Of OxyColor),
                        It.IsAny(Of String),
                        It.IsAny(Of Double),
                        FontWeights.Normal,
                        0,
                        HorizontalAlignment.Left,
                        VerticalAlignment.Bottom,
                        Nothing
                    ),
                    Times.Once
                )
            Next label

            context.VerifyNoOtherCalls()
        End Sub


        Private Shared Function CreateTimeAxis(
                minimum As Date,
                maximum As Date,
                majorStep As TimeSpan,
                minorStep As TimeSpan
            ) As DateTimeAxis

            Return New MockAxis(majorStep, minorStep) With {
                .Key = "X",
                .Minimum = DateTimeAxis.ToDouble(minimum),
                .Maximum = DateTimeAxis.ToDouble(maximum),
                .IntervalType = DateTimeIntervalType.Minutes
            }
        End Function


        Private Shared Function CreateDurationAxis() As Axis
            Return New LinearAxis With {
                .Key = "Y",
                .Minimum = 0,
                .Maximum = 20,
                .MinorStep = 1,
                .MajorStep = 1
            }
        End Function


        Public Class MockAxis
            Inherits DateTimeAxis


            Public Sub New(
                    majorStep As TimeSpan,
                    minorStep As TimeSpan
                )

                Me.MinorStep = ToDouble(ToDateTime(0).Add(minorStep))
                Me.MajorStep = ToDouble(ToDateTime(0).Add(majorStep))
                ActualMajorStep = Me.MajorStep
                ActualMinorStep = Me.MinorStep
            End Sub

        End Class

    End Class

End Namespace
