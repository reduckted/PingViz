Imports Quartz


<RegisterAs(GetType(ILifetimeService), SingleInstance:=True)>
Public Class PingProcessor
    Implements ILifetimeService


    Private ReadOnly cgSchedulerFactory As ISchedulerFactory
    Private cgScheduler As IScheduler


    Public Sub New(schedulerFactory As ISchedulerFactory)
        If schedulerFactory Is Nothing Then
            Throw New ArgumentNullException(NameOf(schedulerFactory))
        End If

        cgSchedulerFactory = schedulerFactory
    End Sub


    Public Async Function StartAsync() As Task _
        Implements ILifetimeService.StartAsync

        Dim job As IJobDetail
        Dim trigger As ITrigger


        cgScheduler = Await cgSchedulerFactory.GetScheduler()
        Await cgScheduler.Start()

        job = JobBuilder _
            .Create(Of PingJob)() _
            .WithIdentity("ping") _
            .Build()

        ' Start the job at the next interval,
        ' and repeat every interval after that.
        trigger = TriggerBuilder _
            .Create() _
            .WithIdentity("ping") _
            .StartAt(DateBuilder.NextGivenSecondDate(Nothing, CInt(Application.PingInterval.TotalSeconds))) _
            .WithSimpleSchedule(Function(x) x.WithInterval(Application.PingInterval).RepeatForever()) _
            .Build()

        Await cgScheduler.ScheduleJob(job, trigger)
    End Function

End Class
