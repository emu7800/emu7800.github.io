namespace EMU7800.Services.Dto
{
    public class StringType
    {
        public string Line { get; set; } = string.Empty;

        public static StringType ToStringType(string s) => new StringType { Line = s };
    }
}
