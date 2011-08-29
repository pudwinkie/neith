'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: DataTypes.vb
'
'--------------------------------------------------------------------------

''' <summary>Represents a real-estate listing.</summary>
Friend NotInheritable Class Listing
    ''' <summary>Initializes the listing.</summary>
    ''' <param name="name">The name of the listing.</param>
    ''' <param name="price">The price of the listing.</param>
    Public Sub New(name As String, price As Integer)
        Me.Name = name
        Me.Price = price
    End Sub

    ''' <summary>Gets the name of the listing.</summary>
    Public Property Name As String
    ''' <summary>Gets the price of the listing.</summary>
    Public Property Price As Integer
End Class

''' <summary>Represents a search for listings that match certain criteria.</summary>
Friend NotInheritable Class SearchFilter
    ''' <summary>Initializes the filter.</summary>
    ''' <param name="minPrice">The minimum price for listings to select.</param>
    ''' <param name="maxPrice">The maximum price for listings to select.</param>
    Public Sub New(minPrice As Integer, maxPrice As Integer)
        Me.MinPrice = minPrice
        Me.MaxPrice = maxPrice
    End Sub

    ''' <summary>Gets the minimum price for listings to select.</summary>
    Public Property MinPrice As Integer
    ''' <summary>Gets the maximum price for listings to select.</summary>
    Public Property MaxPrice As Integer
End Class

''' <summary>Represents an offer on a listing.</summary>
Friend NotInheritable Class Offer
''' <summary>Initializes the offer.</summary>
''' <param name="buyer">The buyer making the offer.</param>
''' <param name="listing">The listing on which the offer is being made.</param>
''' <param name="price">The price of the offer.</param>
    Public Sub New(buyer As BuyerAgent, listing As Listing, price As Integer)
        Me.Buyer = buyer
        Me.Listing = listing
        Me.Price = price
    End Sub

    ''' <summary>Gets the buyer making the offer.</summary>
    Public Property Buyer As BuyerAgent
    ''' <summary>Gets the listing on which the offer is being made.</summary>
    Public Property Listing As Listing
    ''' <summary>Gets the price of the offer..</summary>
    Public Property Price As Integer
End Class