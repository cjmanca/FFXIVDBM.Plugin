// FFXIVDBM.Plugin
// Initializer.cs
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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using FFXIVAPP.Common.Controls;
using FFXIVDBM.Plugin.Properties;
using FFXIVDBM.Plugin.Views;

namespace FFXIVDBM.Plugin
{
    internal static class Initializer
    {
        #region Declarations

        #endregion

        /// <summary>
        /// </summary>
        public static void LoadSettings()
        {
            if (Constants.XSettings != null)
            {
                foreach (var xElement in Constants.XSettings.Descendants()
                                                  .Elements("Setting"))
                {
                    var xKey = (string)xElement.Attribute("Key");
                    var xValue = (string)xElement.Element("Value");
                    if (String.IsNullOrWhiteSpace(xKey) || String.IsNullOrWhiteSpace(xValue))
                    {
                        return;
                    }
                    Settings.Default.SetValue(xKey, xValue, CultureInfo.InvariantCulture);
                    if (!Constants.Settings.Contains(xKey))
                    {
                        Constants.Settings.Add(xKey);
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        public static void ApplyTheming()
        {
            SetupFont(ref MainView.View.ChatLogFD);
            SetupColor(ref MainView.View.ChatLogFD);
        }

        private static void SetupFont(ref xFlowDocument flowDoc)
        {
            var font = Settings.Default.ChatFont;
            flowDoc._FD.FontFamily = new FontFamily(font.Name);
            flowDoc._FD.FontWeight = font.Bold ? FontWeights.Bold : FontWeights.Regular;
            flowDoc._FD.FontStyle = font.Italic ? FontStyles.Italic : FontStyles.Normal;
            flowDoc._FD.FontSize = font.Size;
        }

        private static void SetupColor(ref xFlowDocument flowDoc)
        {
            flowDoc._FD.Background = new SolidColorBrush(Settings.Default.ChatBackgroundColor);
        }
    }
}
