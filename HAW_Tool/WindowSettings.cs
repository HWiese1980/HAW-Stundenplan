using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://schemas.kingsmill.com/kingsmill/windows", "Kingsmill.Windows")]

namespace Kingsmill.Windows
{
    /// <summary>
    /// Persists a Window's Size, Location and WindowState to UserScopeSettings 
    /// </summary>
    public sealed class WindowSettings
    {
        #region WindowApplicationSettings Helper Class

        public class WindowApplicationSettings : ApplicationSettingsBase
        {
            public WindowApplicationSettings(WindowSettings windowSettings)
                : base(windowSettings._window.Uid.ToString())
            {
            }

            [UserScopedSetting]
            public Rect Location
            {
                get
                {
                    if (this["Location"] != null)
                    {
                        return ((Rect)this["Location"]);
                    }
                    return Rect.Empty;
                }
                set { this["Location"] = value; }
            }

            [UserScopedSetting]
            public WindowState WindowState
            {
                get
                {
                    if (this["WindowState"] != null)
                    {
                        return (WindowState)this["WindowState"];
                    }
                    return WindowState.Normal;
                }
                set { this["WindowState"] = value; }
            }
        }

        #endregion

        #region Constructor

        private Window _window;

        public WindowSettings(Window window)
        {
            _window = window;
        }

        #endregion

        #region Attached "Save" Property Implementation

        /// <summary>
        /// Register the "Save" attached property and the "OnSaveInvalidated" callback 
        /// </summary>
        public static readonly DependencyProperty SaveProperty
            = DependencyProperty.RegisterAttached("Save", typeof(bool), typeof(WindowSettings),
                                                  new FrameworkPropertyMetadata(OnSaveInvalidated));

        public static void SetSave(DependencyObject dependencyObject, bool enabled)
        {
            dependencyObject.SetValue(SaveProperty, enabled);
        }

        /// <summary>
        /// Called when Save is changed on an object.
        /// </summary>
        private static void OnSaveInvalidated(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var window = dependencyObject as Window;
            if (window != null)
            {
                if ((bool)e.NewValue)
                {
                    var settings = new WindowSettings(window);
                    settings.Attach();
                }
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Load the Window Size Location and State from the settings object
        /// </summary>
        private void LoadWindowState()
        {
            Settings.Reload();
            _window.WindowState = Settings.WindowState;
            if (Settings.Location != Rect.Empty)
            {
                _window.Left = Settings.Location.Left;
                _window.Top = Settings.Location.Top;
                _window.Width = Settings.Location.Width;
                _window.Height = Settings.Location.Height;
            }
        }

        /// <summary>
        /// Save the Window Size, Location and State to the settings object
        /// </summary>
        private void SaveWindowState()
        {
            Settings.WindowState = _window.WindowState;
            Settings.Location = _window.RestoreBounds;
            Settings.Save();
        }

        #endregion

        #region Private Methods

        private void Attach()
        {
            if (_window != null)
            {
                _window.Closing += window_Closing;
                _window.Initialized += window_Initialized;
            }
        }

        private void window_Initialized(object sender, EventArgs e)
        {
            LoadWindowState();
        }

        private void window_Closing(object sender, CancelEventArgs e)
        {
            SaveWindowState();
        }

        #endregion

        #region Settings Property Implementation

        private WindowApplicationSettings _windowApplicationSettings;

        [Browsable(false)]
        public WindowApplicationSettings Settings
        {
            get
            {
                return _windowApplicationSettings ??
                       (_windowApplicationSettings = CreateWindowApplicationSettingsInstance());
            }
        }

        private WindowApplicationSettings CreateWindowApplicationSettingsInstance()
        {
            return new WindowApplicationSettings(this);
        }

        #endregion
    }
}