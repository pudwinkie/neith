Imports <xmlns="http://www.w3.org/2005/Atom">
Imports <xmlns:d="http://schemas.microsoft.com/ado/2007/08/dataservices">
Imports <xmlns:m="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata">
Imports System.Threading
Imports System.Xml.Linq
Imports System.Threading.Tasks
Imports System.Collections.ObjectModel

Public MustInherit Class NetflixEntity
    Public Property Title As String

    Public Overridable Sub LoadFromXML(entry As XElement)
        Title = entry.<title>.Value
    End Sub
End Class


Public Class Movie
    Inherits NetflixEntity

    Public Property Url As String
    Public Property Img As String

    Public Overrides Sub LoadFromXML(entry As System.Xml.Linq.XElement)
        MyBase.LoadFromXML(entry)

        Url = entry.<m:properties>.<d:Url>.Value
        Img = entry.<m:properties>.<d:BoxArt>.<d:SmallUrl>.Value
    End Sub
End Class


Public Class NetflixQuery(Of T As {New, NetflixEntity})

    Public Property Query As String
    Public Property Entities As New ObservableCollection(Of T)
    Public Property EntitiesExpected As Integer?
    Private client As New WebClient

    Sub New(query As String)
        Me.Query = query
    End Sub

    Public Async Function FetchEntitiesAsync(cancel As CancellationToken) As Task
        Dim nextUrl = Query

        ' No try block -- cancellation and errors bubble up to the caller naturally
        While nextUrl IsNot Nothing
            Dim result = XDocument.Parse(Await client.DownloadStringTaskAsync(New Uri(nextUrl), cancel))

            Dim countElement = result...<m:count>.SingleOrDefault
            If countElement IsNot Nothing Then EntitiesExpected = CInt(countElement.Value)

            For Each entry In result...<entry>
                Dim entity As New T
                entity.LoadFromXML(entry)
                Entities.Add(entity)
            Next

            nextUrl = GetNextUri(result)
        End While

        EntitiesExpected = Entities.Count
    End Function

    Private Function GetNextUri(xml As XDocument)
        Return (From elem In xml.<feed>.<link>
                Where elem.@rel = "next"
                Select elem.@href).SingleOrDefault
    End Function


    ' No CancelAsync method needed.  Cancellation requests flow from
    ' ViewModel -> Model -> the underlying async API via the CancellationToken.

End Class
