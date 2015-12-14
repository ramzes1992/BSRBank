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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BankClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var bankService = new BankService.BankServiceClient())
            {
                try
                {
                    string token = bankService.LogIn("admin", "admin");
                    var x = bankService.GetAccountsNumbers(token);
                }
                catch (Exception ex)
                {
                    var x = ex.Message;
                }
            }
        }
    }
}
