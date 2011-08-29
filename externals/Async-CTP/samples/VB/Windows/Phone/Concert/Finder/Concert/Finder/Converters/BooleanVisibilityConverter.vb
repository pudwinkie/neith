Imports System
Imports System.Globalization
Imports System.Windows
Imports System.Windows.Data

Namespace ConcertFinder.Converters

    ''' <summary>
    ''' Converter for boolean values.
    ''' </summary>
    Public Class BooleanVisibilityConverter
        Implements IValueConverter

        ''' <summary>
        ''' Convert a boolean value to a visibility.
        ''' </summary>
        Public Function Convert(value As Object, targetType As [Type], parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim convertedValue = Visibility.Visible
            Dim originalValue = DirectCast(value, Boolean)
            Dim isInverted = (If(TryCast(parameter, String), [String].Empty)).[Equals]("IsInverted")
            If originalValue = True Then
                convertedValue = If(Not isInverted, Visibility.Visible, Visibility.Collapsed)
            Else
                convertedValue = If(Not isInverted, Visibility.Collapsed, Visibility.Visible)
            End If
            Return convertedValue
        End Function

        Public Function ConvertBack(value As Object, targetType As [Type], parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function

    End Class

End Namespace