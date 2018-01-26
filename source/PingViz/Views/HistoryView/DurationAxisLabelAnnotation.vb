Imports OxyPlot
Imports OxyPlot.Annotations


Namespace Views

    Public Class DurationAxisLabelAnnotation
        Inherits Annotation


        Public Overrides Sub Render(rc As IRenderContext)
            If (YAxis.ActualMinorStep > 0) AndAlso (YAxis.ActualMajorStep > 0) Then
                Dim ticks As IList(Of Double)


                ticks = Nothing
                YAxis.GetTickValues(Nothing, ticks, Nothing)

                For Each tick In ticks
                    ' Don't render a label for the origin.
                    If tick > 0 Then
                        Dim point As ScreenPoint
                        Dim text As String
                        Dim size As OxySize


                        text = tick.ToString("0")
                        point = Transform(XAxis.ActualMinimum, tick)

                        ' Don't draw the label if it's too close to the 
                        ' top, because it will overlap the day labels.
                        size = rc.MeasureText(text, Font, FontSize, FontWeight, 0)

                        ' Note that points start in the top-left, so the point needs 
                        ' to be _greater_ than our cutoff in order to be rendered.
                        If point.Y > (YAxis.Transform(YAxis.ActualMaximum) + (size.Height * 2)) Then
                            rc.DrawText(
                                New ScreenPoint(point.X + 2, point.Y),
                                text,
                                TextColor,
                                fontFamily:=Font,
                                fontSize:=FontSize,
                                fontWeight:=FontWeight,
                                horizontalAlignment:=HorizontalAlignment.Left,
                                verticalAlignment:=VerticalAlignment.Bottom
                            )
                        End If
                    End If
                Next tick
            End If
        End Sub

    End Class

End Namespace
