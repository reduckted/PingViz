#If DEBUG Then
Imports System.IO


<RegisterAs(GetType(IDataDirectoryProvider), SingleInstance:=True)>
Public Class DebugDataDirectoryProvider
    Implements IDataDirectoryProvider


    Private ReadOnly cgDirectory As String


    Public Sub New()
        cgDirectory = Path.Combine(FindSolutionDirectory(), "data")
    End Sub


    Public Function GetDirectory() As String _
        Implements IDataDirectoryProvider.GetDirectory

        Return cgDirectory
    End Function


    Private Shared Function FindSolutionDirectory() As String
        Dim dir As String


        dir = Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location)

        Do
            If Directory.EnumerateFiles(dir, "*.sln").Any() Then
                Return dir
            End If

            dir = Path.GetDirectoryName(dir)

            If String.IsNullOrEmpty(dir) Then
                Throw New InvalidOperationException("Could not find the solution directory.")
            End If
        Loop
    End Function

End Class
#End If
