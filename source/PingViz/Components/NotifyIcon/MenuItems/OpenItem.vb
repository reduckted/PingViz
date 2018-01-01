Imports PingViz.Views


Namespace NotifyIcon.MenuItems

    <RegisterAs(GetType(IMenuItem))>
    Public Class OpenItem
        Inherits ItemBase


        Private ReadOnly cgWindowManager As IWindowManager


        Public Sub New(windowManager As IWindowManager)
            If windowManager Is Nothing Then
                Throw New ArgumentNullException(NameOf(windowManager))
            End If

            cgWindowManager = windowManager
        End Sub


        Public Overrides ReadOnly Property IsStartOfGroup As Boolean
            Get
                Return False
            End Get
        End Property


        Public Overrides ReadOnly Property Header As String
            Get
                Return "Open"
            End Get
        End Property


        Public Overrides ReadOnly Property Order As Integer
            Get
                Return 1
            End Get
        End Property


        Public Overrides Sub Execute(parameter As Object)
            cgWindowManager.ShowMainWindow()
        End Sub

    End Class

End Namespace
