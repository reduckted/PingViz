Imports Autofac
Imports Quartz
Imports Quartz.Spi
Imports System.Collections.Concurrent


<RegisterAs(GetType(IJobFactory), SingleInstance:=True)>
Public Class AutofacJobFactory
    Implements IJobFactory


    Private ReadOnly cgLifetimeScope As ILifetimeScope
    Private ReadOnly cgJobScopes As ConcurrentDictionary(Of Object, ILifetimeScope)


    Public Sub New(lifetimeScope As ILifetimeScope)
        If lifetimeScope Is Nothing Then
            Throw New ArgumentNullException(NameOf(lifetimeScope))
        End If

        cgLifetimeScope = lifetimeScope
        cgJobScopes = New ConcurrentDictionary(Of Object, ILifetimeScope)
    End Sub


    Public Function NewJob(
            bundle As TriggerFiredBundle,
            scheduler As IScheduler
        ) As IJob _
        Implements IJobFactory.NewJob

        Dim type As Type
        Dim scope As ILifetimeScope
        Dim job As IJob


        type = bundle.JobDetail.JobType
        scope = cgLifetimeScope.BeginLifetimeScope()

        Try
            job = DirectCast(scope.Resolve(type), IJob)
            cgJobScopes(job) = scope

        Catch ex As Exception
            scope.Dispose()
            Throw New SchedulerConfigException($"Failed to create job '{bundle.JobDetail.Key}' of type '{type}'", ex)
        End Try

        Return job
    End Function


    Public Sub ReturnJob(job As IJob) _
        Implements IJobFactory.ReturnJob

        Dim scope As ILifetimeScope = Nothing


        If cgJobScopes.TryRemove(job, scope) Then
            scope.Dispose()
        End If
    End Sub

End Class
