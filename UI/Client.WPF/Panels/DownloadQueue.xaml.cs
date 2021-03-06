﻿#region Copyright Kayomani 2010.  Licensed under the GPLv3 (Or later version), Expand for details. Do not remove this notice.
/**
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or any 
    later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FAP.Application.Views;
using FAP.Application.ViewModels;

namespace Fap.Presentation.Panels
{
    /// <summary>
    /// Interaction logic for DownloadQueue.xaml
    /// </summary>
    public partial class DownloadQueue : UserControl, IDownloadQueue
    {
        public DownloadQueue()
        {
            InitializeComponent(); 
        }

        private void listView2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DownloadQueueViewModel vm = DataContext as DownloadQueueViewModel;
            if (null != vm)
            {
                vm.SelectedItems =  listView2.SelectedItems;
            }
        }

        private DownloadQueueViewModel Model { get { return DataContext as DownloadQueueViewModel; } }

        private void listView2_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (listView2.SelectedItems != null && listView2.SelectedItems.Count > 0)
            {
                System.Windows.Controls.ListBox src = e.Source as System.Windows.Controls.ListBox;

                src.ContextMenu.Items.Clear();

                MenuItem m = new MenuItem();
                m.Foreground = Brushes.Black;
                m.Header = "Move to top";
                m.Command = Model.Movetotop;
                m.CommandParameter = listView2.SelectedItems;
                src.ContextMenu.Items.Add(m);

                m = new MenuItem();
                m.Foreground = Brushes.Black;
                m.Header = "Move up";
                m.Command = Model.Moveup;
                m.CommandParameter = listView2.SelectedItems;
                src.ContextMenu.Items.Add(m);

                m = new MenuItem();
                m.Foreground = Brushes.Black;
                m.Header = "Move down";
                m.Command = Model.Movedown;
                m.CommandParameter = listView2.SelectedItems;
                src.ContextMenu.Items.Add(m);

                m = new MenuItem();
                m.Foreground = Brushes.Black;
                m.Header = "Move to bottom";
                m.Command = Model.Movetobottom;
                m.CommandParameter = listView2.SelectedItems;
                src.ContextMenu.Items.Add(m);
            }
            else
            {
                e.Handled = true;
            }
        }
    }
}
