﻿using System;
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
using LittleHelpers;
using System.ComponentModel;

namespace SeveQsCustomControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SeveQsCustomControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:SeveQsCustomControls;assembly=SeveQsCustomControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:RasteredGroup/>
    ///
    /// </summary>
    public class RasteredGroup : ContentControl, INotifyPropertyChanged
    {
        static RasteredGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RasteredGroup), new FrameworkPropertyMetadata(typeof(RasteredGroup)));
        }

        //TODO: SelectedItem funktioniert noch nicht. GetChildren findet keine Kinder in RasteredGroup...

        internal void OnSelectedItemChanged()
        {
            OnPropertyChanged("SelectedItem");
        }

        public object SelectedItem
        {
            get
            {
                return (from tRIC in this.GetChildren<RasteredItemsControl>() where tRIC.SelectedItem != null select tRIC.SelectedItem).FirstOrDefault();
            }
        }

        #region INotifyPropertyChanged Members

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
