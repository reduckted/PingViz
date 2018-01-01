<RegisterAs(GetType(ISettingsManager), SingleInstance:=True)>
Public Class SettingsManager
    Implements ISettingsManager


    Private cgPingAddress As String


    Public Sub New()
        cgPingAddress = My.Settings.PingAddress
    End Sub


    Public Property PingAddress As String _
        Implements ISettingsManager.PingAddress

        Get
            Return cgPingAddress
        End Get

        Set
            cgPingAddress = If(Value, String.Empty)
            My.Settings.PingAddress = cgPingAddress
        End Set
    End Property


    Public Sub Save() _
        Implements ISettingsManager.Save

        My.Settings.Save()
    End Sub

End Class
