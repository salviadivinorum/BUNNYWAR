using System.Windows;

namespace Bunny_GUI_1
{
    /// <summary>
    /// Interakční logika pro Oaplikaci.xaml
    /// </summary>
    public partial class Oaplikaci : Window
    {
        public Oaplikaci()
        {
            InitializeComponent();
        }

        // pouze jedina udalost
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
