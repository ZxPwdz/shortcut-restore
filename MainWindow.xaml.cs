using System.Windows;
using System.Windows.Input;

namespace ShortcutRestore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double-click - do nothing for now (no maximize)
                return;
            }

            DragMove();
        }
    }
}
