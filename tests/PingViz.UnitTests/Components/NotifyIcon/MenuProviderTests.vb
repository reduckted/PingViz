Imports System.Windows.Controls


Namespace NotifyIcon

    Public Class MenuProviderTests

        Public Class Constructor

            <Fact()>
            Public Sub RejectsNullItems()
                Assert.Throws(Of ArgumentNullException)(Function() New MenuProvider(Nothing))
            End Sub

        End Class


        Public Class GetContextMenuMethod

            <StaFact()>
            Public Sub AddsItemsInOrder()
                Dim provider As MenuProvider
                Dim item1 As IMenuItem
                Dim item2 As IMenuItem
                Dim item3 As IMenuItem
                Dim menu As ContextMenu


                item1 = CreateMockItem(order:=1, header:="Z")
                item2 = CreateMockItem(order:=2, header:="A")
                item3 = CreateMockItem(order:=3, header:="P")

                provider = New MenuProvider({item2, item3, item1})

                menu = provider.GetContextMenu()


                Assert.Equal(3, menu.Items.Count)
                Assert.Equal("Z", TryCast(menu.Items(0), MenuItem)?.Header)
                Assert.Equal("A", TryCast(menu.Items(1), MenuItem)?.Header)
                Assert.Equal("P", TryCast(menu.Items(2), MenuItem)?.Header)
            End Sub


            <StaFact()>
            Public Sub AddsSeparatorsAtStartOfNewGroup()
                Dim provider As MenuProvider
                Dim item1 As IMenuItem
                Dim item2 As IMenuItem
                Dim item3 As IMenuItem
                Dim item4 As IMenuItem
                Dim menu As ContextMenu


                item1 = CreateMockItem(order:=1, header:="Z")
                item2 = CreateMockItem(order:=2, header:="A", isStartOfGroup:=True)
                item3 = CreateMockItem(order:=3, header:="P")
                item4 = CreateMockItem(order:=3, header:="P", isStartOfGroup:=True)

                provider = New MenuProvider({item2, item3, item4, item1})

                menu = provider.GetContextMenu()

                Assert.Equal(6, menu.Items.Count)
                Assert.IsType(Of MenuItem)(menu.Items(0))
                Assert.IsType(Of Separator)(menu.Items(1))
                Assert.IsType(Of MenuItem)(menu.Items(2))
                Assert.IsType(Of MenuItem)(menu.Items(3))
                Assert.IsType(Of Separator)(menu.Items(4))
                Assert.IsType(Of MenuItem)(menu.Items(5))
            End Sub


            <StaFact()>
            Public Sub DoesNotAddSeparatorBeforeFirstItem()
                Dim provider As MenuProvider
                Dim item1 As IMenuItem
                Dim item2 As IMenuItem
                Dim menu As ContextMenu


                item1 = CreateMockItem(order:=1, header:="Z", isStartOfGroup:=True)
                item2 = CreateMockItem(order:=2, header:="A", isStartOfGroup:=True)

                provider = New MenuProvider({item2, item1})

                menu = provider.GetContextMenu()

                Assert.Equal(3, menu.Items.Count)
                Assert.IsType(Of MenuItem)(menu.Items(0))
                Assert.IsType(Of Separator)(menu.Items(1))
                Assert.IsType(Of MenuItem)(menu.Items(2))
            End Sub


            <StaFact()>
            Public Sub UsesItemAsCommand()
                Dim provider As MenuProvider
                Dim item1 As IMenuItem
                Dim item2 As IMenuItem
                Dim menu As ContextMenu


                item1 = CreateMockItem(order:=1)
                item2 = CreateMockItem(order:=2)

                provider = New MenuProvider({item2, item1})

                menu = provider.GetContextMenu()

                Assert.Equal(2, menu.Items.Count)
                Assert.Same(item1, TryCast(menu.Items(0), MenuItem)?.Command)
                Assert.Same(item2, TryCast(menu.Items(1), MenuItem)?.Command)
            End Sub


            <StaFact()>
            Public Sub CachesTheContextMenu()
                Dim provider As MenuProvider


                provider = New MenuProvider({CreateMockItem()})

                Assert.Same(provider.GetContextMenu(), provider.GetContextMenu())
            End Sub


            Private Function CreateMockItem(
                    Optional order As Integer = 0,
                    Optional header As String = "foo",
                    Optional isStartOfGroup As Boolean = False
                ) As IMenuItem

                Dim item As Mock(Of IMenuItem)


                item = New Mock(Of IMenuItem)
                item.SetupGet(Function(x) x.Order).Returns(order)
                item.SetupGet(Function(x) x.Header).Returns(header)
                item.SetupGet(Function(x) x.IsStartOfGroup).Returns(isStartOfGroup)

                Return item.Object
            End Function

        End Class

    End Class

End Namespace
