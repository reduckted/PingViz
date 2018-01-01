Imports System.IO
Imports System.Reflection


<RegisterAs(GetType(ILicenseProvider))>
Public Class LicenseProvider
    Implements ILicenseProvider


    Public Iterator Function GetLicenses() As IEnumerable(Of OpenSourceLicense) _
        Implements ILicenseProvider.GetLicenses

        Yield LoadLicense("Autofac", "https://github.com/autofac/Autofac")
        Yield LoadLicense("CommonServiceLocator", "https://github.com/unitycontainer/commonservicelocator")
        Yield LoadLicense("HardCodet.NotifyIcon.Wpf", "https://bitbucket.org/hardcodet/notifyicon-wpf/src")
        Yield LoadLicense("MahApps.Metro", "https://github.com/MahApps/MahApps.Metro")
        Yield LoadLicense("MaterialDesign", "https://github.com/Templarian/MaterialDesign")
        Yield LoadLicense("Microsoft.CSharp", "https://github.com/dotnet/corefx")
        Yield LoadLicense("Microsoft.Data.Sqlite.Core", "https://github.com/aspnet/Microsoft.Data.Sqlite")
        Yield LoadLicense("Microsoft.EntityFrameworkCore", "https://github.com/aspnet/EntityFrameworkCore")
        Yield LoadLicense("Microsoft.Extensions.Caching", "https://github.com/aspnet/Caching")
        Yield LoadLicense("Microsoft.Extensions.Configuration", "https://github.com/aspnet/Configuration")
        Yield LoadLicense("Microsoft.Extensions.DependencyInjection", "https://github.com/aspnet/DependencyInjection")
        Yield LoadLicense("Microsoft.Extensions.Logging", "https://github.com/aspnet/Logging")
        Yield LoadLicense("Microsoft.Extensions.Options", "https://github.com/aspnet/Options")
        Yield LoadLicense("Microsoft.Extensions.Primitives", "https://github.com/aspnet/Common")
        Yield LoadLicense("Oxyplot", "https://github.com/oxyplot/oxyplot")
        Yield LoadLicense("Prism", "https://github.com/PrismLibrary/Prism")
        Yield LoadLicense("Quartz", "https://github.com/quartznet/quartznet")
        Yield LoadLicense("ReactiveUI", "https://github.com/reactiveui/reactiveui")
        Yield LoadLicense("Remotion.Linq", "https://github.com/re-motion/Relinq")
        Yield LoadLicense("Splat", "https://github.com/reactiveui/splat")
        Yield LoadLicense("SQLitePCLRaw", "https://github.com/ericsink/SQLitePCL.raw")
        Yield LoadLicense("System.Reactive", "https://github.com/Reactive-Extensions/Rx.NET")
    End Function


    Private Function LoadLicense(
            name As String,
            url As String
        ) As OpenSourceLicense

        Using reader As New StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"PingViz.{name}.txt"))
            Return New OpenSourceLicense(name, url, reader.ReadToEnd())
        End Using
    End Function

End Class
