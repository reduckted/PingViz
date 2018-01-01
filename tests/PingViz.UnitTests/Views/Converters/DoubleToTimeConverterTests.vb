Imports OxyPlot.Axes
Imports System.Globalization
Imports System.Windows
Imports System.Windows.Data


Namespace Views

    Public Class DoubleToTimeConverterTests

        Public Class ConvertMethod

            <Fact()>
            Public Sub ReturnsFormattedTime()
                Assert.Equal(
                    "12:34:56",
                    (New DoubleToTimeConverter).Convert(DateTimeAxis.ToDouble(#2001-01-01 12:34:56#), GetType(Visibility), Nothing, CultureInfo.CurrentCulture)
                )
            End Sub

        End Class


        Public Class ConvertBackMethod

            <Fact()>
            Public Sub ReturnsDoNothing()
                Assert.Same(
                    Binding.DoNothing,
                    (New NullToCollapsedConverter).ConvertBack(Nothing, GetType(Visibility), Nothing, CultureInfo.CurrentCulture)
                )
            End Sub

        End Class

    End Class

End Namespace
