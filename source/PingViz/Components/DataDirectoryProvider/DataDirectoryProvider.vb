#If Not DEBUG Then
Imports System.IO


<RegisterAs(GetType(IDataDirectoryProvider), SingleInstance:=True)>
Public Class DataDirectoryProvider
    Implements IDataDirectoryProvider


    Private ReadOnly cgDirectory As String


    Public Sub New()
        cgDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            My.Application.Info.Title
        )
    End Sub


    Public Function GetDirectory() As String _
        Implements IDataDirectoryProvider.GetDirectory

        Return cgDirectory
    End Function

End Class
#End If
