/*************************************************************************
 *
 * File: example.js
 *
 * Description: 
 * Script that controls the gadget functionality for the 
 * "Flyout" Sidebar gadget sample. 
 * 
 * This file is part of the Microsoft Windows SDK Code Samples.
 * 
 * Copyright (C) Microsoft Corporation.  All rights reserved.
 * 
 * This source code is intended only as a supplement to Microsoft
 * Development Tools and/or on-line documentation.  See these other
 * materials for detailed information regarding Microsoft code samples.
 * 
 * THIS CODE AND INFORMATION ARE PROVIDED AS IS WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
 * PARTICULAR PURPOSE.
 * 
 ************************************************************************/
 
// Member variables.
var oFlyoutDocument;

// --------------------------------------------------------------------
// Initialize the gadget.
// --------------------------------------------------------------------
function Init()
{
    // Specify the flyout root.
    System.Gadget.Flyout.file = "flyout.html";
    
    // Initialize the Flyout state display.
    if (!System.Gadget.Flyout.show)
    {
        strFlyoutFeedback.innerText = "Flyout hidden.";
    }

    // Initialize the time display.
    DisplayTime();
	setInterval("DisplayTime()",1000);
}

// --------------------------------------------------------------------
// Display the system time.
// --------------------------------------------------------------------
function DisplayTime()
{
    // Retrieve the local time.
    var sTimeInfo = System.Time.getLocalTime(System.Time.currentTimeZone);
    var dDateInfo = new Date(Date.parse(sTimeInfo));   
    var tHours = dDateInfo.getHours();
    var tMinutes = dDateInfo.getMinutes();
    tMinutes = ((tMinutes < 10) ? ":0" : ":") + tMinutes
    var tSeconds = dDateInfo.getSeconds();
    tSeconds = ((tSeconds < 10) ? ":0" : ":") + tSeconds;
    strGadgetTimeDisplay.innerHTML = tHours + tMinutes + tSeconds;
    if (oFlyoutDocument)
    {
        oFlyoutDocument.getElementById("strFlyoutTimeDisplay").innerText = strGadgetTimeDisplay.innerHTML;
    }
}

// --------------------------------------------------------------------
// Display the flyout associated with the "Flyout" gadget sample.
// --------------------------------------------------------------------
function showFlyout()
{
    System.Gadget.Flyout.show = true;
    oFlyoutDocument = System.Gadget.Flyout.document;
}
