namespace HAW_Tool.Aspects
{
    public interface INotifyValueChanged
    {
        #region Operations (2)

        void OnValueChanged(string property);

        void OnValueChanging(string property, object oldValue, object newValue);

        object GetNewValue(string property);

        #endregion Operations
    }
}