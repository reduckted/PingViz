Imports ReactiveUI
Imports System.Reactive.Concurrency
Imports System.Reactive.Linq


Namespace Views

    Public Class ErrorViewModel
        Inherits ViewModelBase


        Private cgError As String


        Public Sub New(
                scheduler As IScheduler,
                errorSource As IErrorSource
            )

            MyBase.New(scheduler)

            Use(errorSource.Errors.ObserveOn(scheduler).Subscribe(Sub(x) [Error] = x))
            ClearErrorCommand = ReactiveCommand.Create(Sub() [Error] = Nothing, outputScheduler:=scheduler)
        End Sub


        Public Property [Error] As String
            Get
                Return cgError
            End Get

            Private Set
                RaiseAndSetIfChanged(cgError, Value)
            End Set
        End Property


        Public ReadOnly Property ClearErrorCommand As ReactiveCommand

    End Class

End Namespace
