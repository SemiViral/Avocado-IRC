using System;
using Avocado.ViewModel;

namespace Avocado.Model {
    public delegate void ChannelChangedEventHandler(object sender, ChannelChangedEventArgs e);

    public class ChannelChangedEventArgs : EventArgs {
        public ChannelChangedEventArgs(bool isAddition, ChannelViewModel changedChannel) {
            IsAddition = isAddition;
            ChangedChannel = changedChannel;
        }

        public ChannelViewModel ChangedChannel { get; }
        public bool IsAddition { get; }
    }
}
