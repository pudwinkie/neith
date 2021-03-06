/*************************************************************************
 *
 * File: Docked.js
 *
 * Description: 
 * Script that handles the dock and undock events for the 
 * "Docked" Sidebar gadget sample. 
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
 
// Gadget width and height.
var gadgetWidth = 130;
var gadgetHeight = 108;

// Amount to scale gadget when docked or undocked.
var scaleDocked = 1;
var scaleUndocked = 2;

// Amount of time desired to perform transition (in seconds).
var timeTransition = 2;

// Declare the dock and undock event handlers.
System.Gadget.onDock = CheckDockState;
System.Gadget.onUndock = CheckDockState;

// --------------------------------------------------------------------
// Check the gadget dock state; set the gadget style.
// --------------------------------------------------------------------
function CheckDockState()
{
    var oBackground = document.getElementById("imgBackground");
    System.Gadget.beginTransition();

    var oBody = document.body.style;
    if (System.Gadget.docked)
    {
	    oBody.width = gadgetWidth*scaleDocked;
	    oBody.height = gadgetHeight*scaleDocked;
	
        oBackground.src = "../images/bg_docked.png";
        
        txtDocked.innerText = 'Docked';
        txtDocked.className = 'gadgetDocked';
    }
    else
    {
        oBody.width = gadgetWidth*scaleUndocked;
        oBody.height = gadgetHeight*scaleUndocked;  
          
        oBackground.src = "../images/bg_undocked.png";

        txtDocked.innerText = 'Undocked';
        txtDocked.className = 'gadgetUndocked';
    }
    System.Gadget.endTransition(System.Gadget.TransitionType.morph, timeTransition);
}