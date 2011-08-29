//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: DataTypes.cs
//
//--------------------------------------------------------------------------

using System.Threading.Tasks.Dataflow;

namespace RealEstateSimulation
{
    /// <summary>Represents a real-estate listing.</summary>
    internal sealed class Listing
    {
        /// <summary>Initializes the listing.<summary>
        /// <param name="name">The name of the listing.</param>
        /// <param name="price">The price of the listing.</param>
        public Listing(string name, int price) { Name = name; Price = price; }

        /// <summary>Gets the name of the listing.</summary>
        public string Name { get; private set; }
        /// <summary>Gets the price of the listing.</summary>
        public int Price { get; private set; }
    }

    /// <summary>Represents a search for listings that match certain criteria.</summary>
    internal sealed class SearchFilter
    {
        /// <summary>Initializes the filter.</summary>
        /// <param name="minPrice">The minimum price for listings to select.</param>
        /// <param name="maxPrice">The maximum price for listings to select.</param>
        public SearchFilter(int minPrice, int maxPrice) { MinPrice = minPrice; MaxPrice = maxPrice; }

        /// <summary>Gets the minimum price for listings to select.</summary>
        public int MinPrice { get; private set; }
        /// <summary>Gets the maximum price for listings to select.</summary>
        public int MaxPrice { get; private set; }
    }

    /// <summary>Represents an offer on a listing.</summary>
    internal sealed class Offer
    {
        /// <summary>Initializes the offer.</summary>
        /// <param name="buyer">The buyer making the offer.</param>
        /// <param name="listing">The listing on which the offer is being made.</param>
        /// <param name="price">The price of the offer.</param>
        public Offer(BuyerAgent buyer, Listing listing, int price) { Buyer = buyer; Listing = listing; Price = price; }

        /// <summary>Gets the buyer making the offer.</summary>
        public BuyerAgent Buyer { get; private set; }
        /// <summary>Gets the listing on which the offer is being made.</summary>
        public Listing Listing { get; private set; }
        /// <summary>Gets the price of the offer..</summary>
        public int Price { get; private set; }
    }
}