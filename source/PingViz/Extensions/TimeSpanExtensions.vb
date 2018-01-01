Imports System.Runtime.CompilerServices


Public Module TimeSpanExtensions

    <Extension()>
    Public Function ToMilliseconds(time As TimeSpan?) As Integer?
        If time.HasValue Then
            Return CInt(Math.Truncate(time.Value.TotalMilliseconds))
        Else
            Return Nothing
        End If
    End Function

End Module
