Imports OxyPlot
Imports OxyPlot.Annotations
Imports OxyPlot.Axes


Namespace Views

    Public Class TimeAxisLabelAnnotation
        Inherits Annotation


        Public Overrides Sub Render(rc As IRenderContext)
            If (XAxis.ActualMinorStep > 0) AndAlso (XAxis.ActualMajorStep > 0) Then
                Dim ticks As IList(Of Double)


                ticks = Nothing
                XAxis.GetTickValues(Nothing, ticks, Nothing)

                For Each tick In ticks
                    Dim point As ScreenPoint


                    point = Transform(tick, 0)

                    rc.DrawText(
                        New ScreenPoint(point.X + 3, point.Y),
                        DateTimeAxis.ToDateTime(tick).ToString("HH:mm"),
                        TextColor,
                        fontFamily:=Font,
                        fontSize:=FontSize,
                        fontWeight:=FontWeight,
                        horizontalAlignment:=HorizontalAlignment.Left,
                        verticalAlignment:=VerticalAlignment.Bottom
                    )
                Next tick
            End If
        End Sub

    End Class

End Namespace
