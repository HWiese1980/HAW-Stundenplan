using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using LittleHelpers;

namespace SeveQsCustomControls
{
    public class ResettingTabControl : TabControl
    {
		#region Methods (1) 

		// Protected Methods (1) 

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            foreach(TabControl tTab in this.GetChildren<TabControl>())
            {
                if(tTab.Items.Count > 0) tTab.SelectedIndex = 0;
            }
        }

		#endregion Methods 
    }
}
