namespace EMU7800.Launcher
{
    public class DropDownItem<T> where T : new()
    {
        public string DisplayName { get; set; } = string.Empty;
        public T Value { get; set; } = new T();
    }
}
