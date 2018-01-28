Imports OxyPlot
Imports OxyPlot.Series
Imports System.Globalization


Namespace Views

    Public Class DurationToStringConverterTests

        <Fact()>
        Public Sub ReturnsDurationAsMilliseconds()
            Dim result As TrackerHitResult



            result = New TrackerHitResult With {
                .DataPoint = New DataPoint(100, 123),
                .Series = New AreaSeries With {.YAxisKey = PlotViewModelBase.DurationAxisKey}
            }

            Assert.Equal(
                "123 ms",
                (New DurationToStringConverter).Convert(result, GetType(String), Nothing, CultureInfo.CurrentCulture)
            )
        End Sub


        <Fact()>
        Public Sub ReturnsTimeoutAsTimeout()
            Dim result As TrackerHitResult



            result = New TrackerHitResult With {
                .DataPoint = New DataPoint(100, 1),
                .Series = New AreaSeries With {.YAxisKey = PlotViewModelBase.TimeoutAxisKey}
            }

            Assert.Equal(
                "Timeout",
                (New DurationToStringConverter).Convert(result, GetType(String), Nothing, CultureInfo.CurrentCulture)
            )
        End Sub

    End Class

End Namespace
