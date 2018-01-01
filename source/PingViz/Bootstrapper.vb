Imports Autofac
Imports Microsoft.EntityFrameworkCore
Imports Prism.Mvvm
Imports Prism.Regions
Imports System.Reactive.Concurrency
Imports System.Reflection


Public Class Bootstrapper
    Inherits Prism.Autofac.AutofacBootstrapper


    Public Overrides Async Sub Run(runWithDefaultConfiguration As Boolean)
        MyBase.Run(runWithDefaultConfiguration)

        Await MigrateDatabaseAsync()
        RegisterViews()

        ' Now that the database has been migrated, we can start the lifetime services. We need 
        ' to create all of the services first (`.ToList()` will do that), because we want to 
        ' make sure that all of the services are initialized before any of them are started.
        For Each service In Container.Resolve(Of IEnumerable(Of ILifetimeService)).ToList()
            Await service.StartAsync()
        Next service
    End Sub


    Protected Overrides Sub ConfigureContainerBuilder(builder As ContainerBuilder)
        MyBase.ConfigureContainerBuilder(builder)
        ServiceRegistrator.RegisterServices(builder, Assembly.GetExecutingAssembly().GetTypes())
        builder.RegisterInstance(DispatcherScheduler.Current).As(Of IScheduler)()
    End Sub


    Protected Overrides Sub ConfigureViewModelLocator()
        MyBase.ConfigureViewModelLocator()

        ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(
            Function(viewType)
                Dim viewName As String
                Dim viewAssemblyName As String
                Dim suffix As String
                Dim viewModelName As String


                viewName = viewType.FullName
                viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName

                suffix = If(viewName.EndsWith("View"), "Model", "ViewModel")
                viewModelName = $"{viewName}{suffix}, {viewAssemblyName}"

                Return Type.GetType(viewModelName)
            End Function
        )
    End Sub


    Protected Overrides Function ConfigureRegionAdapterMappings() As RegionAdapterMappings
        Dim mappings As RegionAdapterMappings


        mappings = MyBase.ConfigureRegionAdapterMappings()

        For Each type In GetTypesWithAttribute(Of RegionAdapterAttribute)()
            mappings.RegisterMapping(type.Attribute.ControlType, DirectCast(Container.Resolve(type.Type), IRegionAdapter))
        Next type

        Return mappings
    End Function


    Private Async Function MigrateDatabaseAsync() As Task
        Using scope = Container.BeginLifetimeScope()
            Using db = TryCast(scope.Resolve(Of IDatabase), Database)
                If db IsNot Nothing Then
                    Await db.Database.MigrateAsync()
                End If
            End Using
        End Using
    End Function


    Private Sub RegisterViews()
        Dim registry As IRegionViewRegistry


        registry = Container.Resolve(Of IRegionViewRegistry)

        For Each type In GetTypesWithAttribute(Of RegionAttribute)()
            registry.RegisterViewWithRegion(type.Attribute.Name, type.Type)
        Next type
    End Sub


    Private Shared Function GetTypesWithAttribute(Of T As Attribute)() As IEnumerable(Of (Type As Type, Attribute As T))
        Return (
            From type In Assembly.GetExecutingAssembly().GetTypes()
            Let attribute = type.GetCustomAttribute(Of T)
            Where attribute IsNot Nothing
            Select (type:=type, attribute:=attribute)
        )
    End Function

End Class
