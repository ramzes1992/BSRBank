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
using System.Windows.Shapes;

namespace BankClient
{
    /// <summary>
    /// Interaction logic for TransferWindow.xaml
    /// </summary>
    public partial class TransferWindow : Window
    {
        private string sourceAccountNumber = "";

        public TransferWindow(string sourceAccountNumber)
        {
            InitializeComponent();
            this.sourceAccountNumber = sourceAccountNumber;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            using (var client = new BankService.BankServiceClient())
            {
                var operation = new OperationEntry()
                {
                    Amount = v_IntegerUpDown_Amount.Value ?? 0,
                    Source = sourceAccountNumber,
                    Date = DateTime.Now,
                    Destination = v_TextBox_Destination.Text,
                    Title = v_TextBox_AdditionalText.Text,
                };

                try
                {
                    var result = client.TransferRequest(operation, TokenHelper.CurrentTokken);
                    DialogResult = true;
                    return;
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
}
