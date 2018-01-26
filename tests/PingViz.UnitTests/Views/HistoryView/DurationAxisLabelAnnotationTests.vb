Imports OxyPlot
Imports OxyPlot.Axes

Namespace Views

    Public Class DurationAxisLabelAnnotationTests

        <Fact()>
        Public Sub DoesNotRenderIfThereIsNoMajorStep()
            Dim plot As PlotModel
            Dim context As Mock(Of IRenderContext)


            context = New Mock(Of IRenderContext)

            plot = New PlotModel
            plot.Axes.Add(CreateTimeAxis())
            plot.Axes.Add(CreateDurationAxis(0, 1000, 0, 10))

            plot.Annotations.Add(New DurationAxisLabelAnnotation With {.XAxisKey = "Y"})
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
            plot.Axes.Add(CreateTimeAxis())
            plot.Axes.Add(CreateDurationAxis(0, 1000, 100, 0))

            plot.Annotations.Add(New DurationAxisLabelAnnotation With {.XAxisKey = "Y"})
            DirectCast(plot, IPlotModel).Update(True)

            plot.Annotations(0).Render(context.Object)

            context.VerifyNoOtherCalls()
        End Sub


        <Fact()>
        Public Sub DrawsTextForEachMajorTick()
            Dim plot As PlotModel
            Dim context As Mock(Of IRenderContext)


            context = New Mock(Of IRenderContext)

            context _
                .Setup(Function(x) x.MeasureText(It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of Double), It.IsAny(Of Double))) _
                .Returns(New OxySize(50, 25))

            plot = New PlotModel
            plot.Axes.Add(CreateTimeAxis())
            plot.Axes.Add(CreateDurationAxis(0, 500, 100, 10))

            plot.Annotations.Add(
                New DurationAxisLabelAnnotation With {
                    .XAxisKey = "Y",
                    .Font = "Arial",
                    .FontSize = 10
                }
            )

            DirectCast(plot, IPlotModel).Update(True)

            plot.Annotations(0).Render(context.Object)

            For Each label In {"100", "200", "300", "400"}
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

            context.Verify(Function(x) x.MeasureText(It.IsAny(Of String), It.IsAny(Of String), It.IsAny(Of Double), It.IsAny(Of Double)))
            context.VerifyNoOtherCalls()
        End Sub


        Private Shared Function CreateTimeAxis() As DateTimeAxis
            Return New DateTimeAxis With {
                .Minimum = DateTimeAxis.ToDouble(#2001-01-01#),
                .Maximum = DateTimeAxis.ToDouble(#2001-01-02#)
            }
        End Function


        Private Shared Function CreateDurationAxis(
                minimum As Double,
                maximum As Double,
                majorStep As Double,
                minorStep As Double
            ) As LinearAxis

            Return New MockAxis(majorStep, minorStep) With {
                .Key = "Y",
                .Minimum = minimum,
                .Maximum = maximum
            }
        End Function


        Public Class MockAxis
            Inherits LinearAxis


            Public Sub New(
                    majorStep As Double,
                    minorStep As Double
                )

                Me.MinorStep = minorStep
                Me.MajorStep = majorStep
                ActualMajorStep = Me.MajorStep
                ActualMinorStep = Me.MinorStep
                SetTransform(-1, 0)
            End Sub

        End Class

    End Class

End Namespace
