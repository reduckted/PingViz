Imports MahApps.Metro.Controls
Imports PingViz.Views
Imports Prism.Regions
Imports System.Collections.Specialized


<RegionAdapter(GetType(FlyoutsControl))>
Public Class FlyoutsControlRegionAdapter
    Inherits RegionAdapterBase(Of FlyoutsControl)


    Public Sub New(regionBehaviorFactory As IRegionBehaviorFactory)
        MyBase.New(regionBehaviorFactory)
    End Sub


    Protected Overrides Function CreateRegion() As IRegion
        Return New SingleActiveRegion
    End Function


    Protected Overrides Sub Adapt(
            region As IRegion,
            regionTarget As FlyoutsControl
        )

        AddHandler _
            region.Views.CollectionChanged,
            Sub(s, e)
                Select Case e.Action
                    Case NotifyCollectionChangedAction.Add
                        AddFlyoutsForViews(regionTarget, e.NewItems)

                    Case NotifyCollectionChangedAction.Remove
                        If e.OldItems IsNot Nothing Then
                            For Each item In e.OldItems
                                RemoveFlyoutForView(regionTarget, item)
                            Next item
                        End If

                    Case NotifyCollectionChangedAction.Reset
                        regionTarget.Items.Clear()
                        AddFlyoutsForViews(regionTarget, region.Views)

                End Select
            End Sub

        AddHandler _
            region.ActiveViews.CollectionChanged,
            Sub(s, e)
                Select Case e.Action
                    Case NotifyCollectionChangedAction.Add
                        OpenFlyouts(region, regionTarget, e.NewItems)

                    Case NotifyCollectionChangedAction.Remove
                        CloseFlyouts(region, regionTarget, e.OldItems)

                    Case NotifyCollectionChangedAction.Reset
                        OpenFlyouts(region, regionTarget, region.ActiveViews)

                End Select
            End Sub
    End Sub


    Private Sub AddFlyoutsForViews(
            flyoutsControl As FlyoutsControl,
            views As IEnumerable
        )

        For Each item In views
            Dim flyout As Flyout
            Dim element As FrameworkElement


            flyout = New Flyout With {.Content = item}
            element = TryCast(item, FrameworkElement)

            If element IsNot Nothing Then
                flyout.DataContext = element.DataContext
            End If

            flyoutsControl.Items.Add(flyout)
        Next item
    End Sub


    Private Sub RemoveFlyoutForView(
            flyoutsControl As FlyoutsControl,
            view As Object
        )

        For i = 0 To flyoutsControl.Items.Count - 1
            Dim flyout As Flyout


            flyout = TryCast(flyoutsControl.Items.Item(i), Flyout)

            If flyout?.Content Is view Then
                flyoutsControl.Items.RemoveAt(i)
                Exit For
            End If
        Next i
    End Sub


    Private Sub OpenFlyouts(
            region As IRegion,
            flyoutsControl As FlyoutsControl,
            views As IEnumerable
        )

        For Each v In views
            Dim flyout As Flyout


            flyout = flyoutsControl.Items.OfType(Of Flyout).FirstOrDefault(Function(x) x.Content Is v)

            If flyout IsNot Nothing Then
                If Not flyout.IsOpen Then
                    flyout.IsOpen = True
                    WatchForClose(region, flyout)
                    NotifyFlyout(flyout, Sub(x) x.OnOpened())
                End If
            End If
        Next v
    End Sub


    Private Sub WatchForClose(
            region As IRegion,
            flyout As Flyout
        )

        Dim handler As RoutedEventHandler


        handler = Sub(s, e)
                      If Not flyout.IsOpen Then
                          ' Once the flyout closes, we don't need this handler anymore.
                          RemoveHandler flyout.IsOpenChanged, handler

                          ' Deactivate the flyout's content
                          ' if it's currently an active view.
                          If region.ActiveViews.Contains(flyout.Content) Then
                              region.Deactivate(flyout.Content)
                          End If

                          ' Notify the view that the flyout closed.
                          NotifyFlyout(flyout, Sub(x) x.OnClosed())
                      End If
                  End Sub

        AddHandler flyout.IsOpenChanged, handler
    End Sub


    Private Sub CloseFlyouts(
            region As IRegion,
            flyoutsControl As FlyoutsControl,
            views As IEnumerable
        )

        For Each V In views
            Dim flyout As Flyout


            flyout = flyoutsControl.Items.OfType(Of Flyout).FirstOrDefault(Function(x) x.Content Is V)

            If (flyout IsNot Nothing) AndAlso flyout.IsOpen Then
                ' We just need to close the flyout. When it was opened, a handler was
                ' attached to watch for it to close, so everything that we need to
                ' do when closing the flyout will happen in that event handler.
                flyout.IsOpen = False
            End If
        Next V
    End Sub


    Private Sub NotifyFlyout(flyout As Flyout, callback As Action(Of IFlyoutViewModel))
        Dim vm As IFlyoutViewModel


        vm = TryCast(TryCast(flyout.Content, FrameworkElement)?.DataContext, IFlyoutViewModel)

        If vm IsNot Nothing Then
            callback(vm)
        End If
    End Sub

End Class
