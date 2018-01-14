Imports ReactiveUI
Imports System.Reactive.Concurrency


Namespace Views

    Public Class ViewModelBase
        Inherits ReactiveObject
        Implements IDisposable


        Private ReadOnly cgDisposables As List(Of IDisposable)
        Private cgOnLoadedCalled As Boolean
        Private cgIsLoading As Integer


        Protected Sub New(scheduler As IScheduler)
            cgDisposables = New List(Of IDisposable)

            LoadedCommand = ReactiveCommand.CreateFromTask(AddressOf OnLoadedAsync, outputScheduler:=scheduler)

            SetIsLoading(True)
        End Sub


        Protected Sub Use(disposable As IDisposable)
            cgDisposables.Add(disposable)
        End Sub


        Public Async Function OnLoadedAsync() As Task
            If Not cgOnLoadedCalled Then
                cgOnLoadedCalled = True
                Await LoadAsync()
                SetIsLoading(False)
            End If
        End Function


        Protected Overridable Function LoadAsync() As Task
            Return Task.CompletedTask
        End Function


        Public ReadOnly Property IsLoading As Boolean
            Get
                Return cgIsLoading > 0
            End Get
        End Property


        Protected Sub SetIsLoading(value As Boolean)
            If value Then
                cgIsLoading += 1

                If cgIsLoading = 1 Then
                    RaisePropertyChanged(NameOf(IsLoading))
                End If

            Else
                cgIsLoading -= 1

                If cgIsLoading = 0 Then
                    RaisePropertyChanged(NameOf(IsLoading))
                End If
            End If
        End Sub


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
