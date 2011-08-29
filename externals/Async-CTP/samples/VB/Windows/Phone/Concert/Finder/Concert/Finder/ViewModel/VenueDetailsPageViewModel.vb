Namespace ConcertFinder.ViewModel

    ''' <summary>
    ''' The view model for the VenueDetailsPage.
    ''' </summary>
    Public Class VenueDetailsPageViewModel
        Inherits AbstractDetailsPageViewModel

        Public Sub New()
            Me.EventGroup = Model.EventGroup.Venue
        End Sub

    End Class

End Namespace