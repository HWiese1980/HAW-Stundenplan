#region Usings

using System;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace SeveQsCustomControls
{
    ///<summary>
    ///  Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    ///  Step 1a) Using this custom control in a XAML file that exists in the current project.
    ///  Add this XmlNamespace attribute to the root element of the markup file where it is 
    ///  to be used:
    ///
    ///  xmlns:MyNamespace="clr-namespace:SeveQsCustomControls"
    ///
    ///
    ///  Step 1b) Using this custom control in a XAML file that exists in a different project.
    ///  Add this XmlNamespace attribute to the root element of the markup file where it is 
    ///  to be used:
    ///
    ///  xmlns:MyNamespace="clr-namespace:SeveQsCustomControls;assembly=SeveQsCustomControls"
    ///
    ///  You will also need to add a project reference from the project where the XAML file lives
    ///  to this project and Rebuild to avoid compilation errors:
    ///
    ///  Right click on the target project in the Solution Explorer and
    ///  "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    ///  Step 2)
    ///  Go ahead and use your control in the XAML file.
    ///
    ///  <MyNamespace:Accordion />
    ///</summary>
    public class Accordion : StackPanel
    {
        static Accordion()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (Accordion),
                                                     new FrameworkPropertyMetadata(typeof (Accordion)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitializeAccordion();
        }

        private void InitializeAccordion()
        {
            foreach (DependencyObject element in Children)
            {
                var selectedExpander = element as Expander;
                if (selectedExpander != null)
                {
                    selectedExpander.Expanded += selectedExpander_Expanded;
                }
            }
        }

        private void selectedExpander_Expanded(object sender, RoutedEventArgs e)
        {
            var selectedExpander = sender as Expander;
            double totalExpanderHeight = 0;

            if (selectedExpander != null)
            {
                ContentPresenter contentPresenter;
                foreach (DependencyObject element in Children)
                {
                    var otherExpander = element as Expander;
                    if (otherExpander != null & otherExpander != selectedExpander)
                    {
                        if (otherExpander.IsExpanded)
                        {
                            contentPresenter =
                                otherExpander.Template.FindName("PART_ExpandSite", otherExpander) as ContentPresenter;
                            if (contentPresenter != null)
                                totalExpanderHeight -= contentPresenter.ActualHeight;
                        }
                        otherExpander.IsExpanded = false;
                        totalExpanderHeight += otherExpander.ActualHeight;
                    }
                }

                if (selectedExpander.IsExpanded)
                {
                    contentPresenter =
                        selectedExpander.Template.FindName("PART_ExpandSite", selectedExpander) as ContentPresenter;
                    if (contentPresenter != null)
                        contentPresenter.Height = ActualHeight - totalExpanderHeight - selectedExpander.ActualHeight;
                }
            }
        }
    }
}