Imports System.Reactive.Concurrency
Imports System.Reactive.Disposables
Imports System.Reactive.Linq
Imports System.Runtime.CompilerServices


Namespace Views

    Public Module ObservableHelpers

        <Extension()>
        Public Function ThrottleInWindow(Of T)(
                source As IObservable(Of T),
                maxTime As TimeSpan,
                scheduler As IScheduler
            ) As IObservable(Of T)

            Return Observable.Create(Of T)(
                Function(observer) New ThrottleInWindowSubscription(Of T)(observer, source, maxTime, scheduler)
            )
        End Function


        Private Class ThrottleInWindowSubscription(Of T)
            Implements IDisposable


            Private ReadOnly cgObserver As IObserver(Of T)
            Private ReadOnly cgDelayDisposable As SerialDisposable
            Private ReadOnly cgCompositeDisposable As IDisposable
            Private ReadOnly cgMaxTime As TimeSpan
            Private ReadOnly cgScheduler As IScheduler
            Private cgNextValue As T
            Private cgHasNextValue As Boolean
            Private cgIsHolding As Boolean


            Public Sub New(
                    observer As IObserver(Of T),
                    source As IObservable(Of T),
                    maxTime As TimeSpan,
                    scheduler As IScheduler
                )

                cgObserver = observer
                cgMaxTime = maxTime
                cgScheduler = scheduler

                cgDelayDisposable = New SerialDisposable

                cgCompositeDisposable = New CompositeDisposable(
                    source.Subscribe(AddressOf OnNext, AddressOf observer.OnError, AddressOf OnCompleted),
                    cgDelayDisposable
                )
            End Sub


            Private Sub OnNext(item As T)
                ' Record the item as the next item to be emitted.
                cgNextValue = item
                cgHasNextValue = True

                ' If we're *not* holding items,
                ' then we can emit this item now.
                If Not cgIsHolding Then
                    EmitIfHasValue()
                End If
            End Sub


            Private Sub OnCompleted()
                ' Emit the last value we received,
                ' then complete the observable.
                EmitIfHasValue()
                cgObserver.OnCompleted()
            End Sub


            Private Sub EmitIfHasValue()
                If cgHasNextValue Then
                    ' Flag that we don't have a value any more before 
                    ' we emit the value in case we receive a new value
                    ' as a result of emitting the current value.
                    cgHasNextValue = False

                    ' Hold any future values that we receive so that we don't 
                    ' emit more than one value within the maximum time.
                    Hold()

                    cgObserver.OnNext(cgNextValue)
                End If
            End Sub


            Private Sub Hold()
                ' Set the "holding" flag and schedule the "release" method
                ' to be called after the maximum time has passed.
                cgIsHolding = True
                cgDelayDisposable.Disposable = cgScheduler.Schedule(cgMaxTime, AddressOf Release)
            End Sub


            Private Sub Release()
                ' Turn off the "holding" flag and get rid of the disposable 
                ' for the delay because we don't need it any more.
                cgIsHolding = False
                cgDelayDisposable.Disposable = Nothing

                ' Now that we have stopped holding values,
                ' we can emit the latest value if there is one.
                EmitIfHasValue()
            End Sub


            Public Sub Dispose() _
                Implements IDisposable.Dispose

                cgCompositeDisposable.Dispose()
            End Sub

        End Class

    End Module

End Namespace
