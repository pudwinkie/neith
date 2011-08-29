//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: QuoteCalculatorService.cs
//
//--------------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace WCFTaskAsyncSample
{
    public class QuoteCalculatorService : IQuoteCalculatorService
    {
        public Quote GetQuote(string ticker)
        {
            double price, change;

            string[] quoteValues = new WebClient().DownloadString("http://download.finance.yahoo.com/d/quotes.csv?s=" + ticker + "&f=l1c1n").Split(',');
            double.TryParse(quoteValues[0], out price);
            double.TryParse(quoteValues[1], out change);

            return new Quote() { Ticker = ticker, Price = price, Change = change};
        }

        public async Task<Quote> GetQuoteAsync(string ticker)
        {
            double price, change;

            string[] quoteValues = (await new WebClient().DownloadStringTaskAsync("http://download.finance.yahoo.com/d/quotes.csv?s=" + ticker + "&f=l1c1n")).Split(',');
            double.TryParse(quoteValues[0], out price);
            double.TryParse(quoteValues[1], out change);

            return new Quote() { Ticker = ticker, Price = price, Change = change };
        }

        public ReadOnlyCollection<Quote> GetQuotes(string[] tickers)
        {
            return new ReadOnlyCollection<Quote>(
                (from ticker in tickers select GetQuote(ticker)).ToArray());
        }

        public async Task<ReadOnlyCollection<Quote>> GetQuotesAsync(string[] tickers)
        {
            return new ReadOnlyCollection<Quote>(
                await TaskEx.WhenAll(from ticker in tickers select GetQuoteAsync(ticker)));
        }

        #region APM Begin/End Wrappers
        public IAsyncResult BeginGetQuote(string ticker, AsyncCallback callback, object state) {
            return WithApmCallback(GetQuoteAsync(ticker), callback, state);
        }
        public Quote EndGetQuote(IAsyncResult result) { return ((Task<Quote>)result).Result; }

        public IAsyncResult BeginGetQuotes(string[] tickers, AsyncCallback callback, object state) {
            return WithApmCallback(GetQuotesAsync(tickers), callback, state);
        }
        public ReadOnlyCollection<Quote> EndGetQuotes(IAsyncResult result) { return ((Task<ReadOnlyCollection<Quote>>)result).Result; }

        private static Task<T> WithApmCallback<T>(Task<T> task, AsyncCallback callback, object state)
        {
            if (task == null) throw new ArgumentNullException("task");
            var tcs = new TaskCompletionSource<T>(state);
            task.ContinueWith(delegate
            {
                switch (task.Status)
                {
                    case TaskStatus.RanToCompletion: tcs.TrySetResult(task.Result); break;
                    case TaskStatus.Faulted: tcs.TrySetException(task.Exception.InnerExceptions); break;
                    case TaskStatus.Canceled: tcs.TrySetCanceled(); break;
                }
                if (callback != null) callback(tcs.Task);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }
        #endregion
    }
}