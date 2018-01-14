Imports MahApps.Metro.Controls
Imports ReactiveUI
Imports System.Reactive.Concurrency
Imports System.Reactive.Linq

Namespace Views

    Public Class AboutViewModel
        Inherits ViewModelBase
        Implements IFlyoutViewModel


        Private ReadOnly cgUrlOpener As IUrlOpener
        Private cgSelectedLicense As OpenSourceLicense


        Public Sub New(
                scheduler As IScheduler,
                licenseProvider As ILicenseProvider,
                urlOpener As IUrlOpener
            )

            MyBase.New(scheduler)

            cgUrlOpener = urlOpener

            Licenses = licenseProvider.GetLicenses().ToList()
            cgSelectedLicense = Licenses.FirstOrDefault()

            OpenUrlCommand = ReactiveCommand.Create(Of String)(AddressOf OpenUrl, outputScheduler:=DispatcherScheduler.Current)
        End Sub


        Public ReadOnly Property Header As String _
            Implements IFlyoutViewModel.Header

            Get
                Return "About"
            End Get
        End Property


        Public ReadOnly Property Position As Position _
            Implements IFlyoutViewModel.Position

            Get
                Return Position.Right
            End Get
        End Property


        Private Sub OnOpened() _
            Implements IFlyoutViewModel.OnOpened

            ' Nothing to do here.
        End Sub


        Private Sub OnClosed() _
            Implements IFlyoutViewModel.OnClosed

            ' Nothing to do here.
        End Sub


        Public ReadOnly Property Title As String
            Get
                Return My.Application.Info.Title
            End Get
        End Property


        Public ReadOnly Property Version As String
            Get
                Return My.Application.Info.Version.ToString()
            End Get
        End Property


        Public ReadOnly Property ProjectUrl As String
            Get
                Return "https://github.com/reduckted/PingViz"
            End Get
        End Property


        Public ReadOnly Property Licenses As IEnumerable(Of OpenSourceLicense)


        Public Property SelectedLicense As OpenSourceLicense
            Get
                Return cgSelectedLicense
            End Get

            Set
                RaiseAndSetIfChanged(cgSelectedLicense, Value)
            End Set
        End Property


        Public ReadOnly Property OpenUrlCommand As ICommand


        Private Sub OpenUrl(url As String)
            cgUrlOpener.Open(url)
        End Sub

    End Class

End Namespace
