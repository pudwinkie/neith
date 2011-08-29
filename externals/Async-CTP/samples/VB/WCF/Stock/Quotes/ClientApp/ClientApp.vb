'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: ClientApp.cs
'
'--------------------------------------------------------------------------

Imports System.Threading.Tasks
Imports ClientApp.WCFService

Module ClientApp

    Sub Main()
        Dim tickers As String() = {"MSFT", "C", "YHOO", "GOOG", "GE", "FOO"}

        ComputeStockPricesAsync(tickers).ContinueWith(
            Sub(completed)
                Console.WriteLine("All the stock prices have been obtained.")
            End Sub)

        Console.WriteLine("Wait for the stock prices to be printed out or hit ENTER to exit...\n")
        Console.ReadLine()
    End Sub

    Async Function ComputeStockPricesAsync(tickers As String()) As Task
        For Each ticker As String In tickers
            Try
                Dim quoteTask = New QuoteCalculatorServiceClient().GetQuoteAsync(ticker)
                If (quoteTask Is Await TaskEx.WhenAny(quoteTask, TaskEx.Delay(10000))) Then
                    Dim quote = Await quoteTask
                    Console.WriteLine("Ticker: " + quote.Ticker)
                    Console.WriteLine(vbTab & "Price: " + If(Not quote.Price.Equals(0.0), quote.Price.ToString(), "Unknown"))
                    Console.WriteLine(vbTab & "Change of the day: " + If(Not quote.Change.Equals(0.0), quote.Change.ToString(), "Unknown"))
                    Console.WriteLine()
                Else
                    Console.WriteLine("Timed out")
                End If
            Catch e As Exception
                Console.WriteLine(e.Message)
                Console.WriteLine()
            End Try
        Next
    End Function

End Module