//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: RealEstateForm.cs
//
//--------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;

namespace RealEstateSimulation
{
    public partial class RealEstateForm : Form
    {
        public RealEstateForm() { InitializeComponent(); }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Create the realtor for the simulation
            var realtor = new RealtorAgent();

            // Create dataflow options for targetting the UI thread
            var ui = new ExecutionDataflowBlockOptions { TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext(), MaxMessagesPerTask = 1 };

            // Whenever a new activity log arrives, publish it to our running notebook
            var updateDisplayLog = new ActionBlock<string>(line =>
            {
                lbActivityLog.Items.Add(line);
                lbActivityLog.SelectedIndex = lbActivityLog.Items.Count-1;
            }, ui);

            // Whenever a listing update arrives, update the UI
            var updateCurrentListings = new ActionBlock<Listing[]>(listing =>
                txtActiveListings.Text = string.Join(Environment.NewLine, listing.Select(l => l.Name + ": $" + l.Price + ",000")), 
                ui);
            
            // Whenever a buyer update arrives, update the UI
            var updateCurrentBuyers = new ActionBlock<string[]>(buyers =>
                txtActiveBuyers.Text = string.Join(Environment.NewLine, buyers), 
                ui);

            // Link up the realtor to our UI publishing
            realtor.CurrentListings.LinkTo(updateCurrentListings);
            realtor.CurrentBuyers.LinkTo(updateCurrentBuyers);

            // Run simulation asynchronously
            var rand = new Random();
            while (true)
            {
                // Pause for a random period of time before adding to the simulation
                await TaskEx.Delay(rand.Next(0, 1000));

                // Randomly choose between adding a new buyer and adding a new listing
                if (rand.Next(2) == 0)
                {
                    // Add a new random listing
                    string description =
                        SupportingData.PropertyAdjectives[rand.Next(SupportingData.PropertyAdjectives.Length)] + " " +
                        SupportingData.PropertyTypes[rand.Next(SupportingData.PropertyTypes.Length)];
                    realtor.ListHouse(new Listing(description, rand.Next(200, 1000)));
                }
                else
                {
                    // Add a new random buyer
                    var min = rand.Next(200, 900);
                    var buyer = new BuyerAgent(SupportingData.PeopleNames[rand.Next(SupportingData.PeopleNames.Length)], min, rand.Next(min, Math.Min(min+100, 1000)));
                    buyer.ActivityLog.LinkTo(updateDisplayLog);
                    realtor.ServeBuyer(buyer);
                }
            }
        }
    }
}