Imports Hardcodet.Wpf.TaskbarNotification
Imports PingViz.Views
Imports System.Reactive.Linq


Namespace NotifyIcon

    <RegisterAs(GetType(ILifetimeService), SingleInstance:=True)>
    Public Class IconController
        Implements ILifetimeService


        Private ReadOnly cgIcon As TaskbarIcon
        Private ReadOnly cgWindowManager As IWindowManager


        Public Sub New(
                icon As TaskbarIcon,
                menuProvider As IMenuProvider,
                windowManager As IWindowManager,
                resultsSource As IPingResultSource
            )

            If icon Is Nothing Then
                Throw New ArgumentNullException(NameOf(icon))
            End If

            If menuProvider Is Nothing Then
                Throw New ArgumentNullException(NameOf(menuProvider))
            End If

            If windowManager Is Nothing Then
                Throw New ArgumentNullException(NameOf(windowManager))
            End If

            If resultsSource Is Nothing Then
                Throw New ArgumentNullException(NameOf(resultsSource))
            End If


            cgIcon = icon
            cgWindowManager = windowManager

            cgIcon.ToolTipText = My.Application.Info.Title
            cgIcon.Icon = My.Resources.NotifyIcon
            cgIcon.ContextMenu = menuProvider.GetContextMenu()

            AddHandler cgIcon.TrayMouseDoubleClick, AddressOf OnDoubleClick

            ' Update the tooltip with the current ping's duration.
            resultsSource _
                .Results _
                .Select(Function(result) result.Duration.ToMilliseconds()) _
                .Select(Function(ms) If(ms.HasValue, $"{ms.Value} ms", "Timed out")) _
                .ObserveOnDispatcher() _
                .Subscribe(Sub(text) cgIcon.ToolTipText = $"{ My.Application.Info.Title}: {text}")

            ' Change the icon when a timeout occurs.
            resultsSource _
                .Results _
                .Select(Function(result) result.Duration.HasValue) _
                .DistinctUntilChanged() _
                .ObserveOnDispatcher() _
                .Select(Function(success) If(success, My.Resources.NotifyIcon, My.Resources.NotifyIconTimedOut)) _
                .Subscribe(Sub(x) cgIcon.Icon = x)
        End Sub


        Public Function StartAsync() As Task _
            Implements ILifetimeService.StartAsync

            Return Task.CompletedTask
        End Function


        Private Sub OnDoubleClick(
                sender As Object,
                e As RoutedEventArgs
            )

            cgWindowManager.ShowMainWindow()
        End Sub

    End Class

End Namespace
