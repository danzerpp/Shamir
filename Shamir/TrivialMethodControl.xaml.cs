using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shamir.Models;

namespace Shamir
{
    /// <summary>
    /// longeraction logic for TrivialMethodControl.xaml
    /// </summary>
    public partial class TrivialMethodControl : UserControl
    {
        private List<long> SharesValues = new List<long>();
        private long SecretRange;
        public TrivialMethodControl()
        {
            InitializeComponent();
        }




        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSecretRange.Text))
            {
                txtSecretValue.IsEnabled = false;
                txtShares.IsEnabled = false;
            }
            else
            {
                txtSecretValue.IsEnabled = true;

                if (!string.IsNullOrEmpty(txtSecretValue.Text))
                {
                    txtShares.IsEnabled = true;
                }
                else
                {
                    txtShares.IsEnabled = false;
                }
            }

        }


        private void TextInput_PreviewInteger(object sender, TextCompositionEventArgs e)
        {
            try
            {
                long txtValue = long.Parse(e.Text);

                if (((TextBox)sender).Name == "txtSecretValue" && long.Parse(txtSecretValue.Text + e.Text) >= long.Parse(txtSecretRange.Text) )
                {
                    e.Handled = true;
                    MessageBox.Show(string.Format("Sekret musi być liczbą z zakresu <0, {0}>",
                        long.Parse(txtSecretRange.Text)-1), " Uwaga!!");
                    return;
                }

                if (((TextBox)sender).Name == "txtShares" && long.Parse(txtShares.Text + e.Text) == 0)
                {
                    e.Handled = true;
                    MessageBox.Show(string.Format("Ilość udziałów musi być większa od 0!",
                        long.Parse(txtSecretRange.Text) - 1), " Uwaga!!");
                    return;
                }

                e.Handled = false;
            }
            catch (Exception exception)
            {
                e.Handled = true;
            }
        }

       

        private void GenerateShares_OnClick(object sender, RoutedEventArgs e)
        {
            lbShares.Items.Clear();
            lbReturnSecret.Items.Clear();
            SharesValues.Clear();

            btnReturnSecret.IsEnabled = false;

            long secretRange, secretValue, shares;

            if (!long.TryParse(txtSecretRange.Text,out secretRange))
            {
                MessageBox.Show("Nie podano długości sekretu!", " Uwaga!!");
                return;
            }
            if (!long.TryParse(txtSecretValue.Text, out secretValue))
            {
                MessageBox.Show("Nie podano wartości sekretu!", " Uwaga!!");
                return;
            }
            if (!long.TryParse(txtShares.Text, out shares))
            {
                MessageBox.Show("Nie podano ilości udziałów!", " Uwaga!!");
                return;
            }

            


            string equation = "(" + secretValue;
            long currentVal = secretValue;
            for (long i = 1; i < shares; i++)
            {
                long newShare = MyMath.LongRandom(0, secretRange,new Random());
                currentVal -= newShare;
                equation += " - " + newShare;
                SharesValues.Add(newShare);
                lbShares.Items.Add(string.Format("s{0} = {1}", i, newShare));
            }

            equation += ") mod " + secretRange;
            long lastShare =Math.Abs(MyMath.Modulo(currentVal,secretRange));
            SharesValues.Add(lastShare);
            SecretRange = secretRange;

            lbShares.Items.Add(string.Format("s{0} = {1} ", shares, equation));
            lbShares.Items.Add(string.Format("s{0} = {1}", shares, lastShare));

            btnReturnSecret.IsEnabled = true;
        }

        private void ReturnSecret_OnClick(object sender, RoutedEventArgs e)
        {
            string equation = "(" + SharesValues[0];

            for (int i = 1; i < SharesValues.Count; i++)
            {
                equation += " + " + SharesValues[i];
            }
            equation += ") mod " + SecretRange;

            lbReturnSecret.Items.Clear();
            lbReturnSecret.Items.Add("s = " + equation);
            lbReturnSecret.Items.Add("s = " + SharesValues.Sum() % SecretRange);

        }

    }
}
