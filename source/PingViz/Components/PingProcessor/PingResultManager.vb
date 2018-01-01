Imports System.Reactive.Subjects


<RegisterAs({GetType(IPingResultEmitter), GetType(IPingResultSource)}, SingleInstance:=True)>
Public Class PingResultManager
    Implements IPingResultEmitter
    Implements IPingResultSource


    Private ReadOnly cgResults As Subject(Of PingResult)


    Public Sub New()
        cgResults = New Subject(Of PingResult)
    End Sub


    Public ReadOnly Property Results As IObservable(Of PingResult) _
        Implements IPingResultSource.Results

        Get
            Return cgResults
        End Get
    End Property


    Public Sub Emit(result As PingResult) _
        Implements IPingResultEmitter.Emit

        cgResults.OnNext(result)
    End Sub

End Class
