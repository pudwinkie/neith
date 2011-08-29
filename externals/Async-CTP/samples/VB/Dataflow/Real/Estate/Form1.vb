'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: Form1.vb
'
'--------------------------------------------------------------------------

Imports System.Threading.Tasks
Imports System.Threading.Tasks.Dataflow

Public Class RealEstateForm

    Private Async Sub RealEstateForm_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

        ' Create the realtor for the simulation
        Dim realtor = New RealtorAgent()

        ' Create dataflow options for targetting the UI thread
        Dim ui = New ExecutionDataflowBlockOptions With {.TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext(), .MaxMessagesPerTask = 1}

        ' Whenever a new activity log arrives, publish it to our running notebook
        Dim updateDisplayLog = New ActionBlock(Of String)(
            Sub(line)
                lbActivityLog.Items.Add(line)
                lbActivityLog.SelectedIndex = lbActivityLog.Items.Count - 1
            End Sub, ui)

        ' Whenever a listing update arrives, update the UI
        Dim updateCurrentListings = New ActionBlock(Of Listing())(
            Sub(listing)
                txtActiveListings.Text = String.Join(Environment.NewLine, listing.Select(Function(l) l.Name & ": $" + l.Price.ToString() & ",000"))
            End Sub, ui)

        ' Whenever a buyer update arrives, update the UI
        Dim updateCurrentBuyers = New ActionBlock(Of String())(Sub(buyers) txtActiveBuyers.Text = String.Join(Environment.NewLine, buyers), ui)

        ' Link up the realtor to our UI publishing
        realtor.CurrentListings.LinkTo(updateCurrentListings)
        realtor.CurrentBuyers.LinkTo(updateCurrentBuyers)

        ' Run simulation asynchronously
        Dim rand = New Random()
        While (True)
            ' Pause for a random period of time before adding to the simulation
            Await TaskEx.Delay(rand.Next(0, 1000))

            ' Randomly choose between adding a new buyer and adding a new listing
            If (rand.Next(2) = 0) Then
                ' Add a new random listing
                Dim description =
                    SupportingData.PropertyAdjectives(rand.Next(SupportingData.PropertyAdjectives.Length)) & " " &
                    SupportingData.PropertyTypes(rand.Next(SupportingData.PropertyTypes.Length))
                realtor.ListHouse(New Listing(description, rand.Next(200, 1000)))
            Else
                ' Add a new random buyer
                Dim min = rand.Next(200, 900)
                Dim buyer = New BuyerAgent(SupportingData.PeopleNames(rand.Next(SupportingData.PeopleNames.Length)), min, rand.Next(min, Math.Min(min + 100, 1000)))
                buyer.ActivityLog.LinkTo(updateDisplayLog)
                realtor.ServeBuyer(buyer)
            End If
        End While

    End Sub

End Class