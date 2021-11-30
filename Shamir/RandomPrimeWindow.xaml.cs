using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Shamir.Models;

namespace Shamir
{
    /// <summary>
    /// longeraction logic for RandomPrimeWindow.xaml
    /// </summary>
    public partial class RandomPrimeWindow : Window
    {
        public string prime="";
        public RandomPrimeWindow()
        {
            InitializeComponent();
        }

        private void TxtSecretRange_PreviewInteger(object sender, TextCompositionEventArgs e)
        {
            try
            {
                long txtValue = long.Parse(e.Text);
            }
            catch (Exception exception)
            {
                e.Handled = true;
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {

            if (string.IsNullOrEmpty(txtPrimeFrom.Text))
                txtPrimeTo.IsEnabled = false;
            else
                txtPrimeTo.IsEnabled = true;
        }

        private void TxtPrimeFrom_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPrimeFrom.Text))
                if (long.Parse(txtPrimeFrom.Text) < 3)
                {
                    MessageBox.Show("wartośc minimalna zakresu wynosi 3");
                    txtPrimeFrom.Text = "3";
                }
           
        }

        private void BtnGeneratePrime_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPrimeFrom.Text) || string.IsNullOrEmpty(txtPrimeTo.Text))
                MessageBox.Show("Podaj zakres liczby pierwszej !");
            else
            {
                long primeFrom = long.Parse(txtPrimeFrom.Text);
                long primeTo= long.Parse(txtPrimeTo.Text);
                if (primeFrom > primeTo)
                {
                    txtPrimeTo.Text = (primeTo + 1).ToString();
                    MessageBox.Show("Niepoprawny zakres - wartość do musi być co najmniej o 1 większa!");
                    return;
                }

                long count = 0;
                Random random = new Random();
                long probablyPrime = MyMath.LongRandom(primeFrom, primeTo,random);
                while (!MyMath.IsPrime(probablyPrime))
                {
                    if (count >100000)
                    {
                        MessageBox.Show(
                            "Nie można odnaleźć lcizby pierwszej w podanym zakresie! Spróbuj ponownie lub podaj nowy zakres.");
                        return;
                    }
                    probablyPrime = MyMath.LongRandom(primeFrom, primeTo, random);
                    count++;
                }

                prime = probablyPrime.ToString();
                this.DialogResult = true;
                this.Close();
            }
        }
       
    }
}
