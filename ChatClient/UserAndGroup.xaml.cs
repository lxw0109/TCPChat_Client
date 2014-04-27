using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatClient
{
    /// <summary>
    /// UserAndGroup.xaml 的交互逻辑
    /// </summary>
    public partial class UserAndGroup : Window
    {
        // Just for trial.
        private int number = 0;

        public UserAndGroup()
        {
            InitializeComponent();
        }

        // Simulate adding elements into the listview.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ++number;
            this.listview.Items.Add(number);
        }

        // When choose someone, then open the window for communicating.
        private void listview_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            Comunicate com = new Comunicate();
            com.Show();
        }
    }
}
