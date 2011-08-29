'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: QuoteCalculatorService.cs
'
'--------------------------------------------------------------------------

Imports System.Net
Imports System.Threading.Tasks
Imports System.Collections.ObjectModel

Public Class QuoteCalculatorService
    Implements IQuoteCalculatorService

    Public Function GetQuote(ByVal ticker As String) As Quote 'Implements IQuoteCalculatorService.GetQuote
        Dim price As Double
        Dim change As Double

        Dim quoteValues As String() = (New WebClient()).DownloadString("http://download.finance.yahoo.com/d/quotes.csv?s=" & ticker & "&f=l1c1n").Split(",")
        Double.TryParse(quoteValues(0), price)
        Double.TryParse(quoteValues(1), change)

        Return New Quote() With {.Ticker = ticker, .Price = price, .Change = change}
    End Function

    Public Async Function GetQuoteAsync(ByVal ticker As String) As Task(Of Quote)
        Dim price As Double
        Dim change As Double

        Dim quoteValues As String() = (Await New WebClient().DownloadStringTaskAsync("http://download.finance.yahoo.com/d/quotes.csv?s=" & ticker & "&f=l1c1n")).Split(",")
        Double.TryParse(quoteValues(0), price)
        Double.TryParse(quoteValues(1), change)

        Return New Quote() With {.Ticker = ticker, .Price = price, .Change = change}
    End Function

    Public Function GetQuotes(ByVal tickers As String()) As ReadOnlyCollection(Of Quote) 'Implements IQuoteCalculatorService.GetQuotes
        Return New ReadOnlyCollection(Of Quote)(
            (From ticker In tickers Select GetQuote(ticker)).ToArray())
    End Function

    Public Async Function GetQuotesAsync(ByVal tickers As String()) As Task(Of ReadOnlyCollection(Of Quote))
        Return New ReadOnlyCollection(Of Quote)(
            Await TaskEx.WhenAll(From ticker In tickers Select GetQuoteAsync(ticker)))
    End Function

#Region "APM Begin/End Wrappers"
    Public Function BeginGetQuote(ByVal ticker As String, ByVal callback As AsyncCallback, ByVal state As Object) As IAsyncResult Implements IQuoteCalculatorService.BeginGetQuote
        Return WithApmCallback(GetQuoteAsync(ticker), callback, state)
    End Function

    Public Function EndGetQuote(ByVal result As IAsyncResult) As Quote Implements IQuoteCalculatorService.EndGetQuote
        Return CType(result, Task(Of Quote)).Result
    End Function

    Public Function BeginGetQuotes(ByVal tickers As String(), ByVal callback As AsyncCallback, ByVal state As Object) As IAsyncResult Implements IQuoteCalculatorService.BeginGetQuotes
        Return WithApmCallback(GetQuotesAsync(tickers), callback, state)
    End Function

    Public Function EndGetQuotes(ByVal result As IAsyncResult) As ReadOnlyCollection(Of Quote) Implements IQuoteCalculatorService.EndGetQuotes
        Return CType(result, Task(Of ReadOnlyCollection(Of Quote))).Result
    End Function

    Private Shared Function WithApmCallback(Of T)(ByVal task As Task(Of T), ByVal callback As AsyncCallback, ByVal state As Object)
        If (task Is Nothing) Then Throw New ArgumentNullException("task")
        Dim tcs = New TaskCompletionSource(Of T)(state)
        task.ContinueWith(
            Sub(completed)
                Select Case task.Status
                    Case TaskStatus.RanToCompletion
                        tcs.TrySetResult(task.Result)
                    Case TaskStatus.Faulted
                        tcs.TrySetException(task.Exception.InnerExceptions)
                    Case TaskStatus.Canceled
                        tcs.TrySetCanceled()
                End Select
                If (callback IsNot Nothing) Then callback(tcs.Task)
            End Sub, TaskContinuationOptions.ExecuteSynchronously)
        Return tcs.Task
    End Function
#End Region

End Class
