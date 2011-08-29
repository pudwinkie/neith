Namespace ConcertFinder.ViewModel

    ''' <summary>
    ''' The view model for the ArtistDetailsPage.
    ''' </summary>
    Public Class ArtistDetailsPageViewModel
        Inherits AbstractDetailsPageViewModel

        Public Sub New()
            Me.EventGroup = Model.EventGroup.Artist
        End Sub

    End Class

End Namespace