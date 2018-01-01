Imports System.Runtime.CompilerServices


Public Module DateExtensions

    <Extension()>
    Public Function RoundDown(d As Date, toNearest As TimeSpan) As Date
        Return New Date((d.Ticks \ toNearest.Ticks) * toNearest.Ticks)
    End Function

End Module
