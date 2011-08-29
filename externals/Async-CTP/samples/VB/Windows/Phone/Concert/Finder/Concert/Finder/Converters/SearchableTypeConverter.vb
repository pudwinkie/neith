Imports System
Imports System.Globalization
Imports System.Windows.Data
Imports Concert_Finder.ConcertFinder.Model

Namespace ConcertFinder.Converters

    ''' <summary>
    ''' Converter for ISearchable values.
    ''' </summary>
    Public Class SearchableTypeConverter
        Implements IValueConverter

        ''' <summary>
        ''' Convert an ISearchable to a string based on its type.
        ''' </summary>
        Public Function Convert(value As Object, targetType As [Type], parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim convertedValue = [String].Empty
            If TypeOf value Is Artist Then
                convertedValue = My.Resources.Strings.Artist
            ElseIf TypeOf value Is Venue Then
                convertedValue = My.Resources.Strings.Venue
            End If
            Return convertedValue
        End Function

        Public Function ConvertBack(value As Object, targetType As [Type], parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function

    End Class

End Namespace