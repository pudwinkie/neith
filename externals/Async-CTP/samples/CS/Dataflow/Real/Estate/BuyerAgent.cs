//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: BuyerAgent.cs
//
//--------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Threading;

namespace RealEstateSimulation
{
    /// <summary>Represents a buyer in the real estate simulation.</summary>
    internal class BuyerAgent
    {
        /// <summary>Gets the name of the buyer.</summary>
        public string Name { get; private set; }

        /// <summary>The buyer's search criteria.</summary>
        private SearchFilter m_searchFilter;

        /// <summary>Gets a target for alerting the buyer to realtor having new listings available.</summary>
        public ITargetBlock<RealtorAgent> AvailableRealtor { get { return m_availableRealtor; } }
        /// <summary>A buffer of realtors with new listings available.</summary>
        private BufferBlock<RealtorAgent> m_availableRealtor = new BufferBlock<RealtorAgent>();

        /// <summary>Gets a source for activity information published by the buyer.</summary>
        public ISourceBlock<string> ActivityLog { get { return m_activityLog; } }
        /// <summary>A buffer of activity information published by the buyer.</summary>
        private BroadcastBlock<string> m_activityLog = new BroadcastBlock<string>(_ => _);

        /// <summary>Initializes the buyer agent.</summary>
        /// <param name="name">The name of the buyer.</param>
        /// <param name="minPrice">The minimum price the buyer wants to spend.</param>
        /// <param name="maxPrice">The maximum price the buyer wants to spend.</param>
        public BuyerAgent(string name, int minPrice, int maxPrice)
        {
            // Initialize the buyer and kick off its processing asynchronously
            Name = name;
            m_searchFilter = new SearchFilter(minPrice, maxPrice);
            RunAsync();
        }

        /// <summary>Asynchronously runs the buyer's processing.</summary>
        private void RunAsync()
        {
            TaskEx.Run(async delegate
            {
                while (true)
                {
                    // Get the next realtor to work with and ask the realtor for a house in our range
                    // If the realtor does not have any house available for us to see, try again.
                    var realtor = await m_availableRealtor.ReceiveAsync();
                    var house = await realtor.SearchAsync(m_searchFilter);
                    if (house == null) continue;

                    // We found a house in our price range.  Make an offer!
                    var offer = new Offer(this, house, Math.Max(m_searchFilter.MinPrice, house.Price));
                    m_activityLog.Post(string.Format("{0}: I'd like to buy the {1} for ${2},000.", Name, offer.Listing.Name, offer.Price));
                    var accepted = await realtor.MakeOfferAsync(offer);

                    // If our offer is accepted, we own a house!
                    if (accepted)
                    {
                        m_activityLog.Post(string.Format("\t{0}: I bought a home!!!", Name));
                        break;
                    }
                    // Otherwise, try again.
                    else
                    {
                        m_activityLog.Post(string.Format("\t{0}: My offer was rejected.", Name));
                    }
                }
            });
        }
    }
}