Imports System
Imports System.Globalization
Imports System.Windows.Data

Namespace ConcertFinder.Converters

    ''' <summary>
    ''' Converter for boolean values.
    ''' </summary>
    Public Class BooleanOpacityConverter
        Implements IValueConverter

        ''' <summary>
        ''' Convert a boolean value to an opacity.
        ''' </summary>
        Public Function Convert(value As Object, targetType As [Type], parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim convertedValue = 1
            Dim originalValue = DirectCast(value, Boolean)
            convertedValue = If((originalValue = True), 1, 0)
            Return convertedValue
        End Function

        Public Function ConvertBack(value As Object, targetType As [Type], parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function

    End Class

End Namespace