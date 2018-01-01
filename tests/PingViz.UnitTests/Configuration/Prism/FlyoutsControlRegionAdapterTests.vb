Imports MahApps.Metro.Controls
Imports PingViz.Views
Imports Prism.Regions
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Threading


Public Class FlyoutsControlRegionAdapterTests

    <StaFact()>
    Public Sub CreatesSingleActiveRegion()
        Dim adapter As FlyoutsControlRegionAdapter
        Dim flyouts As FlyoutsControl


        flyouts = New FlyoutsControl
        adapter = New FlyoutsControlRegionAdapter(Nothing)

        Assert.IsType(Of SingleActiveRegion)(adapter.Initialize(flyouts, "foo"))
    End Sub


    <StaFact()>
    Public Sub WrapsControlsInFlyout()
        Dim adapter As FlyoutsControlRegionAdapter
        Dim flyouts As FlyoutsControl
        Dim region As IRegion
        Dim control As TextBlock
        Dim dataContext As Object


        flyouts = New FlyoutsControl
        adapter = New FlyoutsControlRegionAdapter(Nothing)

        control = New TextBlock
        dataContext = New Object
        control.DataContext = dataContext

        region = adapter.Initialize(flyouts, "foo")
        region.Add(control)

        Assert.Equal(1, flyouts.Items.Count)
        Assert.IsType(Of Flyout)(flyouts.Items.Item(0))
        Assert.Same(control, DirectCast(flyouts.Items.Item(0), Flyout).Content)
        Assert.Same(control.DataContext, DirectCast(flyouts.Items.Item(0), Flyout).DataContext)
    End Sub


    <StaFact()>
    Public Sub OpensFlyoutWhenViewIsActivated()
        Dim adapter As FlyoutsControlRegionAdapter
        Dim flyouts As FlyoutsControl
        Dim flyout As Flyout
        Dim region As IRegion
        Dim control As FrameworkElement


        flyouts = New FlyoutsControl
        adapter = New FlyoutsControlRegionAdapter(Nothing)

        control = New TextBlock

        region = adapter.Initialize(flyouts, "foo")
        region.Add(control)

        Assert.Equal(1, flyouts.Items.Count)
        flyout = DirectCast(flyouts.Items.Item(0), Flyout)

        Assert.False(flyout.IsOpen)

        region.Activate(control)
        Assert.True(flyout.IsOpen)
    End Sub


    <StaFact()>
    Public Sub NotifiesTheViewModelWhenTheViewIsActivated()
        Dim adapter As FlyoutsControlRegionAdapter
        Dim flyouts As FlyoutsControl
        Dim region As IRegion
        Dim control As FrameworkElement
        Dim viewModel As Mock(Of IFlyoutViewModel)


        flyouts = New FlyoutsControl
        adapter = New FlyoutsControlRegionAdapter(Nothing)

        viewModel = New Mock(Of IFlyoutViewModel)
        control = New TextBlock With {.DataContext = viewModel.Object}

        region = adapter.Initialize(flyouts, "foo")
        region.Add(control)

        viewModel.Verify(Sub(x) x.OnOpened(), Times.Never)
        viewModel.Verify(Sub(x) x.OnClosed(), Times.Never)

        region.Activate(control)

        viewModel.Verify(Sub(x) x.OnOpened(), Times.Once)
        viewModel.Verify(Sub(x) x.OnClosed(), Times.Never)
    End Sub


    <StaFact()>
    Public Sub ClosesFlyoutWhenViewIsDeactivated()
        Dim adapter As FlyoutsControlRegionAdapter
        Dim flyouts As FlyoutsControl
        Dim flyout As Flyout
        Dim region As IRegion
        Dim control As FrameworkElement


        flyouts = New FlyoutsControl
        adapter = New FlyoutsControlRegionAdapter(Nothing)

        control = New TextBlock

        region = adapter.Initialize(flyouts, "foo")
        region.Add(control)

        Assert.Equal(1, flyouts.Items.Count)
        flyout = DirectCast(flyouts.Items.Item(0), Flyout)

        region.Activate(control)
        Assert.True(flyout.IsOpen)

        region.Deactivate(control)
        Assert.False(flyout.IsOpen)
    End Sub


    <StaFact()>
    Public Sub NotifiesTheViewModelWhenTheViewIsDeactivated()
        Dim adapter As FlyoutsControlRegionAdapter
        Dim flyouts As FlyoutsControl
        Dim region As IRegion
        Dim control As FrameworkElement
        Dim viewModel As Mock(Of IFlyoutViewModel)


        flyouts = New FlyoutsControl
        adapter = New FlyoutsControlRegionAdapter(Nothing)

        viewModel = New Mock(Of IFlyoutViewModel)
        control = New TextBlock With {.DataContext = viewModel.Object}

        region = adapter.Initialize(flyouts, "foo")
        region.Add(control)

        region.Activate(control)
        viewModel.Verify(Sub(x) x.OnOpened(), Times.Once)
        viewModel.Verify(Sub(x) x.OnClosed(), Times.Never)

        region.Deactivate(control)

        ' Deactivating closes the flyout, and the notification should be triggered
        ' from that event. That `IsOpenChanged` event is fired asynchronously, 
        ' so we need to make the dispatcher process the event queue.
        PumpDispatcher()

        viewModel.Verify(Sub(x) x.OnOpened(), Times.Once)
        viewModel.Verify(Sub(x) x.OnClosed(), Times.Once)
    End Sub


    <StaFact()>
    Public Sub DeactivatesViewWhenFlyoutIsClosed()
        Dim adapter As FlyoutsControlRegionAdapter
        Dim flyouts As FlyoutsControl
        Dim flyout As Flyout
        Dim region As IRegion
        Dim control As FrameworkElement


        flyouts = New FlyoutsControl
        adapter = New FlyoutsControlRegionAdapter(Nothing)

        control = New TextBlock

        region = adapter.Initialize(flyouts, "foo")
        region.Add(control)

        Assert.Equal(1, flyouts.Items.Count)
        flyout = DirectCast(flyouts.Items.Item(0), Flyout)

        region.Activate(control)
        Assert.True(flyout.IsOpen)
        Assert.Contains(control, region.ActiveViews)

        flyout.IsOpen = False

        ' The `IsOpenChanged` event is fired asynchronously, so we
        ' need to make the dispatcher process the event queue.
        PumpDispatcher()

        Assert.DoesNotContain(control, region.ActiveViews)
    End Sub


    <StaFact()>
    Public Sub NotifiesTheViewModelWhenTheFlyoutIsClosed()
        Dim adapter As FlyoutsControlRegionAdapter
        Dim flyouts As FlyoutsControl
        Dim flyout As Flyout
        Dim region As IRegion
        Dim control As FrameworkElement
        Dim viewModel As Mock(Of IFlyoutViewModel)


        flyouts = New FlyoutsControl
        adapter = New FlyoutsControlRegionAdapter(Nothing)

        viewModel = New Mock(Of IFlyoutViewModel)
        control = New TextBlock With {.DataContext = viewModel.Object}

        region = adapter.Initialize(flyouts, "foo")
        region.Add(control)

        Assert.Equal(1, flyouts.Items.Count)
        flyout = DirectCast(flyouts.Items.Item(0), Flyout)

        region.Activate(control)
        viewModel.Verify(Sub(x) x.OnOpened(), Times.Once)
        viewModel.Verify(Sub(x) x.OnClosed(), Times.Never)

        flyout.IsOpen = False

        ' The `IsOpenChanged` event is fired asynchronously, so we
        ' need to make the dispatcher process the event queue.
        PumpDispatcher()

        viewModel.Verify(Sub(x) x.OnOpened(), Times.Once)
        viewModel.Verify(Sub(x) x.OnClosed(), Times.Once)
    End Sub


    Private Shared Sub PumpDispatcher()
        Dim frame As DispatcherFrame


        frame = New DispatcherFrame

        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            New DispatcherOperationCallback(
                Function()
                    frame.Continue = False
                    Return Nothing
                End Function
            ),
            frame
        )

        Dispatcher.PushFrame(frame)
    End Sub

End Class
