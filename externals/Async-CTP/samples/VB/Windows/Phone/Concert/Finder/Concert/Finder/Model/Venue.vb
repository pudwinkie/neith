Imports System
Imports System.ComponentModel
Imports System.Net
Imports System.Xml.Serialization

Namespace ConcertFinder.Model

    ''' <summary>
    ''' Represents a venue.
    ''' </summary>
    Public Class Venue
        Implements ISearchable

        ''' <summary>
        ''' The ID of the venue.
        ''' </summary>
        Public Property Id As String Implements ISearchable.Id

        ''' <summary>
        ''' The name of the venue.
        ''' </summary>
        Public Property Name As String Implements ISearchable.Name
            Get
                Return _Name
            End Get
            Set(value As String)
                _Name = HttpUtility.HtmlDecode(value)
            End Set
        End Property
        Private _Name As String

        ''' <summary>
        ''' The address of the venue.
        ''' </summary>
        Public Property Address As Address

        ''' <summary>
        ''' The image for the venue.
        ''' </summary>
        <XmlIgnore()>
        Public Property Image As Uri Implements ISearchable.Image
            Get
                Return If(Not [String].IsNullOrEmpty(_Image), New Uri(_Image), Nothing)
            End Get
            Set(value As Uri)
                _Image = If(value IsNot Nothing, value.OriginalString, Nothing)
            End Set
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Property _Image As String Implements ISearchable._Image

        ''' <summary>
        ''' The Uri of the venue.
        ''' </summary>
        <XmlIgnore()>
        Public Property Uri As Uri Implements ISearchable.Uri
            Get
                Return If(Not [String].IsNullOrEmpty(_Uri), New Uri(_Uri), Nothing)
            End Get
            Set(value As Uri)
                _Uri = If(value IsNot Nothing, value.OriginalString, Nothing)
            End Set
        End Property

        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Property _Uri As String Implements ISearchable._Uri

        ''' <summary>
        ''' Override for Equals.
        ''' </summary>
        Public Overrides Function Equals(obj As Object) As Boolean
            Dim other = TryCast(obj, Venue)
            Return other IsNot Nothing AndAlso other.Id.[Equals](Me.Id)
        End Function

    End Class

End Namespace