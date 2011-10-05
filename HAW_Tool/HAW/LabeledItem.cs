namespace HAW_Tool.HAW
{
    public abstract class LabeledItem
    {
        public virtual string Label
        {
            get
            {
                return ToString();
            }
        }

        public virtual string ToolTip
        {
            get
            {
                return this.Label;
            }
        }
    }
}