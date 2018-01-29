Imports System.IO
Imports System.Reactive.Subjects
Imports System.Text


<RegisterAs({GetType(IErrorHandler), GetType(IErrorSource)}, SingleInstance:=True)>
Public Class ErrorHandler
    Implements IErrorHandler
    Implements IErrorSource


    Private Shared ReadOnly LogFileEncoding As New UTF8Encoding(False)


    Private ReadOnly cgDataDirectoryProvider As IDataDirectoryProvider
    Private ReadOnly cgErrors As Subject(Of String)
    Private ReadOnly cgFileLock As Object


    Public Sub New(dataDirectoryProvider As IDataDirectoryProvider)
        cgDataDirectoryProvider = dataDirectoryProvider
        cgFileLock = New Object
        cgErrors = New Subject(Of String)
        Errors = cgErrors
    End Sub


    Public ReadOnly Property Errors As IObservable(Of String) _
        Implements IErrorSource.Errors


    Public Sub Handle(message As String) _
        Implements IErrorHandler.Handle

        WriteErrorToLog(message)
        cgErrors.OnNext(message)
    End Sub


    Private Sub WriteErrorToLog(message As String)
        Try
            Dim dir As String
            Dim fileName As String


            dir = cgDataDirectoryProvider.GetDirectory()
            Directory.CreateDirectory(dir)

            fileName = Path.Combine(dir, "errors.log")

            ' Lock file access to avoid potential errors from
            ' trying to write multiple errors to it at the same time.
            SyncLock cgFileLock
                File.AppendAllLines(
                    fileName,
                    {$"[{Date.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {message}"},
                    LogFileEncoding
                )
            End SyncLock

        Catch ex As Exception
            Debug.WriteLine($"Failed to write error to log file: {ex.Message}")
        End Try
    End Sub

End Class
