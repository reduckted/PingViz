Imports Autofac
Imports Autofac.Builder
Imports System.Reflection


Public NotInheritable Class ServiceRegistrator

    Public Shared Sub RegisterServices(builder As ContainerBuilder, types As IEnumerable(Of Type))
        Dim services As IEnumerable(Of (Type As Type, Attribute As RegisterAsAttribute))


        services = (
            From type In types
            From attribute In type.GetCustomAttributes(Of RegisterAsAttribute)
            Select (type, attribute)
        )

        For Each service In services
            Dim registrationBuilder As IRegistrationBuilder(Of Object, ConcreteReflectionActivatorData, SingleRegistrationStyle)


            registrationBuilder = builder.RegisterType(service.Type).As(service.Attribute.Types.ToArray())

            If service.Attribute.SingleInstance Then
                registrationBuilder.SingleInstance()
            End If
        Next service
    End Sub

End Class
