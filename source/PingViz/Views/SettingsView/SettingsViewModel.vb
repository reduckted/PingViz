Imports MahApps.Metro.Controls
Imports ReactiveUI
Imports System.Reactive.Concurrency


Namespace Views

    Public Class SettingsViewModel
        Inherits ViewModelBase
        Implements IFlyoutViewModel


        Private ReadOnly cgSettingsManager As ISettingsManager
        Private cgPingAddress As String


        Public Sub New(
                scheduler As IScheduler,
                settingsManager As ISettingsManager
            )

            MyBase.New(scheduler)

            If settingsManager Is Nothing Then
                Throw New ArgumentNullException(NameOf(settingsManager))
            End If

            cgSettingsManager = settingsManager
            cgPingAddress = settingsManager.PingAddress
        End Sub


        Public ReadOnly Property Header As String _
            Implements IFlyoutViewModel.Header

            Get
                Return "Settings"
            End Get
        End Property


        Public ReadOnly Property Position As Position _
            Implements IFlyoutViewModel.Position

            Get
                Return Position.Right
            End Get
        End Property


        Public Property PingAddress As String
            Get
                Return cgPingAddress
            End Get

            Set
                RaiseAndSetIfChanged(cgPingAddress, Value)
            End Set
        End Property


        Private Sub OnOpened() _
            Implements IFlyoutViewModel.OnOpened

            ' Nothing to do here.
        End Sub


        Private Sub OnClosed() _
            Implements IFlyoutViewModel.OnClosed

            cgSettingsManager.PingAddress = cgPingAddress
            cgSettingsManager.Save()
        End Sub

    End Class

End Namespace
