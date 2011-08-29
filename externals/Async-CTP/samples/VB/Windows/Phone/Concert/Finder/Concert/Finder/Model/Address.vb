Imports System
Imports System.Globalization
Imports System.Net

Namespace ConcertFinder.Model

    ''' <summary>
    ''' Represents an address.
    ''' </summary>
    Public Class Address

        ''' <summary>
        ''' The street component of the address.
        ''' </summary>
        Public Property Street As String
            Get
                Return _Street
            End Get
            Set
                _Street = HttpUtility.HtmlDecode(value)
            End Set
        End Property
        Private _Street As String

        ''' <summary>
        ''' The city component of the address.
        ''' </summary>
        Public Property City As String
            Get
                Return _City
            End Get
            Set
                _City = HttpUtility.HtmlDecode(value)
            End Set
        End Property
        Private _City As String

        Public ReadOnly Property IsCityEmpty As Boolean
            Get
                Return [String].IsNullOrEmpty(City)
            End Get
        End Property

        ''' <summary>
        ''' The region component of the address.
        ''' </summary>
        Public Property [Region] As String
            Get
                Return _Region
            End Get
            Set
                _Region = HttpUtility.HtmlDecode(value)
            End Set
        End Property
        Private _Region As String

        Public ReadOnly Property IsRegionEmpty As Boolean
            Get
                Return [String].IsNullOrEmpty([Region])
            End Get
        End Property

        ''' <summary>
        ''' The country component of the address.
        ''' </summary>
        Public Property Country As String
            Get
                Return _Country
            End Get
            Set
                _Country = HttpUtility.HtmlDecode(value)
            End Set
        End Property
        Private _Country As String

        ''' <summary>
        ''' The latitude coordinate of the venue.
        ''' </summary>
        Public Property Latitude As Double?

        ''' <summary>
        ''' The longitude coordinate of the venue.
        ''' </summary>
        Public Property Longitude As Double?

        ''' <summary>
        ''' Convert an address to a string.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return If(Latitude.HasValue AndAlso Longitude.HasValue, [String].Format("{0},{1}", Latitude.Value.ToString(CultureInfo.InvariantCulture.NumberFormat), Longitude.Value.ToString(CultureInfo.InvariantCulture.NumberFormat)), [String].Format("{0} {1} {2} {3}", Street, City, [Region], Country))
        End Function

    End Class

End Namespace