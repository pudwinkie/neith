'--------------------------------------------------------------------------
' 
'  Copyright (c) Microsoft Corporation.  All rights reserved. 
' 
'  File: IQuoteCalculatorService.cs
'
'--------------------------------------------------------------------------

Imports System.Collections.ObjectModel

<ServiceContract()>
Public Interface IQuoteCalculatorService

    '<OperationContract()>
    'Function GetQuote( ticker As String) As Quote

    <OperationContract(AsyncPattern:=True)>
    Function BeginGetQuote(ticker As String, callback As AsyncCallback, state As Object) As IAsyncResult
    Function EndGetQuote(result As IAsyncResult) As Quote

    '<OperationContract()>
    'Function GetQuotes( tickers As String()) As ReadOnlyCollection(Of Quote)

    <OperationContract(AsyncPattern:=True)>
    Function BeginGetQuotes(tickers As String(), callback As AsyncCallback, state As Object) As IAsyncResult
    Function EndGetQuotes(result As IAsyncResult) As ReadOnlyCollection(Of Quote)

End Interface

<DataContract()>
Public Class Quote

    <DataMember()>
    Public Property Ticker() As String

    <DataMember()>
    Public Property Price() As Double

    <DataMember()>
    Public Property Change() As Double

End Class