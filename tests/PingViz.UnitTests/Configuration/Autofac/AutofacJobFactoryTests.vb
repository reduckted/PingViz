Imports Autofac
Imports Autofac.Core
Imports Quartz
Imports Quartz.Spi


Public Class AutofacJobFactoryTests

    <Fact()>
    Public Sub CreatesJobInNewScope()
        Dim factory As AutofacJobFactory
        Dim rootScope As Mock(Of ILifetimeScope)
        Dim childScope As Mock(Of ILifetimeScope)


        childScope = New Mock(Of ILifetimeScope)
        ConfigureChildScope(childScope)

        rootScope = New Mock(Of ILifetimeScope)
        rootScope.Setup(Function(x) x.BeginLifetimeScope()).Returns(childScope.Object)

        factory = New AutofacJobFactory(rootScope.Object)
        factory.NewJob(CreateTriggerFiredBundle(), Mock.Of(Of IScheduler))

        childScope.Verify(
            Function(x) x.ResolveComponent(It.IsAny(Of IComponentRegistration), It.IsAny(Of IEnumerable(Of Parameter))),
            Times.Once
        )
    End Sub


    <Fact()>
    Public Sub DisposesScopeWhenJobIsReturned()
        Dim factory As AutofacJobFactory
        Dim rootScope As Mock(Of ILifetimeScope)
        Dim childScope As Mock(Of ILifetimeScope)
        Dim job As IJob


        childScope = New Mock(Of ILifetimeScope)
        ConfigureChildScope(childScope)

        rootScope = New Mock(Of ILifetimeScope)
        rootScope.Setup(Function(x) x.BeginLifetimeScope()).Returns(childScope.Object)

        factory = New AutofacJobFactory(rootScope.Object)

        job = factory.NewJob(CreateTriggerFiredBundle(), Mock.Of(Of IScheduler))
        childScope.Verify(Sub(x) x.Dispose(), Times.Never)

        factory.ReturnJob(job)
        childScope.Verify(Sub(x) x.Dispose(), Times.Once)
    End Sub


    Private Shared Sub ConfigureChildScope(scope As Mock(Of ILifetimeScope))
        Dim registry As Mock(Of IComponentRegistry)
        Dim registration As IComponentRegistration
        Dim job As IJob


        job = Mock.Of(Of IJob)

        registration = Mock.Of(Of IComponentRegistration)

        registry = New Mock(Of IComponentRegistry)
        registry.Setup(Function(x) x.TryGetRegistration(It.IsAny(Of Service), registration)).Returns(True)

        scope.SetupGet(Function(x) x.ComponentRegistry).Returns(registry.Object)
        scope.Setup(Function(x) x.ResolveComponent(It.IsAny(Of IComponentRegistration), It.IsAny(Of IEnumerable(Of Parameter)))).Returns(job)
    End Sub


    Private Shared Function CreateTriggerFiredBundle() As TriggerFiredBundle
        Dim detail As Mock(Of IJobDetail)


        detail = New Mock(Of IJobDetail)

        detail.SetupGet(Function(x) x.JobType).Returns(GetType(Object))
        Return New TriggerFiredBundle(
            detail.Object,
            Mock.Of(Of IOperableTrigger),
            Mock.Of(Of ICalendar),
            False,
            DateTimeOffset.Now,
            Nothing,
            Nothing,
            Nothing
        )
    End Function

End Class
