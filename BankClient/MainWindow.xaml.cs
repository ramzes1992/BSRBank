using BankClient.BankService;
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
            this.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var lw = new LoginWindow();

            if (lw.ShowDialog() == true)
            {
                this.IsEnabled = true;
            }
            else
            {
                Close();
            }
        }

        private void v_Button_GetAccounts_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new BankService.BankServiceClient())
            {
                //TODO: get accounts
                try
                {
                    var accounts = client.GetAccountsNumbers(TokenHelper.CurrentTokken);
                    v_ListView_Accounts.ItemsSource = accounts;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    if (ex.Message.Contains("Token"))
                    {
                        this.IsEnabled = false;
                        var lw = new LoginWindow();

                        if (lw.ShowDialog() == true)
                        {
                            this.IsEnabled = true;
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
            }
        }

        private void v_ListView_Accounts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject obj = (DependencyObject)e.OriginalSource;

            AccountEntry accountEntry = null;

            while (obj != null && obj != v_ListView_Accounts)
            {
                if (obj.GetType() == typeof(ListViewItem))
                {
                    accountEntry = (obj as ListViewItem).Content as AccountEntry;
                    break;
                }
                obj = VisualTreeHelper.GetParent(obj);
            }

            if (accountEntry != null)
            {
                using (var client = new BankService.BankServiceClient())
                {
                    try
                    {
                        var history = client.GetAccountHistory(accountEntry.AccountNumber, TokenHelper.CurrentTokken);
                        v_ListView_History.ItemsSource = history;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        if (ex.Message.Contains("Token"))
                        {
                            this.IsEnabled = false;
                            var lw = new LoginWindow();

                            if (lw.ShowDialog() == true)
                            {
                                this.IsEnabled = true;
                            }
                            else
                            {
                                Close();
                            }
                        }
                    }
                }
            }
        }

        private void v_Button_CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            using (var client = new BankService.BankServiceClient())
            {
                try
                {
                    var accountNumber = client.CreateNewAccount(TokenHelper.CurrentTokken);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    if (ex.Message.Contains("Token"))
                    {
                        this.IsEnabled = false;
                        var lw = new LoginWindow();

                        if (lw.ShowDialog() == true)
                        {
                            this.IsEnabled = true;
                        }
                        else
                        {
                            Close();
                        }
                    }
                }
            }
        }

        private void v_Button_Transfer_Click(object sender, RoutedEventArgs e)
        {
            var tw = new TransferWindow((v_ListView_Accounts.SelectedItem as AccountEntry).AccountNumber);
            if(tw.ShowDialog() == true)
            {
                MessageBox.Show("Done!");
            }
        }
    }
}
