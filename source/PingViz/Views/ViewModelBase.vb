Imports ReactiveUI


Namespace Views

    Public Class ViewModelBase
        Inherits ReactiveObject
        Implements IDisposable


        Private ReadOnly cgDisposables As List(Of IDisposable)
        Private cgOnLoadedCalled As Boolean


        Protected Sub New()
            cgDisposables = New List(Of IDisposable)
            LoadedCommand = ReactiveCommand.Create(AddressOf InternalOnLoadedAsync)
        End Sub


        Protected Sub Use(disposable As IDisposable)
            cgDisposables.Add(disposable)
        End Sub


        Private Async Function InternalOnLoadedAsync() As Task
            If Not cgOnLoadedCalled Then
                cgOnLoadedCalled = True
                Await OnLoadedAsync()
            End If
        End Function


        Public Overridable Function OnLoadedAsync() As Task
            Return Task.CompletedTask
        End Function


        Public ReadOnly Property LoadedCommand As ICommand


        Protected Overridable Sub Dispose(disposing As Boolean)
            For Each d In cgDisposables
                d.Dispose()
            Next d
        End Sub


        Public Sub Dispose() _
            Implements IDisposable.Dispose

            Dispose(True)
        End Sub

    End Class

End Namespace
