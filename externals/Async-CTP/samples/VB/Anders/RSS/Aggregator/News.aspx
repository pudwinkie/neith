<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="News.aspx.vb" Inherits="RSSAggregator.News" Async="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Latest News From RSS Feeds</title>
</head>
<body style="font-family: Arial, Helvetica, sans-serif">
    <form id="form1" runat="server">
    <h1>Latest News <label id="lblTime" runat="server" /></h1>
    <asp:Repeater ID="NewsRepeater" runat="server" EnableViewState="false">
        <ItemTemplate>
            <h2><a href="<%# DataBinder.Eval(Container.DataItem, "Link") %>"><%# DataBinder.Eval(Container.DataItem, "Title") %></a></h2>
            <p style="font-size: small">
                <a href="<%# DataBinder.Eval(Container.DataItem, "SourceLink") %>"><%# DataBinder.Eval(Container.DataItem, "Source") %></a>
                <%# DataBinder.Eval(Container.DataItem, "PubDate")%>
            </p>
            <p><%# DataBinder.Eval(Container.DataItem, "Description") %></p>
            <br />
        </ItemTemplate>
    </asp:Repeater>
    </form>
</body>
</html>
