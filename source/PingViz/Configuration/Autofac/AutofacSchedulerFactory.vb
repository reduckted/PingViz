Imports Quartz
Imports Quartz.Core
Imports Quartz.Impl
Imports Quartz.Spi


<RegisterAs(GetType(ISchedulerFactory), SingleInstance:=True)>
Public Class AutofacSchedulerFactory
    Inherits StdSchedulerFactory


    Private ReadOnly cgJobFactory As IJobFactory


    Public Sub New(jobFactory As IJobFactory)
        If jobFactory Is Nothing Then
            Throw New ArgumentNullException(NameOf(jobFactory))
        End If

        cgJobFactory = jobFactory
    End Sub


    Protected Overrides Function Instantiate(
            rsrcs As QuartzSchedulerResources,
            qs As QuartzScheduler
        ) As IScheduler

        Dim scheduler As IScheduler


        scheduler = MyBase.Instantiate(rsrcs, qs)
        scheduler.JobFactory = cgJobFactory

        Return scheduler
    End Function

End Class
