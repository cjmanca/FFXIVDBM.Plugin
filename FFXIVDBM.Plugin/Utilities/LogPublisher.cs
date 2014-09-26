// FFXIVDBM.Plugin
// LogPublisher.cs
// 
// Copyright © 2007 - 2014 Ryan Wilson - All Rights Reserved
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions are met: 
// 
//  * Redistributions of source code must retain the above copyright notice, 
//    this list of conditions and the following disclaimer. 
//  * Redistributions in binary form must reproduce the above copyright 
//    notice, this list of conditions and the following disclaimer in the 
//    documentation and/or other materials provided with the distribution. 
//  * Neither the name of SyndicatedLife nor the names of its contributors may 
//    be used to endorse or promote products derived from this software 
//    without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
// POSSIBILITY OF SUCH DAMAGE. 

using System;
using FFXIVAPP.Common.Core.Memory;
using FFXIVAPP.Common.Utilities;
using NLog;
using FFXIVDBM.Plugin.Properties;
using FFXIVDBM.Plugin.Views;

namespace FFXIVDBM.Plugin.Utilities
{
    public static class LogPublisher
    {
        public static void Process(ChatLogEntry chatEntry)
        {
            try
            {
                var timeStampColor = Settings.Default.TimeStampColor.ToString();
                var timeStamp = chatEntry.TimeStamp.ToString("[HH:mm:ss] ");
                var line = chatEntry.Line.Replace("  ", " ");
                var color = (Constants.Colors.ContainsKey(chatEntry.Code)) ? Constants.Colors[chatEntry.Code][0] : "FFFFFF";
                FFXIVAPP.Common.Constants.FD.AppendFlow(timeStamp, "", line, new[]
                {
                    timeStampColor, "#" + color
                }, MainView.View.ChatLogFD._FDR);
            }
            catch (Exception ex)
            {
                Logging.Log(LogManager.GetCurrentClassLogger(), "", ex);
            }
        }
    }
}
