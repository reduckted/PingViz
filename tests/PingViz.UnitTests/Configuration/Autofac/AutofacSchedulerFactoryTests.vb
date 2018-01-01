Imports Quartz
Imports Quartz.Core
Imports Quartz.Spi
Imports System.Reflection


Public Class AutofacSchedulerFactoryTests

    <Fact()>
    Public Async Function AssignsJobFactoryToScheduler() As Task
        Dim schedulerFactory As AutofacSchedulerFactory
        Dim jobFactory As IJobFactory
        Dim scheduler As IScheduler
        Dim underlyingScheduler As QuartzScheduler


        jobFactory = Mock.Of(Of IJobFactory)
        schedulerFactory = New AutofacSchedulerFactory(jobFactory)

        scheduler = Await schedulerFactory.GetScheduler()

        underlyingScheduler = scheduler _
            .GetType() _
            .GetFields(BindingFlags.NonPublic Or BindingFlags.Instance) _
            .Where(Function(x) x.FieldType.Equals(GetType(QuartzScheduler))) _
            .Select(Function(x) x.GetValue(scheduler)) _
            .OfType(Of QuartzScheduler) _
            .FirstOrDefault()

        Assert.NotNull(underlyingScheduler)
        Assert.Same(jobFactory, underlyingScheduler.JobFactory)
    End Function

End Class
