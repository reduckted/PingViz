Public Class RegisterAsAttributeTests

    <Fact()>
    Public Sub StoresTheSingleType()
        Assert.Equal({GetType(String)}, New RegisterAsAttribute(GetType(String)).Types)
    End Sub


    <Fact()>
    Public Sub StoresTheManyTypes()
        Assert.Equal(
            {GetType(String), GetType(Integer)},
            New RegisterAsAttribute({GetType(String), GetType(Integer)}).Types
        )
    End Sub

End Class
