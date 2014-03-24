// © Mike Murphy

using EMU7800.Core;

namespace EMU7800.Services.Dto
{
    public class GameProgramInfo
    {
        public string Title { get; set; }
        public string Manufacturer { get; set; }
        public string Author { get; set; }
        public string Qualifier { get; set; }
        public string Year { get; set; }
        public string ModelNo { get; set; }
        public string Rarity { get; set; }
        public CartType CartType { get; set; }
        public MachineType MachineType { get; set; }
        public Controller LController { get; set; }
        public Controller RController { get; set; }
        public string MD5 { get; set; }
        public string HelpUri { get; set; }
    }
}
