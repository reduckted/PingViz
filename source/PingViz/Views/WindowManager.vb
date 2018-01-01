Imports Autofac
Imports Prism.Regions


Namespace Views

    <RegisterAs(GetType(IWindowManager), SingleInstance:=True)>
    Public Class WindowManager
        Implements IWindowManager


        Private ReadOnly cgRootScope As ILifetimeScope
        Private cgWindowScope As ILifetimeScope
        Private cgMainWindow As MainWindow


        Public Sub New(rootScope As ILifetimeScope)
            If rootScope Is Nothing Then
                Throw New ArgumentNullException(NameOf(rootScope))
            End If

            cgRootScope = rootScope
        End Sub


        Public Sub ShowMainWindow() _
            Implements IWindowManager.ShowMainWindow

            If cgMainWindow Is Nothing Then
                ' Create the window in a new scope.
                cgWindowScope = cgRootScope.BeginLifetimeScope()
                cgMainWindow = cgWindowScope.Resolve(Of MainWindow)

                AddHandler cgMainWindow.Closed, AddressOf OnMainWindowClosed

                cgMainWindow.Show()

            Else
                cgMainWindow.BringIntoView()
                cgMainWindow.Activate()
            End If
        End Sub


        Private Sub OnMainWindowClosed(
                sender As Object,
                e As EventArgs
            )

            RemoveHandler cgMainWindow.Closed, AddressOf OnMainWindowClosed

            ' The regions are not automatically removed when the
            ' window is closed, whicc will cause exceptions if we
            ' try to open the window again. Remove them manually.
            RemoveRegions()

            cgWindowScope.Dispose()
            cgWindowScope = Nothing
            cgMainWindow = Nothing
        End Sub


        Private Sub RemoveRegions()
            Dim regionManager As IRegionManager


            regionManager = cgWindowScope.Resolve(Of IRegionManager)()

            For Each r In regionManager.Regions.ToList()
                regionManager.Regions.Remove(r.Name)
            Next r
        End Sub

    End Class

End Namespace
