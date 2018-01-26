Imports Microsoft.Reactive.Testing


Namespace Views

    Public Class ObservableHelpersTests

        Public Class ThrottleInWindowTests
            Inherits ReactiveTest


            <Fact()>
            Public Sub EmitsTheFirstItemImmediately()
                Dim scheduler As TestScheduler
                Dim results As ITestableObserver(Of Integer)
                Dim source As ITestableObservable(Of Integer)


                scheduler = New TestScheduler

                source = scheduler.CreateHotObservable(
                    OnNext(250, 1)
                )

                results = scheduler.Start(
                    Function() source.ThrottleInWindow(TimeSpan.FromTicks(1000), scheduler)
                )

                results.Messages.AssertEqual(
                    OnNext(250, 1)
                )
            End Sub


            <Fact()>
            Public Sub EmitsSecondItemAfterMaxTimeWhenItOccursWithinWindow()
                Dim scheduler As TestScheduler
                Dim results As ITestableObserver(Of Integer)
                Dim source As IObservable(Of Integer)


                scheduler = New TestScheduler

                source = scheduler.CreateHotObservable(
                    OnNext(250, 1),
                    OnNext(500, 2)
                )

                results = scheduler.Start(
                    Function() source.ThrottleInWindow(TimeSpan.FromTicks(1000), scheduler),
                    10000
                )

                results.Messages.AssertEqual(
                    OnNext(250, 1),
                    OnNext(1250, 2)
                )
            End Sub


            <Fact()>
            Public Sub OnlyEmitsLatestItemWhenMultipleItemsOccurDuringWindow()
                Dim scheduler As TestScheduler
                Dim results As ITestableObserver(Of Integer)
                Dim source As IObservable(Of Integer)


                scheduler = New TestScheduler

                source = scheduler.CreateHotObservable(
                    OnNext(250, 1),
                    OnNext(300, 2),
                    OnNext(350, 3),
                    OnNext(450, 4)
                )

                results = scheduler.Start(
                    Function() source.ThrottleInWindow(TimeSpan.FromTicks(250), scheduler),
                    10000
                )

                results.Messages.AssertEqual(
                    OnNext(250, 1),
                    OnNext(500, 4)
                )
            End Sub


            <Fact()>
            Public Sub EmitsFirstItemThatOccursAfterWindowEndsImmediately()
                Dim scheduler As TestScheduler
                Dim results As ITestableObserver(Of Integer)
                Dim source As IObservable(Of Integer)


                scheduler = New TestScheduler

                source = scheduler.CreateHotObservable(
                    OnNext(250, 1),
                    OnNext(750, 2)
                )

                results = scheduler.Start(
                    Function() source.ThrottleInWindow(TimeSpan.FromTicks(250), scheduler),
                    10000
                )

                results.Messages.AssertEqual(
                    OnNext(250, 1),
                    OnNext(750, 2)
                )
            End Sub


            <Fact()>
            Public Sub RestartsWindowWhenItFinishes()
                Dim scheduler As TestScheduler
                Dim results As ITestableObserver(Of Integer)
                Dim source As IObservable(Of Integer)


                scheduler = New TestScheduler

                source = scheduler.CreateHotObservable(
                    OnNext(250, 1),
                    OnNext(350, 2),
                    OnNext(450, 3),
                    OnNext(550, 4),
                    OnNext(650, 5),
                    OnNext(750, 6),
                    OnNext(850, 7),
                    OnNext(950, 8)
                )

                results = scheduler.Start(
                    Function() source.ThrottleInWindow(TimeSpan.FromTicks(300), scheduler),
                    10000
                )

                results.Messages.AssertEqual(
                    OnNext(250, 1),
                    OnNext(550, 4),
                    OnNext(850, 7),
                    OnNext(1150, 8)
                )
            End Sub


            <Fact()>
            Public Sub EmitsFinalItemWhenSourceIsCompleted()
                Dim scheduler As TestScheduler
                Dim results As ITestableObserver(Of Integer)
                Dim source As IObservable(Of Integer)


                scheduler = New TestScheduler

                source = scheduler.CreateHotObservable(
                    OnNext(250, 1),
                    OnNext(300, 2),
                    OnCompleted(Of Integer)(350)
                )

                results = scheduler.Start(
                    Function() source.ThrottleInWindow(TimeSpan.FromTicks(250), scheduler),
                    10000
                )

                results.Messages.AssertEqual(
                    OnNext(250, 1),
                    OnNext(350, 2),
                    OnCompleted(Of Integer)(350)
                )
            End Sub


            <Fact()>
            Public Sub DoesNotEmitAfterBeingDisposed()
                Dim scheduler As TestScheduler
                Dim results As ITestableObserver(Of Integer)
                Dim source As ITestableObservable(Of Integer)


                scheduler = New TestScheduler

                source = scheduler.CreateHotObservable(
                    OnNext(250, 1),
                    OnNext(500, 2)
                )

                results = scheduler.Start(
                    Function() source.ThrottleInWindow(TimeSpan.FromTicks(500), scheduler),
                    600
                )

                results.Messages.AssertEqual(
                    OnNext(250, 1)
                )
            End Sub

        End Class

    End Class

End Namespace
