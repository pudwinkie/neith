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
var scaleDocked = 1.0;
var scaleUndocked = 2.0;
 
// Declare the dock and undock event handlers.
System.Gadget.onDock = dockState;
System.Gadget.onUndock = dockState;
    
// --------------------------------------------------------------------
// Check the gadget state and set the appearance accordingly.
// --------------------------------------------------------------------
function dockState()
{
    var oBody = document.body.style;
	System.Gadget.beginTransition();	
	if (System.Gadget.docked) 
	{
		oBody.width = gadgetWidth*scaleDocked;
		oBody.height = gadgetHeight*scaleDocked;
		
        //imgBackground.src = 'url(../images/bg_docked.png)';
        System.Gadget.background = "../images/bg_docked.png";
        imgBackground.opacity = 0;
        
        txtDocked.className = 'gadgetDocked';
        txtDocked.innerText = 'Docked Mode';
    }
    else
    {
		oBody.width = gadgetWidth*scaleUndocked;
		oBody.height = gadgetHeight*scaleUndocked;
		
//        imgBackground.src = 'url(../images/bg_undocked.png)';
        System.Gadget.background = "../images/bg_undocked.png";
        imgBackground.height = 260;
        imgBackground.width = 216;
                
        txtDocked.className = 'gadgetUndocked';
        txtDocked.innerText = 'Undocked Mode';
    }
	System.Gadget.endTransition(System.Gadget.TransitionType.morph, 5.0);
}

