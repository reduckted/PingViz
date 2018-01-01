Imports OxyPlot.Axes
Imports System.Globalization


Namespace Views

    Public Class DoubleToTimeConverter
        Implements IValueConverter


        Public Function Convert(
                value As Object,
                targetType As Type,
                parameter As Object,
                culture As CultureInfo
            ) As Object _
            Implements IValueConverter.Convert

            Return DateTimeAxis.ToDateTime(CDbl(value)).ToString("HH:mm:ss")
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
