Public Class RegionAttributeTests

    <Fact()>
    Public Sub StoresRegionName()
        Assert.Equal("foo bar", New RegionAttribute("foo bar").Name)
    End Sub

End Class
