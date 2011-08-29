Imports System
Imports System.Globalization
Imports System.Windows.Data

Namespace ConcertFinder.Converters

    ''' <summary>
    ''' Converter for DateTime values.
    ''' </summary>
    Public Class TimeConverter
        Implements IValueConverter

        ''' <summary>
        ''' Convert a DateTime value to a string using the given format string parameter.
        ''' </summary>
        Public Function Convert(value As Object, targetType As [Type], parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim convertedValue = [String].Empty
            Dim originalValue = CType(value, DateTime?)
            Dim format = TryCast(parameter, String)
            If originalValue.HasValue AndAlso Not [String].IsNullOrEmpty(format) Then
                convertedValue = originalValue.Value.ToString(format)
            End If
            Return convertedValue
        End Function

        Public Function ConvertBack(value As Object, targetType As [Type], parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function

    End Class

End Namespace