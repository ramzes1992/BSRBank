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

namespace BankClient
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void v_Button_Login_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(v_TextBox_Username.Text)
                || string.IsNullOrWhiteSpace(v_PasswordBox_Password.Password))
            {
                MessageBox.Show("Empty Username or Password");
            }
            else
            {
                using (BankService.BankServiceClient client = new BankService.BankServiceClient())
                {
                    try
                    {
                        var token = client.LogIn(v_TextBox_Username.Text, v_PasswordBox_Password.Password);
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            TokenHelper.CurrentTokken = token;
                            DialogResult = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }

        }

        private void v_ListView_Accounts_ListViewItem_Click(object sender, MouseButtonEventArgs e)
        {
            var x = sender;
        }
    }
}
