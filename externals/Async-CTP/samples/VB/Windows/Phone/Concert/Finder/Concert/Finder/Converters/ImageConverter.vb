Imports System
Imports System.Globalization
Imports System.Windows
Imports System.Windows.Data

Namespace ConcertFinder.Converters

    ''' <summary>
    ''' Converter for Image paths.
    ''' </summary>
    Public Class ImageConverter
        Implements IValueConverter

        ''' <summary>
        ''' Convert an image name to a path based on the visible theme.
        ''' </summary>
        Public Function Convert(value As Object, targetType As [Type], parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Dim convertedValue = [String].Empty
            Dim imageName = TryCast(parameter, String)
            If Not [String].IsNullOrEmpty(imageName) Then
                convertedValue = If(DirectCast(App.Current.Resources("PhoneDarkThemeVisibility"), Visibility) = Visibility.Visible, [String].Format("/Images/Dark/{0}.png", imageName), [String].Format("/Images/Light/{0}.png", imageName))
            End If
            Return convertedValue
        End Function

        Public Function ConvertBack(value As Object, targetType As [Type], parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function

    End Class

End Namespace