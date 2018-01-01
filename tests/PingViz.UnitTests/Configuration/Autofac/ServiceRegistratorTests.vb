Imports Autofac

Public Class ServiceRegistratorTests

    <Fact()>
    Public Sub RegistersServiceAsSpecifiedSingleType()
        Dim builder As ContainerBuilder
        Dim container As IContainer


        builder = New ContainerBuilder

        ServiceRegistrator.RegisterServices(builder, {GetType(SingleType)})

        container = builder.Build()

        Assert.IsType(Of SingleType)(container.ResolveOptional(Of IFoo))
    End Sub


    <Fact()>
    Public Sub RegistersServiceAsSpecifiedMultipleTypes()
        Dim builder As ContainerBuilder
        Dim container As IContainer
        Dim foo As IFoo
        Dim bar As IBar


        builder = New ContainerBuilder

        ServiceRegistrator.RegisterServices(builder, {GetType(MultiType)})

        container = builder.Build()

        foo = container.ResolveOptional(Of IFoo)
        bar = container.ResolveOptional(Of IBar)

        Assert.IsType(Of MultiType)(foo)
        Assert.IsType(Of MultiType)(bar)
        Assert.Same(foo, bar)
    End Sub


    <Fact()>
    Public Sub RegistersServiceUsingAllDefinedAttributes()
        Dim builder As ContainerBuilder
        Dim container As IContainer
        Dim foo As IFoo
        Dim bar As IBar


        builder = New ContainerBuilder

        ServiceRegistrator.RegisterServices(builder, {GetType(MultiAttributes)})

        container = builder.Build()

        foo = container.ResolveOptional(Of IFoo)
        bar = container.ResolveOptional(Of IBar)

        Assert.IsType(Of MultiAttributes)(foo)
        Assert.IsType(Of MultiAttributes)(bar)
        Assert.NotSame(foo, bar)
    End Sub


    <Fact()>
    Public Sub RegistersServiceAsSingleInstanceWhenSpecified()
        Dim builder As ContainerBuilder
        Dim container As IContainer
        Dim foo As IFoo
        Dim bar As IBar


        builder = New ContainerBuilder

        ServiceRegistrator.RegisterServices(builder, {GetType(SingleInstance), GetType(MultiInstance)})

        container = builder.Build()

        foo = container.ResolveOptional(Of IFoo)
        bar = container.ResolveOptional(Of IBar)

        Assert.IsType(Of SingleInstance)(foo)
        Assert.IsType(Of MultiInstance)(bar)

        Assert.Same(foo, container.ResolveOptional(Of IFoo))
        Assert.NotSame(bar, container.ResolveOptional(Of IBar))
    End Sub


    <RegisterAs(GetType(IFoo))>
    Private Class SingleType
        Implements IFoo

    End Class


    <RegisterAs({GetType(IFoo), GetType(IBar)}, SingleInstance:=True)>
    Private Class MultiType
        Implements IFoo
        Implements IBar

    End Class


    <RegisterAs(GetType(IFoo))>
    <RegisterAs(GetType(IBar))>
    Private Class MultiAttributes
        Implements IFoo
        Implements IBar

    End Class

    <RegisterAs(GetType(IFoo), SingleInstance:=True)>
    Private Class SingleInstance
        Implements IFoo

    End Class


    <RegisterAs(GetType(IBar))>
    Private Class MultiInstance
        Implements IBar

    End Class


    Private Interface IFoo

    End Interface


    Private Interface IBar

    End Interface

End Class
