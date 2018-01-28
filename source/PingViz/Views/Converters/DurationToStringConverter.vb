Imports OxyPlot
Imports OxyPlot.Series
Imports System.Globalization


Namespace Views

    Public Class DurationToStringConverter
        Implements IValueConverter


        Public Function Convert(
                value As Object,
                targetType As Type,
                parameter As Object,
                culture As CultureInfo
            ) As Object _
            Implements IValueConverter.Convert

            Dim result As TrackerHitResult


            result = DirectCast(value, TrackerHitResult)

            If String.Equals(TryCast(result.Series, XYAxisSeries)?.YAxisKey, PlotViewModelBase.TimeoutAxisKey) Then
                Return "Timeout"
            Else
                Return result.DataPoint.Y.ToString("0") & " ms"
            End If
        End Function


        Public Function ConvertBack(
                value As Object,
                targetType As Type,
                parameter As Object,
                culture As CultureInfo
            ) As Object _
            Implements IValueConverter.ConvertBack

            Return Binding.DoNothing
        End Function

    End Class

End Namespace
