using LittleHelpers;

namespace HAW_Tool.HAW.Native
{
    public class EventCode
    {
        private string _code;

        public string Code
        {
            get { return _code.ConvertUmlauts(UmlautConvertDirection.FromCrossWordFormat); }
            set { _code = value.ConvertUmlauts(UmlautConvertDirection.ToCrossWordFormat); }
        }

        public GroupID Group { get; set; }
    }
}