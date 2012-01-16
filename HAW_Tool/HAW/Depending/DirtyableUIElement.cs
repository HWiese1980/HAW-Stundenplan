using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace HAW_Tool.HAW.Depending
{
    public abstract class DirtyableUIElement : Control
    {
        private bool _initializing = false;
        public void BeginCleanInit()
        {
            _initializing = true;
        }

        public void EndCleanInit()
        {
            _initializing = false;
        }

        public abstract string HashInfo { get; set; }
        private byte[] ID
        {
            get { return Encoding.ASCII.GetBytes(HashInfo); }
        }

        protected void ReGenerateHash()
        {
            var tMD5 = MD5.Create();
            Hash = Convert.ToBase64String(tMD5.ComputeHash(ID), Base64FormattingOptions.InsertLineBreaks);
        }

        [JsonProperty]
        public string Hash
        {
            get { return (string)GetValue(HashProperty); }
            set { SetValue(HashProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Hash.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HashProperty =
            DependencyProperty.Register("Hash", typeof(string), typeof(DirtyableUIElement), new UIPropertyMetadata(""));


        public bool IsDirty
        {
            get { return (bool)GetValue(IsDirtyProperty); }
            private set { SetValue(IsDirtyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDirty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDirtyProperty =
            DependencyProperty.Register("IsDirty", typeof(bool), typeof(DirtyableUIElement), new UIPropertyMetadata(false));

        public void CleanUp()
        {
            _oldValues.Clear();
            HashInfo = String.Empty;
            IsDirty = false;
        }

        public void Reset()
        {
            foreach (var oldValuePropertyName in _oldValues.Keys)
            {
                var propField = GetType().GetField(oldValuePropertyName + "Property", BindingFlags.FlattenHierarchy | BindingFlags.Static | BindingFlags.Public);
                var prop = (DependencyProperty)propField.GetValue(null);

                SetValue(prop, _oldValues[oldValuePropertyName]);
            }
            CleanUp();
        }

        private readonly Dictionary<string, object> _oldValues = new Dictionary<string, object>();

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!_initializing && e.OldValue != e.NewValue)
                if (e.Property.Name != "IsDirty" &&
                e.Property.Name != "IsSelected" &&
                e.Property.Name != "Visibility" &&
                e.Property.Name != "Row")
                {
                    if (!_oldValues.ContainsKey(e.Property.Name))
                    {
                        _oldValues.Add(e.Property.Name, e.OldValue);
                    }
                    IsDirty = true;
                }


            base.OnPropertyChanged(e);
        }
    }
}
