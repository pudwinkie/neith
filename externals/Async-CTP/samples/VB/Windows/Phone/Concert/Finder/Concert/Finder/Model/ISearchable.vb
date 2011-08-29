Imports System
Imports System.ComponentModel
Imports System.Xml.Serialization

Namespace ConcertFinder.Model
    ''' <summary>
    ''' Represents a search result.
    ''' </summary>
    Public Interface ISearchable

        ''' <summary>
        ''' The ID of the search result.
        ''' </summary>
        Property Id As String

        ''' <summary>
        ''' The name of the search result.
        ''' </summary>
        Property Name As String

        ''' <summary>
        ''' The image for the search result.
        ''' </summary>
        <XmlIgnore()>
        Property Image As Uri

        <EditorBrowsable(EditorBrowsableState.Never)>
        Property _Image As String

        ''' <summary>
        ''' The Uri of the search result.
        ''' </summary>
        <XmlIgnore()>
        Property Uri As Uri

        <EditorBrowsable(EditorBrowsableState.Never)>
        Property _Uri As String

    End Interface

End Namespace