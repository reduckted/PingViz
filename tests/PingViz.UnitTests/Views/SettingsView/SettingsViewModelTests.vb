Imports System.ComponentModel


Namespace Views

    Public Class SettingsViewModelTests

        Public Class PingAddressProperty

            <Fact()>
            Public Sub IsInitializedToValueFromSettings()
                Dim vm As SettingsViewModel
                Dim manager As Mock(Of ISettingsManager)


                manager = New Mock(Of ISettingsManager)
                manager.SetupGet(Function(x) x.PingAddress).Returns("foo")

                vm = New SettingsViewModel(manager.Object)

                Assert.Equal(vm.PingAddress, "foo")
            End Sub


            <Fact()>
            Public Sub CanGetAndSetProperty()
                Dim vm As SettingsViewModel
                Dim manager As Mock(Of ISettingsManager)


                manager = New Mock(Of ISettingsManager)
                manager.SetupGet(Function(x) x.PingAddress).Returns("")

                vm = New SettingsViewModel(manager.Object)

                Assert.Equal(vm.PingAddress, "")

                vm.PingAddress = "abc"

                Assert.Equal(vm.PingAddress, "abc")
            End Sub


            <Fact()>
            Public Sub RaisesPropertyChangedEventWhenChanged()
                Dim vm As SettingsViewModel
                Dim manager As Mock(Of ISettingsManager)
                Dim args As PropertyChangedEventArgs


                manager = New Mock(Of ISettingsManager)
                manager.SetupGet(Function(x) x.PingAddress).Returns("")

                vm = New SettingsViewModel(manager.Object)
                args = Nothing

                AddHandler vm.PropertyChanged, Sub(s, e) args = e

                vm.PingAddress = "bar"

                Assert.NotNull(args)
                Assert.Equal(NameOf(vm.PingAddress), args.PropertyName)
            End Sub

        End Class


        Public Class OnClosedMethod

            <Fact()>
            Public Sub SavesTheSettings()
                Dim vm As SettingsViewModel
                Dim manager As Mock(Of ISettingsManager)


                manager = New Mock(Of ISettingsManager)
                manager.SetupGet(Function(x) x.PingAddress).Returns("foo")

                vm = New SettingsViewModel(manager.Object) With {
                    .PingAddress = "bar"
                }

                DirectCast(vm, IFlyoutViewModel).OnClosed()

                manager.VerifySet(Sub(x) x.PingAddress = "bar", Times.Once)
                manager.Verify(Sub(x) x.Save(), Times.Once)
            End Sub

        End Class

    End Class

End Namespace
