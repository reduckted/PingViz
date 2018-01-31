Imports Autofac
Imports Hardcodet.Wpf.TaskbarNotification
Imports PingViz.Views


Class Application

    Public Shared ReadOnly PingInterval As TimeSpan = TimeSpan.FromSeconds(10)


    Private cgIcon As TaskbarIcon
    Private cgContainer As IContainer


    Protected Overrides Sub OnStartup(e As StartupEventArgs)
        Dim bootstrapper As Bootstrapper


        MyBase.OnStartup(e)

        bootstrapper = New Bootstrapper
        bootstrapper.Run()

        cgContainer = bootstrapper.Container

        ' Setup some development helpers. Show the main window
        ' if the `-show` argument is used, and exit when that
        ' window closes if the `-exit` argument is used.
        If e.Args.Contains("-show") Then
            cgContainer.Resolve(Of IWindowManager).ShowMainWindow()
        End If

        If e.Args.Contains("-exit") Then
            ShutdownMode = ShutdownMode.OnMainWindowClose
        End If
    End Sub


    Protected Overrides Sub OnExit(e As ExitEventArgs)
        cgContainer?.Dispose()
        MyBase.OnExit(e)
    End Sub

End Class
