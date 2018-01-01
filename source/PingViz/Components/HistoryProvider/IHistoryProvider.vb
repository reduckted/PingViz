Public Interface IHistoryProvider

    Function GetPingsAsync(
            startDateTime As Date,
            endDateTime As Date
        ) As Task(Of IEnumerable(Of PingResult))

End Interface
