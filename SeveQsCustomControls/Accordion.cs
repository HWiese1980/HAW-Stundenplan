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
    ///     <MyNamespace:Accordion/>
    ///
    /// </summary>
    public class Accordion : StackPanel
    {
        static Accordion()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Accordion), new FrameworkPropertyMetadata(typeof(Accordion)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeAccordion();
        }

        private void InitializeAccordion()
        {
            Expander selectedExpander;
            foreach (UIElement element in this.Children)
            {
                selectedExpander = element as Expander;
                if (selectedExpander != null)
                {
                    selectedExpander.Expanded += new RoutedEventHandler(selectedExpander_Expanded);
                }
            }
        }

        void selectedExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander selectedExpander = sender as Expander;
            Expander otherExpander = null;
            ContentPresenter contentPresenter = null;
            double totalExpanderHeight = 0;

            if (selectedExpander != null)
            {
                foreach (UIElement element in this.Children)
                {
                    otherExpander = element as Expander;
                    if (otherExpander != null & otherExpander != selectedExpander)
                    {
                        if (otherExpander.IsExpanded)
                        {
                            contentPresenter = otherExpander.Template.FindName("PART_ExpandSite", otherExpander) as ContentPresenter;
                            if (contentPresenter != null)
                                totalExpanderHeight -= contentPresenter.ActualHeight;
                        }
                        otherExpander.IsExpanded = false;
                        totalExpanderHeight += otherExpander.ActualHeight;
                    }
                }

                if (selectedExpander.IsExpanded)
                {
                    contentPresenter = selectedExpander.Template.FindName("PART_ExpandSite", selectedExpander) as ContentPresenter;
                    if (contentPresenter != null)
                        contentPresenter.Height = this.ActualHeight - totalExpanderHeight - selectedExpander.ActualHeight; 
                }
            }
        }
    }
}
