//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: ClientApp.cs
//
//--------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using ClientApp.WCFService;

namespace ClientStockApp
{
    class ClientApp
    {
        static void Main(string[] args)
        {
            string[] tickers = { "MSFT", "C", "YHOO", "GOOG", "GE", "FOO" };

            ComputeStockPricesAsync(tickers).ContinueWith(
                delegate { Console.WriteLine("All the stock prices have been obtained."); });

            Console.WriteLine("Wait for the stock prices to be printed out or hit ENTER to exit...\n");
            Console.ReadLine();
        }

        static async Task ComputeStockPricesAsync(string[] tickers)
        {
            foreach (string ticker in tickers)
            {
                try
                {
                    var quoteTask = new QuoteCalculatorServiceClient().GetQuoteAsync(ticker);
                    if (quoteTask == await TaskEx.WhenAny(quoteTask, TaskEx.Delay(10000)))
                    {
                        Quote quote = await quoteTask;
                        Console.WriteLine("Ticker: " + quote.Ticker);
                        Console.WriteLine("\tPrice: " + (!quote.Price.Equals(0.0) ? quote.Price.ToString() : "Unknown"));
                        Console.WriteLine("\tChange of the day: " + (!quote.Change.Equals(0.0) ? quote.Change.ToString() : "Unknown"));
                        Console.WriteLine();
                    }
                    else Console.WriteLine("Timed out");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                }
            }
        }
    }
}