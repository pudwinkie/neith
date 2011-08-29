
Namespace ConcertFinder.Resources

    ''' <summary>
    ''' Wrapper class for data binding to resource strings.
    ''' </summary>
    Public Class Resources

        ''' <summary>
        ''' Create a property to databind to strings in Configuration.resx.
        ''' </summary>
        Public ReadOnly Property Configuration As My.Resources.Configuration
            Get
                Return _Configuration
            End Get
        End Property
        Private _Configuration As My.Resources.Configuration = New My.Resources.Configuration()

        ''' <summary>
        ''' Create a property to databind to strings in Strings.resx.
        ''' </summary>
        Public ReadOnly Property Strings As My.Resources.Strings
            Get
                Return _Strings
            End Get
        End Property
        Private _Strings As My.Resources.Strings = New My.Resources.Strings()

    End Class

End Namespace