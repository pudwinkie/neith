/*************************************************************************
 *
 * File: Example.js
 *
 * Description: 
 * Script that controls the presentation functionality for the 
 * "Graphic" Sidebar gadget sample. 
 *
 * Note: All user input should be validated.
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

// --------------------------------------------------------------------
// Initialize the gadget.
// --------------------------------------------------------------------
function initGadget()
{
    // Display a known image path as the default for all gadget operations.
    txtImagePath.value = System.Gadget.path + "\\images\\aerologo.png";
}

// --------------------------------------------------------------------
// Add an image to the background.
// --------------------------------------------------------------------
function addImage(file) 
{
    var oBoundingRect = fldsetAddImage.getBoundingClientRect();
    var img = new Image();
    img.src = file;

    // Add the image. Position it somewhere in the fieldset rect.
    var imgGlow = imgBackground.addImageObject(
        file, oBoundingRect.right - img.width, oBoundingRect.top);
    imgGlow.opacity = 50;
    imgGlow.addGlow("black",50,50);
    // Alternative method for specifying the color parameter.
    // imgGlow.addGlow("Color(255, 255, 0, 0)",50,25);    
}

// --------------------------------------------------------------------
// Add text to the background.
// --------------------------------------------------------------------
function addText(text)
{
    var oSrcElement = window.event.srcElement;
    var oBoundingRect = oSrcElement.getBoundingClientRect();
    
    // Add the text. Position it next to the button.
    var txtEcho = imgBackground.addTextObject(
        text, "Verdana", 12, "Red", oBoundingRect.right, oBoundingRect.top);

    txtEcho.value = text;
}

// --------------------------------------------------------------------
// Remove all image and text elements added to the background since load.
// --------------------------------------------------------------------
function removeElements()
{
    imgBackground.removeObjects();
}

// --------------------------------------------------------------------
// Add an image to the gadget DOM using the gimage protocol.
// --------------------------------------------------------------------
function addGIMAGE(file)
{
    var dimensions = "?width=25&height=25";
    var oGIMAGE = document.createElement("img");
    oGIMAGE.src = "gimage:///" + file + dimensions;
    oGIMAGE.id = "imgGIMAGEx";
    // Add the gimage element as a child of the related fieldset.
    fldsetGIMAGE.appendChild(oGIMAGE);
}

// --------------------------------------------------------------------
// Find an image that uses the gimage protocol.
// --------------------------------------------------------------------
function findGIMAGE()
{
    // Find the gimage element added to the DOM in the addGIMAGE() function.
    var oGIMAGE = document.getElementById("imgGIMAGEx");
    // Highlight the image if found.
    if (oGIMAGE)
    {
        oGIMAGE.style.borderStyle = "solid";
        oGIMAGE.style.borderWidth = "1px";
        oGIMAGE.style.borderColor = "Red";
    }
}

// --------------------------------------------------------------------
// Switch (or load) an image using the gimage protocol.
// --------------------------------------------------------------------
function switchGIMAGE(file)
{
    // Specify the height, width, and interpolation method.
    imgGIMAGE.style.height = 25;
    imgGIMAGE.style.width = 25;
    imgGIMAGE.style.msInterpolationMode = "bicubic";
    // Replace the placeholder image.
    imgGIMAGE.src = "gimage:///" + file;
}

