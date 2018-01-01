Imports System.Globalization


Namespace Views

    Public Class NullToCollapsedConverter
        Implements IValueConverter


        Public Function Convert(
                value As Object,
                targetType As Type,
                parameter As Object,
                culture As CultureInfo
            ) As Object _
            Implements IValueConverter.Convert

            If value Is Nothing Then
                Return Visibility.Collapsed
            Else
                Return Visibility.Visible
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
