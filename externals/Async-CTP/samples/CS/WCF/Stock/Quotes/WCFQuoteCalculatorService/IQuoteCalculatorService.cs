//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: IQuoteCalculatorService.cs
//
//--------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace WCFTaskAsyncSample
{
    [ServiceContract]
    public interface IQuoteCalculatorService
    {
        //[OperationContract]
        //Quote GetQuote(string ticker);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetQuote(string ticker, AsyncCallback callback, object state);
        Quote EndGetQuote(IAsyncResult result);
        
        //[OperationContract]
        //ReadOnlyCollection<Quote> GetQuotes(string[] tickers);

        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetQuotes(string[] tickers, AsyncCallback callback, object state);
        ReadOnlyCollection<Quote> EndGetQuotes(IAsyncResult result);
    }

    [DataContract]
    public class Quote
    {
        [DataMember]
        public string Ticker { get; set; }

        [DataMember]
        public double Price { get; set; }

        [DataMember]
        public double Change { get; set; }
    }
}
