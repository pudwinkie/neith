Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Net
Imports System.Xml.Serialization

Namespace ConcertFinder.Model

    ''' <summary>
    ''' Represents an event.
    ''' </summary>
    Public Class [Event]
        Implements ISearchable

        ''' <summary>
        ''' The ID of the event.
        ''' </summary>
        Public Property Id As String Implements ISearchable.Id

        ''' <summary>
        ''' The name of the event.
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
        ''' The location of the event used for databinding.
        ''' </summary>
        Public Property Location As String
            Get
                Return _Location
            End Get
            Set(value As String)
                _Location = HttpUtility.HtmlDecode(value)
            End Set
        End Property
        Private _Location As String

        ''' <summary>
        ''' The venue of the event.
        ''' </summary>
        Public Property Venue As Venue

        ''' <summary>
        ''' The start time of the event.
        ''' </summary>
        Public Property StartTime As DateTime?

        ''' <summary>
        ''' The end time of the event.
        ''' </summary>
        Public Property EndTime As DateTime?

        ''' <summary>
        ''' Whether the start and end times fall within a day.
        ''' </summary>
        Public Property IsSingleDay As Boolean

        ''' <summary>
        ''' Whether the start and end dates contain time components.
        ''' </summary>
        Public Property IsTimeSpecified As Boolean

        ''' <summary>
        ''' The list of artists performing at the event.
        ''' </summary>
        Public Property Artists As List(Of Artist)

        Public ReadOnly Property IsListEmpty As Boolean
            Get
                Return Artists.Count = 0
            End Get
        End Property

        ''' <summary>
        ''' The description of the event.
        ''' </summary>
        Public Property Description As String

        ''' <summary>
        ''' The image for the event.
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
        ''' The Uri of the event.
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
            Dim other = TryCast(obj, [Event])
            Return other IsNot Nothing AndAlso other.Id.[Equals](Me.Id)
        End Function

    End Class

    ''' <summary>
    ''' An IComparer that compares start times.
    ''' </summary>
    Public Class StartTimeComparer
        Implements IComparer(Of [Event])

        Public Function [Compare](x As [Event], y As [Event]) As Integer Implements IComparer(Of Concert_Finder.ConcertFinder.Model.Event).Compare
            Dim result = 0
            If x.StartTime.HasValue AndAlso y.StartTime.HasValue Then
                result = x.StartTime.Value.CompareTo(y.StartTime.Value)
            End If
            Return result
        End Function

    End Class

    ''' <summary>
    ''' An enumeration of event groups.
    ''' </summary>
    Public Enum EventGroup
        Artist = 0
        Venue = 1
    End Enum

End Namespace