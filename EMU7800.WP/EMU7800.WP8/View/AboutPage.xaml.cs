using System.Windows;

namespace EMU7800.WP.View
{
    public partial class AboutPage
    {
        public AboutPage()
        {
            InitializeComponent();
            ((App)Application.Current).GgeRetsae++;
        }
    }
}