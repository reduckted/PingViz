Imports System.Globalization
Imports System.Windows
Imports System.Windows.Data


Namespace Views

    Public Class NullToCollapsedConverterTests

        Public Class ConvertMethod

            <Fact()>
            Public Sub ReturnsCollapsedForNullValue()
                Assert.Equal(
                    Visibility.Collapsed,
                    (New NullToCollapsedConverter).Convert(Nothing, GetType(Visibility), Nothing, CultureInfo.CurrentCulture)
                )
            End Sub


            <Fact()>
            Public Sub ReturnsVisibleForNonNullValue()
                Assert.Equal(
                    Visibility.Visible,
                    (New NullToCollapsedConverter).Convert(1, GetType(Visibility), Nothing, CultureInfo.CurrentCulture)
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
