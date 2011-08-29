'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: RealtorAgent.vb
'
'--------------------------------------------------------------------------

Imports System.Threading.Tasks.Dataflow
Imports System.Threading.Tasks

Friend Class RealtorAgent
    ''' <summary>Gets a source for querying the realtor's current buyers.</summary>
    Public ReadOnly CurrentBuyers As New BroadcastBlock(Of String())(Function(i) i)
    ''' <summary>Gets a source for querying the realtor's current listings.</summary>
    Public ReadOnly CurrentListings As New BroadcastBlock(Of Listing())(Function(i) i)

    ''' <summary>Gets a target for new listings the realtor should represent.</summary>
    Private ReadOnly m_listHouse As ITargetBlock(Of Listing)
    ''' <summary>Gets a target for alerting the realtor to a new buyer.</summary>
    Private ReadOnly m_subscribeBuyer As ITargetBlock(Of BuyerAgent)
    ''' <summary>Gets a target for querying the realtor for listings that match certain criteria..</summary>
    Private ReadOnly m_search As ITargetBlock(Of Tuple(Of SearchFilter, WriteOnceBlock(Of Listing)))
    ''' <summary>Gets a target for making an offer to the realtor to buy a particular listing.</summary>
    Private ReadOnly m_buy As ITargetBlock(Of Tuple(Of Offer, WriteOnceBlock(Of Boolean)))

    ''' <summary>A collection of the realtor's listings.</summary>
    Private ReadOnly m_listings As New HashSet(Of Listing)
    ''' <summary>A collection of the realtor's buyers.</summary>
    Private ReadOnly m_buyers As New HashSet(Of BuyerAgent)

    ''' <summary>Initializes the realtor agent.</summary>
    Public Sub New()
        ' Allow certain realtor processing to happen concurrently and some processing to happen exclusively
        Dim ce = New ConcurrentExclusiveSchedulerPair()
        Dim exclusive = New ExecutionDataflowBlockOptions With {.TaskScheduler = ce.ExclusiveScheduler}
        Dim concurrent = New ExecutionDataflowBlockOptions With {.TaskScheduler = ce.ConcurrentScheduler}

        ' When a new buyer arrives, notify them of any properties we may have
        m_subscribeBuyer = New ActionBlock(Of BuyerAgent)(
            Sub(buyer)
                m_buyers.Add(buyer)
                CurrentBuyers.Post(m_buyers.Select(Function(b) b.Name).ToArray())
                If (m_listings.Count > 0) Then buyer.AvailableRealtor.Post(Me)
            End Sub, exclusive)

        ' When a buyer queries us about potential listings that match their criteria,
        ' search and let them know if we found any
        m_search = New ActionBlock(Of Tuple(Of SearchFilter, WriteOnceBlock(Of Listing)))(
            Sub(requestResponse)
                Dim s = requestResponse.Item1
                For Each listing In m_listings
                    If (listing.Price >= s.MinPrice AndAlso listing.Price <= s.MaxPrice) Then
                        requestResponse.Item2.Post(listing)
                        Return
                    End If
                Next
                requestResponse.Item2.Post(Nothing)
            End Sub, concurrent)

        ' When a new listing is presented to us, notify all buyers that a new
        ' listing is available.
        m_listHouse = New ActionBlock(Of Listing)(
            Sub(listing)
                m_listings.Add(listing)
                CurrentListings.Post(m_listings.ToArray())
                If (m_buyers.Count > 0) Then
                    For Each buyer In m_buyers
                        buyer.AvailableRealtor.Post(Me)
                    Next
                End If
            End Sub, exclusive)

        ' When an offer is made, if it meets the listing and the listing is still
        ' available, sell.  Otherwise, reject.
        m_buy = New ActionBlock(Of Tuple(Of Offer, WriteOnceBlock(Of Boolean)))(
            Sub(requestResponse)
                Dim offer = requestResponse.Item1
                If (m_listings.Contains(offer.Listing) AndAlso
                    offer.Listing.Price <= offer.Price AndAlso
                    m_buyers.Contains(offer.Buyer)) Then
                    m_buyers.Remove(offer.Buyer)
                    m_listings.Remove(offer.Listing)

                    requestResponse.Item2.Post(True)

                    CurrentBuyers.Post(m_buyers.Select(Function(b) b.Name).ToArray())
                    CurrentListings.Post(m_listings.ToArray())
                Else
                    requestResponse.Item2.Post(False)
                End If
            End Sub, exclusive)
    End Sub

    ''' <summary>Asynchronously searches based on the specified filter criteria.</summary>
    ''' <param name="filter">The criteria with which to search.</param>
    ''' <returns>A task representing the result of the search.</returns>
    Public Function SearchAsync(filter As SearchFilter) As Task(Of Listing)
        Dim response = New WriteOnceBlock(Of Listing)(Function(i) i)
        m_search.Post(Tuple.Create(filter, response))
        Return response.ReceiveAsync()
    End Function

    ''' <summary>Asynchronously makes an offer.</summary>
    ''' <param name="offer">The offer to make</param>
    ''' <returns>A task representing whether the offer was accepted.</returns>
    Public Function MakeOfferAsync(offer As Offer) As Task(Of Boolean)
        Dim response = New WriteOnceBlock(Of Boolean)(Function(i) i)
        m_buy.Post(Tuple.Create(offer, response))
        Return response.ReceiveAsync()
    End Function

    ''' <summary>List a new house.</summary>
    ''' <param name="listing">The listing.</param>
    Public Sub ListHouse(listing As Listing)
        m_listHouse.Post(listing)
    End Sub

    ''' <summary>Take on a new client.</summary>
    ''' <param name="buyer">The buyer client.</param>
    Public Sub ServeBuyer(buyer As BuyerAgent)
        m_subscribeBuyer.Post(buyer)
    End Sub
End Class