Imports OxyPlot
Imports OxyPlot.Annotations
Imports OxyPlot.Axes


Namespace Views

    Public Class DayBoundsAnnotation
        Inherits Annotation


        Private Const BaseDate As Date = #1970-01-01#
        Private Const HorizontalPadding As Double = 5.0
        Private Const VerticalPadding As Double = 3.0


        Private Shared ReadOnly DateFormats() As String = {
            "dddd, d MMM yyyy",
            "ddd, d MMM yyyy",
            "d MMM yyyy"
        }


        Public Property BoundaryColor As OxyColor


        Public Property AlternateFillColor As OxyColor


        Public Overrides Sub Render(rc As IRenderContext)
            Dim viewStartDate As Date
            Dim viewEndDate As Date
            Dim currentDay As Date
            Dim format As (DateFormat As String, StepSize As Integer)


            viewStartDate = DateTimeAxis.ToDateTime(XAxis.ActualMinimum).Date
            viewEndDate = DateTimeAxis.ToDateTime(XAxis.ActualMaximum).Date.AddDays(1)

            format = SelectLabelFormat(rc)
            currentDay = viewStartDate

            Do While currentDay < viewEndDate
                AddAnnotationsForDay(rc, currentDay, format.DateFormat, format.StepSize)
                currentDay = currentDay.AddDays(1)
            Loop
        End Sub


        Private Function SelectLabelFormat(rc As IRenderContext) As (DateFormat As String, StepSize As Integer)
            Dim format As String
            Dim stepSize As Integer
            Dim dayWidth As Double
            Dim size As OxySize
            Dim d As Date


            format = Nothing

            ' Work out how wide a day is on screen and subtract the padding to
            ' get the total available width that we can use for each date label.
            d = DateTimeAxis.ToDateTime(XAxis.ActualMinimum).Date
            dayWidth = XAxis.Transform(DateTimeAxis.ToDouble(d.AddDays(1))) - XAxis.Transform(DateTimeAxis.ToDouble(d))

            For Each f In DateFormats
                ' We'll use this format if it's small
                ' enough, or if it's the last one.
                format = f

                ' Use "Wednesday, March 25 2020" as the sample date
                ' because it's about the widest date you can get.
                size = rc.MeasureText(#2020-03-25#.ToString(f), Font, FontSize, FontWeight, 0)

                If (size.Width + (HorizontalPadding * 2)) < dayWidth Then
                    Exit For
                End If
            Next f

            ' Calculate how many days the label will span. We will use 
            ' that as the step size so that no two labels will overlap.
            If dayWidth > 0 Then
                stepSize = CInt(Math.Ceiling((size.Width + (HorizontalPadding * 2)) / dayWidth))
            Else
                stepSize = 1
            End If

            ' None of the formats are short enough, so just 
            ' use the last one because it's the shortest.
            Return (dateformat:=format, stepSize)
        End Function


        Private Sub AddAnnotationsForDay(
                rc As IRenderContext,
                currentDay As Date,
                dateFormat As String,
                labelStepSize As Integer
            )

            Dim bounds As OxyRect


            ' Get the bounds of the day in screen coordinates.
            bounds = New OxyRect(
                 Transform(DateTimeAxis.ToDouble(currentDay), 0),
                 Transform(DateTimeAxis.ToDouble(currentDay.AddDays(1)), 1)
            )

            ' Shade each alternate day.
            If (CInt(currentDay.Subtract(BaseDate).TotalDays) Mod 2) = 0 Then
                rc.DrawRectangle(bounds, AlternateFillColor, Nothing, thickness:=0)
            End If

            ' Add a darker line between this day and the previous day.
            rc.DrawLine(bounds.Left, bounds.Top, bounds.Left, bounds.Bottom, New OxyPen(BoundaryColor), aliased:=True)

            ' Add a label for the date if this day needs one.
            If (labelStepSize = 1) OrElse ((CInt(currentDay.Subtract(BaseDate).TotalDays) Mod labelStepSize) = 0) Then
                DrawDayLabel(rc, bounds, currentDay, dateFormat)
            End If
        End Sub


        Private Sub DrawDayLabel(
                rc As IRenderContext,
                bounds As OxyRect,
                currentDay As Date,
                dateFormat As String
            )

            Dim label As String
            Dim labelSize As OxySize
            Dim left As Double
            Dim overhang As Double


            label = currentDay.ToString(dateFormat)
            labelSize = rc.MeasureText(label, Font, FontSize, FontWeight, 0)

            ' The label will go at the start of the day. If the start of
            ' the day is off screen, start the label at the left of the
            ' screen, but don't let it go into the next day's bounds.
            left = Math.Max(0, bounds.Left) + HorizontalPadding
            overhang = (left + labelSize.Width + HorizontalPadding) - bounds.Right

            If overhang > 0 Then
                ' The label will overhang the next day, so push it 
                ' back so that it ends within the bounds of the day, 
                ' but don't push it back past the start of the day.
                left = Math.Max(left - overhang, bounds.Left + HorizontalPadding)
            End If

            rc.DrawText(
                New ScreenPoint(left, VerticalPadding),
                label,
                TextColor,
                fontFamily:=Font,
                fontSize:=FontSize,
                fontWeight:=FontWeight,
                horizontalAlignment:=HorizontalAlignment.Left,
                verticalAlignment:=VerticalAlignment.Top
            )
        End Sub

    End Class

End Namespace
