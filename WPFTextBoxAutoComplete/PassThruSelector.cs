namespace WPFTextBoxAutoComplete
{
    public class PassThruSelector : IStringSelector
    {
        public string Select(object input)
        {
            return (string)input;
        }
    }
}