//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: RealtorAgent.cs
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RealEstateSimulation
{
    /// <summary>Represents a realtor in the real estate simulation.</summary>
    internal class RealtorAgent
    {
        /// <summary>Gets a source for querying the realtor's current buyers.</summary>
        public BroadcastBlock<string[]> CurrentBuyers { get; private set; }
        /// <summary>Gets a source for querying the realtor's current listings.</summary>
        public BroadcastBlock<Listing[]> CurrentListings { get; private set; }

        /// <summary>Gets a target for new listings the realtor should represent.</summary>
        private readonly ITargetBlock<Listing> m_listHouse;
        /// <summary>Gets a target for alerting the realtor to a new buyer.</summary>
        private readonly ITargetBlock<BuyerAgent> m_subscribeBuyer;
        /// <summary>Gets a target for querying the realtor for listings that match certain criteria..</summary>
        private readonly ITargetBlock<Tuple<SearchFilter, WriteOnceBlock<Listing>>> m_search;
        /// <summary>Gets a target for making an offer to the realtor to buy a particular listing.</summary>
        private readonly ITargetBlock<Tuple<Offer, WriteOnceBlock<bool>>> m_buy;

        /// <summary>A collection of the realtor's listings.</summary>
        private readonly HashSet<Listing> m_listings = new HashSet<Listing>();
        /// <summary>A collection of the realtor's buyers.</summary>
        private readonly HashSet<BuyerAgent> m_buyers = new HashSet<BuyerAgent>();

        /// <summary>Initializes the realtor agent.</summary>
        public RealtorAgent()
        {
            // Allow certain realtor processing to happen concurrently and some processing to happen exclusively
            var ce = new ConcurrentExclusiveSchedulerPair();
            var exclusive = new ExecutionDataflowBlockOptions { TaskScheduler = ce.ExclusiveScheduler };
            var concurrent = new ExecutionDataflowBlockOptions { TaskScheduler = ce.ConcurrentScheduler };

            // When a new buyer arrives, notify them of any properties we may have
            m_subscribeBuyer = new ActionBlock<BuyerAgent>(buyer =>
            {
                m_buyers.Add(buyer);
                CurrentBuyers.Post(m_buyers.Select(b => b.Name).ToArray());
                if (m_listings.Count > 0) buyer.AvailableRealtor.Post(this);
            }, exclusive);

            // When a buyer queries us about potential listings that match their criteria,
            // search and let them know if we found any
            m_search = new ActionBlock<Tuple<SearchFilter, WriteOnceBlock<Listing>>>(requestResponse =>
            {
                var s = requestResponse.Item1;
                foreach (var listing in m_listings)
                {
                    if (listing.Price >= s.MinPrice && listing.Price <= s.MaxPrice)
                    {
                        requestResponse.Item2.Post(listing);
                        return;
                    }
                }
                requestResponse.Item2.Post(null);
            }, concurrent);

            // When a new listing is presented to us, notify all buyers that a new
            // listing is available.
            m_listHouse = new ActionBlock<Listing>(listing =>
            {
                m_listings.Add(listing);
                CurrentListings.Post(m_listings.ToArray());
                if (m_buyers.Count > 0)
                {
                    foreach (var buyer in m_buyers) buyer.AvailableRealtor.Post(this);
                }
            }, exclusive);

            // When an offer is made, if it meets the listing and the listing is still
            // available, sell.  Otherwise, reject.
            m_buy = new ActionBlock<Tuple<Offer, WriteOnceBlock<bool>>>(requestResponse =>
            {
                var offer = requestResponse.Item1;
                if (m_listings.Contains(offer.Listing) &&
                    offer.Listing.Price <= offer.Price &&
                    m_buyers.Contains(offer.Buyer))
                {
                    m_buyers.Remove(offer.Buyer);
                    m_listings.Remove(offer.Listing);

                    requestResponse.Item2.Post(true);

                    CurrentBuyers.Post(m_buyers.Select(b => b.Name).ToArray());
                    CurrentListings.Post(m_listings.ToArray());
                }
                else requestResponse.Item2.Post(false);
            }, exclusive);

            CurrentBuyers = new BroadcastBlock<string[]>(_ => _);
            CurrentListings = new BroadcastBlock<Listing[]>(_ => _);
        }

        /// <summary>Asynchronously searches based on the specified filter criteria.</summary>
        /// <param name="filter">The criteria with which to search.</param>
        /// <returns>A task representing the result of the search.</returns>
        public Task<Listing> SearchAsync(SearchFilter filter)
        {
            var response = new WriteOnceBlock<Listing>(_ => _);
            m_search.Post(Tuple.Create(filter, response));
            return response.ReceiveAsync();
        }

        /// <summary>Asynchronously makes an offer.</summary>
        /// <param name="offer">The offer to make</param>
        /// <returns>A task representing whether the offer was accepted.</returns>
        public Task<bool> MakeOfferAsync(Offer offer)
        {
            var response = new WriteOnceBlock<bool>(_ => _);
            m_buy.Post(Tuple.Create(offer, response));
            return response.ReceiveAsync();
        }

        /// <summary>List a new house.</summary>
        /// <param name="listing">The listing.</param>
        public void ListHouse(Listing listing) { m_listHouse.Post(listing); }

        /// <summary>Take on a new client.</summary>
        /// <param name="buyer">The buyer client.</param>
        public void ServeBuyer(BuyerAgent buyer) { m_subscribeBuyer.Post(buyer); }
    }
}