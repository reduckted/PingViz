Public Class RegionAdapterAttributeTests

    <Fact()>
    Public Sub StoresControlType()
        Assert.Equal(GetType(String), New RegionAdapterAttribute(GetType(String)).ControlType)
    End Sub

End Class
