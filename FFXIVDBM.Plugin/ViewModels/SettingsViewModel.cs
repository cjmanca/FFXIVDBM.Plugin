// FFXIVDBM.Plugin
// SettingsViewModel.cs
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FFXIVAPP.Common.Events;
using FFXIVAPP.Common.Models;
using FFXIVAPP.Common.ViewModelBase;
using FFXIVDBM.Plugin.Views;

namespace FFXIVDBM.Plugin.ViewModels
{
    internal sealed class SettingsViewModel : INotifyPropertyChanged
    {
        #region Property Bindings

        private static SettingsViewModel _instance;

        public static SettingsViewModel Instance
        {
            get { return _instance ?? (_instance = new SettingsViewModel()); }
        }

        #endregion

        #region Declarations

        public ICommand ClearChatLogCommand { get; private set; }

        #endregion

        public SettingsViewModel()
        {
            ClearChatLogCommand = new DelegateCommand(ClearChatLog);
        }

        #region Loading Functions

        #endregion

        #region Utility Functions

        #endregion

        #region Command Bindings

        /// <summary>
        /// </summary>
        public static void ClearChatLog()
        {
            var popupContent = new PopupContent();
            popupContent.PluginName = Plugin.PName;
            popupContent.Title = PluginViewModel.Instance.Locale["app_WarningMessage"];
            popupContent.Message = PluginViewModel.Instance.Locale["sample_ClearChatLogMessage"];
            popupContent.CanCancel = true;
            Plugin.PHost.PopupMessage(Plugin.PName, popupContent);
            EventHandler<PopupResultEvent> resultChanged = null;
            resultChanged = delegate(object sender, PopupResultEvent e)
            {
                switch (e.NewValue.ToString())
                {
                    case "OK":
                        MainView.View.ChatLogFD._FD.Blocks.Clear();
                        break;
                    case "Cancel":
                        break;
                }
                PluginViewModel.Instance.PopupResultChanged -= resultChanged;
            };
            PluginViewModel.Instance.PopupResultChanged += resultChanged;
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }

        #endregion
    }
}
