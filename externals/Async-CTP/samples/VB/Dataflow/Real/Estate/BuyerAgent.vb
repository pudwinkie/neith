'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: BuyerAgent.vb
'
'--------------------------------------------------------------------------

Imports System.Threading
Imports System.Threading.Tasks
Imports System.Threading.Tasks.Dataflow

Friend Class BuyerAgent
    ''' <summary>Gets the name of the buyer.</summary>
    Public Property Name As String

    ''' <summary>The buyer's search criteria.</summary>
    Private m_searchFilter As SearchFilter

    ''' <summary>Gets a target for alerting the buyer to realtor having new listings available.</summary>
    Public ReadOnly Property AvailableRealtor As ITargetBlock(Of RealtorAgent)
        Get
            Return m_availableRealtor
        End Get
    End Property

    ''' <summary>A buffer of realtors with new listings available.</summary>
    Private m_availableRealtor As New BufferBlock(Of RealtorAgent)

    ''' <summary>Gets a source for activity information published by the buyer.</summary>
    Public ReadOnly Property ActivityLog As ISourceBlock(Of String)
        Get
            Return m_activityLog
        End Get
    End Property

    ''' <summary>A buffer of activity information published by the buyer.</summary>
    Private m_activityLog As New BroadcastBlock(Of String)(Function(i) i)

    ''' <summary>Initializes the buyer agent.</summary>
    ''' <param name="name">The name of the buyer.</param>
    ''' <param name="minPrice">The minimum price the buyer wants to spend.</param>
    ''' <param name="maxPrice">The maximum price the buyer wants to spend.</param>
    Public Sub New(name As String, minPrice As Integer, maxPrice As Integer)
        ' Initialize the buyer and kick off its processing asynchronously
        Me.Name = name
        m_searchFilter = New SearchFilter(minPrice, maxPrice)
        RunAsync()
    End Sub

    ''' <summary>Asynchronously runs the buyer's processing.</summary>
    Private Sub RunAsync()
        TaskEx.Run(
            Async Sub()
                While (True)
                    ' Get the next realtor to work with and ask the realtor for a house in our range
                    ' If the realtor does not have any house available for us to see, try again.
                    Dim realtor = Await m_availableRealtor.ReceiveAsync()
                    Dim house = Await realtor.SearchAsync(m_searchFilter)
                    If (house IsNot Nothing) Then
                        ' We found a house in our price range.  Make an offer!
                        Dim offer = New Offer(Me, house, Math.Max(m_searchFilter.MinPrice, house.Price))
                        m_activityLog.Post(String.Format("{0}: I'd like to buy the {1} for ${2},000.", Name, offer.Listing.Name, offer.Price))
                        Dim accepted = Await realtor.MakeOfferAsync(offer)

                        ' If our offer is accepted, we own a house!
                        If (accepted) Then
                            m_activityLog.Post(String.Format(vbTab & "{0}: I bought a home!!!", Name))
                            Exit While
                        Else
                            ' Otherwise, try again.
                            m_activityLog.Post(String.Format(vbTab & "{0}: My offer was rejected.", Name))
                        End If
                    End If
                End While
            End Sub)
    End Sub
End Class