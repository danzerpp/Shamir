using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shamir.Models;

namespace Shamir
{
    /// <summary>
    /// Interaction logic for ShamirMethodControl.xaml
    /// </summary>
    public partial class ShamirMethodControl : UserControl
    {
        private long _prime;           //p
        private long _secretValue;     //s
        private long _shares;          //n
        private long _sharesToRecover; //t

        private List<long> _aList = new List<long>(); // list of a
        private List<Tuple<int, long>>  _siList = new List<Tuple<int, long>>(); // list of si - (x,y) points

        public ShamirMethodControl()
        {
            InitializeComponent();
        }


        

        private void BtnGetPrime_OnClick(object sender, RoutedEventArgs e)
        {
            RandomPrimeWindow rdw = new RandomPrimeWindow();
            if (rdw.ShowDialog() == true)
            {
                txtPrime.Text = rdw.prime;
            }
        }
        
        private void BtnGetListOfA_OnClick(object sender, RoutedEventArgs e)
        {
            _aList.Clear();
            _siList.Clear();

            lbListOfA.Items.Clear();
            lbListOfS.Items.Clear();
            lbRecoverSecret.Items.Clear();
            btnRecoverSecret.IsEnabled = false;

            long prime, secretValue, shares, sharesToRecover;

            if (!long.TryParse(txtPrime.Text, out prime))
            {
                MessageBox.Show("Nie podano liczby pierwszej!", " Uwaga!!");
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
            if (!long.TryParse(txtSharesToRecover.Text, out sharesToRecover))
            {
                MessageBox.Show("Nie podano ilości udziałów potrzebnych do odtworzenia!", " Uwaga!!");
                return;
            }

            _prime = prime;
            _secretValue = secretValue;
            _shares = shares;
            _sharesToRecover = sharesToRecover;
            Random rand = new Random();

            string listOfA = "";
            for (int i = 1; i < _sharesToRecover; i++) // obliczamy a dla t-1 możliwości
            {
                long val = MyMath.LongRandom(3, _secretValue,rand);
                _aList.Add(val);
                listOfA += $"a{i} = {val}, ";
            }

            listOfA = listOfA.Substring(0, listOfA.Length - 2);

            lbListOfA.Items.Add(listOfA);
            btnGetListOfS.IsEnabled = true;
          
        }

        private void BtnGetListOfS_OnClick(object sender, RoutedEventArgs e)
        {
            _siList.Clear();
            lbListOfS.Items.Clear();
            lbRecoverSecret.Items.Clear();

            for (int i = 1; i <= _shares; i++)
            {
                string equation = $"s{i} =( {_secretValue}";
                long result = 0;
                for (int j = 1; j < _sharesToRecover; j++)
                {
                    result += _aList[j - 1] * (long)Math.Pow(i, j);

                    equation += $" + {_aList[j - 1]}*{i}^{j}";
                }

                equation += ") % " + _prime;

                result = ((long)_secretValue + result) % _prime;

                _siList.Add(new Tuple<int, long>(i, result));
                lbListOfS.Items.Add(equation + " = " +result);
            }

            btnRecoverSecret.IsEnabled = true;
        }

        private void BtnRecoverSecret_OnClick(object sender, RoutedEventArgs e)
        {
            lbRecoverSecret.Items.Clear();
            var lbSelectedSis = lbListOfS.SelectedItems;
            
            if (lbSelectedSis.Count < _sharesToRecover)
            {
                MessageBox.Show($"Należy wybrać {_sharesToRecover} wartości z listy si");
                return;
            }

            List<Tuple<int, long>> selectedSiList =new List<Tuple<int, long>>();


           foreach (string si in lbSelectedSis) 
           {
               string index = "";
               for (int i = 1; i < si.Length; i++)
               {
                   if (char.IsDigit(si[i]))
                       index += si[i];
                   else
                   {
                       selectedSiList.Add(_siList[int.Parse(index)-1]);
                       break;
                   }
               }
           }


            List<object> totalEquation = new List<object>();
            List<List<object>> li = new List<List<object>>(); // list of langrage interpolation equations
            List<int> dividers = new List<int>();

            foreach (var currentSi in selectedSiList)
            {
                int divider = 0;
                int intercept = 0;

                var otherSiList = selectedSiList.Where(si => si != currentSi).ToList();
             
                foreach (var otherSi in otherSiList)
                {
                    if (totalEquation.Count == 0)
                    {
                        divider = currentSi.Item1 - otherSi.Item1;
                        totalEquation.Add(new ModelX(1, 1));
                        totalEquation.Add(otherSi.Item1);
                        intercept = -otherSi.Item1;
                        continue;
                    }

                    divider = divider * (currentSi.Item1 - otherSi.Item1);

                    ModelX secondX = new ModelX(1, 1);
                    int otherSiValueX = -otherSi.Item1;

                    List<ModelX> results = new List<ModelX>();
                    foreach (var mainValue in totalEquation)
                    {
                        if (typeof(ModelX) == mainValue.GetType())
                        {
                            results.Add(MyMath.Multiply((ModelX)mainValue, secondX));

                            results.Add(MyMath.Multiply((ModelX)mainValue, otherSiValueX));
                        }
                        else
                        {
                            results.Add(MyMath.Multiply((ModelX)secondX, -(int)mainValue));
                            intercept *= otherSiValueX;
                        }
                    }

                    List<object> summary = new List<object>();
                    var xPowerGrouped = results.GroupBy(g => g.Power).Select(s => s.ToList()).ToList();

                    foreach (var group in xPowerGrouped.OrderByDescending(o => o.First().Power))
                    {
                        summary.Add(new ModelX(group.Sum(s => s.A), group.First().Power));
                    }

                    summary.Add(intercept);

                    totalEquation.Clear();
                    totalEquation.AddRange(summary);
                }
                dividers.Add(divider);
                var copyOfEquation = new object[totalEquation.Count];
                totalEquation.CopyTo(copyOfEquation);
                li.Add(copyOfEquation.ToList());
                totalEquation.Clear();
            }




            List<string> yili = new List<string>();
            int equationsCount = 0;
            foreach (var values in li)
            {
                string equation = "";
                //string equation = $"l{equationsCount}(x) = ";

                foreach (var value in values)
                {
                    if (typeof(ModelX) == value.GetType())
                    {
                        ModelX modelX = value as ModelX;
                        if (modelX.A>0)
                            equation += $" + {modelX.A}x^{modelX.Power}";
                        else
                            equation += $" - {Math.Abs(modelX.A)}x^{modelX.Power}";

                    }
                    else
                    {
                        if ((int)value>0)
                             equation += " + "+ (int)value;
                        else
                             equation += " - "+ Math.Abs((int)value) ;
                    }
                }

                yili.Add($"y{equationsCount}l{equationsCount}(x) = {selectedSiList[equationsCount].Item2} * (({equation.Substring(3)}) / {dividers[equationsCount]}) mod {_prime}");
                equation = $"l{equationsCount}(x) = ({equation.Substring(3)}) / {dividers[equationsCount]}";
                lbRecoverSecret.Items.Add(equation);
                equationsCount++;
            }
            lbRecoverSecret.Items.Add("");

            yili.ForEach(s=>lbRecoverSecret.Items.Add(s));
            
            lbRecoverSecret.Items.Add("");
            lbRecoverSecret.Items.Add("Dla wszystkich powyższych równań wykonujemy funkcję DivMod(yi *  wyraz_wolny_li % p, dzielnik_li, p)");

            long sum = 0;
            for (int i = 0; i < li.Count; i++)
            {
                long divModResult = MyMath.DivMod(MyMath.Modulo(selectedSiList[i].Item2 * (int)li[i].LastOrDefault(), _prime), dividers[i], _prime);
                sum += divModResult;
                lbRecoverSecret.Items.Add($"DivMod({selectedSiList[i].Item2} * {(int)li[i].LastOrDefault()} % {_prime}, {dividers[i]}, {_prime}) = {divModResult}");
            }

            var recoveredSecret = MyMath.Modulo(sum, _prime);
            lbRecoverSecret.Items.Add("");
            lbRecoverSecret.Items.Add("Powstałe wyniki sumujemy i wykonujemy operację suma % p, która zwraca nam wartość sekretu");
            lbRecoverSecret.Items.Add($"{sum} mod {_prime} = {recoveredSecret}" );

            lbRecoverSecret.Items.Add("Odtworzony sekret -> " + recoveredSecret);
        }

        


        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPrime.Text))
            {
                txtSecretValue.IsEnabled = false;
                txtShares.IsEnabled = false;
                txtSharesToRecover.IsEnabled = false;
                return;
            }
            else
                txtSecretValue.IsEnabled = true;

            if (string.IsNullOrEmpty(txtSecretValue.Text))
            {
                txtShares.IsEnabled = false;
                txtSharesToRecover.IsEnabled = false;
                return;
            }
            else
                txtShares.IsEnabled = true;

            if (string.IsNullOrEmpty(txtShares.Text))
            {
                txtSharesToRecover.IsEnabled = false;
                return;
            }
            else
                txtSharesToRecover.IsEnabled = true;

        }

        private void TxtSecretRange_PreviewInteger(object sender, TextCompositionEventArgs e)
        {
            try
            {
                long txtValue = long.Parse(e.Text);

                if (((TextBox)sender).Name == "txtSecretValue" && long.Parse(txtSecretValue.Text + e.Text) >= long.Parse(txtPrime.Text))
                {
                    e.Handled = true;
                    MessageBox.Show(string.Format("Sekret musi być liczbą z zakresu <0, {0}>",
                        long.Parse(txtPrime.Text) - 1), " Uwaga!!");
                    return;
                }

                if (((TextBox)sender).Name == "txtShares" && (long.Parse(txtShares.Text + e.Text) == 0 || long.Parse(txtShares.Text + e.Text) >= long.Parse(txtPrime.Text)))
                {
                    e.Handled = true;
                    MessageBox.Show(string.Format("Ilość udziałów musi być liczbą z zakresu <1, {0}>",
                        long.Parse(txtPrime.Text) - 1), " Uwaga!!");
                    return;
                }

                if (((TextBox)sender).Name == "txtSharesToRecover" && (long.Parse(txtSharesToRecover.Text + e.Text) == 0 || long.Parse(txtSharesToRecover.Text + e.Text) > long.Parse(txtShares.Text)))
                {
                    e.Handled = true;
                    MessageBox.Show(string.Format("Ilość udziałów do odtworzenia musi być liczbą z zakresu <1, {0}>",
                        long.Parse(txtShares.Text)), " Uwaga!!");
                }
            }
            catch (Exception exception)
            {
                e.Handled = true;
            }
        }


        private void TxtPrime_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPrime.Text))
                if (!MyMath.IsPrime(long.Parse(txtPrime.Text)))
                {
                    MessageBox.Show("Podana liczba nie jest liczbą pierwszą!");
                    txtPrime.Text = "";
                }
        }







    }
}
