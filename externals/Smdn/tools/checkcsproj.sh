#!/bin/sh

xbuild ./checkcsproj/checkcsproj.csproj /property:Configuration=Release
exec mono ./checkcsproj/bin/Release/checkcsproj.exe --base ../Smdn/Smdn-netfx2.0.csproj --combine Smdn.Net.Pop3.Client --combine Smdn.Net.Pop3.WebClients --combine Smdn.Net.Imap4.Client --combine Smdn.Net.Imap4.WebClients
