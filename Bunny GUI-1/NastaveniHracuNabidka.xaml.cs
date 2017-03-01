using System;
using System.Windows;
using System.Windows.Input;


namespace Bunny_GUI_1
{
    /// <summary>
    /// Interakční logika pro NastaveniHracuNabidka.xaml
    /// </summary>
    [Serializable]
    public partial class NastaveniHracuNabidka : Window
    {
        private DispecerHry dispecer2;
        public string jmenoHrace1;
        public string jmenoHrace2;
        public bool nabidkaNovaHra;

        // Parametricky konstruktor okna. To stejne okno pouzivam totiz pro 2 ucely - Nova hra a Nastaveni hry        
        public NastaveniHracuNabidka(bool novaHraTrueFalse)
        {
            InitializeComponent();
           
            nabidkaNovaHra = novaHraTrueFalse;
            this.dispecer2 = MainWindow.GetDispecerHry();
            DataContext = jmenoHrac1TextBox;
            DataContext = hlavniMriz;
            DataContext = oknoNovaHra;

            if (nabidkaNovaHra == false)
            {
                oknoNovaHra.Title = "Změna nastavení hráčů";
                jmenoHrac1TextBox.Text = dispecer2.jmH1CvDispecerovi;
                jmenoHrac2TextBox.Text = dispecer2.jmH2vDispecerovi;
               
                if (MainWindow.GetZpomalenVypocetProPCHrace1)
                    checkBoxZpomalitPCH1.IsChecked = true;

                if (MainWindow.GetZpomalenVypocetProPCHrace2)
                    checkBoxZpomalitPCH2.IsChecked = true;

                if (dispecer2.GetHrac1.GetJePocitacovyHrac)
                {
                    pocitac1RadioButton.IsChecked = true;
                    string obtiznostH1 = dispecer2.obtiznostHrace1vDispecerovi.Substring(0,1);                    
                    switch(obtiznostH1)
                    {
                        case "1":
                            novacek1.IsSelected = true;
                            break;
                        case "2":
                            pokrocily1.IsSelected = true;
                            break;
                        case "3":
                            odbornik1.IsSelected = true;
                            break;
                        case "4":
                            mistr1.IsSelected = true;
                            break;
                        default:
                            neurceno1.IsSelected = true;
                            break;
                    }
                }
                else
                {
                    clovek1RadioButton.IsChecked = true;
                }

                if(dispecer2.GetHrac2.GetJePocitacovyHrac)
                {
                    pocitac2RadioButton.IsChecked = true;
                    string obtiznostH2 = dispecer2.obtiznostHrace2vDispecerovi.Substring(0, 1);
                    switch (obtiznostH2)
                    {
                        case "1":
                            novacek2.IsSelected = true;
                            break;
                        case "2":
                            pokrocily2.IsSelected = true;
                            break;
                        case "3":
                            odbornik2.IsSelected = true;
                            break;
                        case "4":
                            mistr2.IsSelected = true;
                            break;
                        default:
                            neurceno2.IsSelected = true;
                            break;
                    }
                }
                else
                {
                    clovek2RadioButton.IsChecked = true;
                }
            }
        }
       

        // Udalost navazana na tlacitko OK
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {           
            if (nabidkaNovaHra)
            {
                dispecer2.ZahajHru();
                dispecer2.GetHra.VynulujPocitadloTahuBezPreskoku();
                dispecer2.GetHra.VynulujTahy();
                dispecer2.SetHracNaTahu = dispecer2.GetHrac1;

                if (pocitac2RadioButton.IsChecked == true && obtiznostHrace2ComboBox.SelectedIndex == 0)
                {
                    MessageBox.Show("Prosím, zadej obtížnost počítačového hráče ",
                        "Chyba v zadání", MessageBoxButton.OK, MessageBoxImage.Exclamation);                   
                }
                else
                {
                    if (pocitac1RadioButton.IsChecked == true && obtiznostHrace1ComboBox.SelectedIndex == 0)
                    {
                        MessageBox.Show("Prosím, zadej obtížnost počítačového hráče",
                            "Chyba v zadání", MessageBoxButton.OK, MessageBoxImage.Exclamation);                        
                    }
                    else
                    {
                        DialogResult = true; // po stisku ok musim nastavit celemu dialogu hodnotu true !!!! .....
                        Close();
                    }
                }
            }
            else
            {
                UlozZmeneneUdajeOHre();                
            }
        }


        // ukladam zmenu nastaveni hracu
        private void UlozZmeneneUdajeOHre()
        {
            if (pocitac1RadioButton.IsChecked == true && obtiznostHrace1ComboBox.SelectedIndex == 0)
            {
                MessageBox.Show("Prosím, zadej obtížnost počítačového hráče",
                    "Chyba v zadání", MessageBoxButton.OK, MessageBoxImage.Exclamation);               
            }
            else
            {
                if (pocitac2RadioButton.IsChecked == true && obtiznostHrace2ComboBox.SelectedIndex == 0)
                {
                    MessageBox.Show("Prosím, zadej obtížnost počítačového hráče",
                        "Chyba v zadání", MessageBoxButton.OK, MessageBoxImage.Exclamation);                   
                }
                else
                {
                    dispecer2.GetHrac1.SetJmeno(jmenoHrac1TextBox.Text);
                    dispecer2.GetHrac2.SetJmeno(jmenoHrac2TextBox.Text);

                    if (pocitac1RadioButton.IsChecked == true)
                    {
                        dispecer2.GetHrac1.SetJePocitacovyHrac = true;
                        dispecer2.GetHrac2.SetObtiznost = int.Parse(obtiznostHrace1ComboBox.SelectionBoxItem.ToString().Substring(0, 1));
                    } 
                    if (clovek1RadioButton.IsChecked == true)
                    {
                        dispecer2.GetHrac1.SetJePocitacovyHrac = false;
                    }
                    if (pocitac2RadioButton.IsChecked == true)
                    {
                        dispecer2.GetHrac2.SetJePocitacovyHrac = true;
                        dispecer2.GetHrac1.SetObtiznost = int.Parse(obtiznostHrace2ComboBox.SelectionBoxItem.ToString().Substring(0, 1));                        
                    }
                    if (clovek2RadioButton.IsChecked == true)
                    {
                        dispecer2.GetHrac2.SetJePocitacovyHrac = false;
                    }
                    DialogResult = true;
                    Close();
                } 
            }
        }

        

        // Udalost navazana na tlacitko Storno:
        private void StornoButton_Click(object sender, RoutedEventArgs e)
        {            
            Close();
        }       

        

        // Udalosti navazane na RadioButtony:
        private void pocitac1RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            obtiznostHrace1ComboBox.IsEnabled = true;
            checkBoxZpomalitPCH1.IsEnabled = true;
            labelZpomalitPCH1.IsEnabled = true;
        }

        private void pocitac1RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            obtiznostHrace1ComboBox.SelectedIndex = 0;
            obtiznostHrace1ComboBox.IsEnabled = false;
            checkBoxZpomalitPCH1.IsEnabled = false;
            checkBoxZpomalitPCH1.IsChecked = false;
            labelZpomalitPCH1.IsEnabled = false;
        }

        private void pocitac2RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            obtiznostHrace2ComboBox.IsEnabled = true;
            checkBoxZpomalitPCH2.IsEnabled = true;
            labelZpomalitPCH2.IsEnabled = true;
        }

        private void pocitac2RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            obtiznostHrace2ComboBox.SelectedIndex = 0;
            obtiznostHrace2ComboBox.IsEnabled = false;
            checkBoxZpomalitPCH2.IsEnabled = false;
            checkBoxZpomalitPCH2.IsChecked = false;
            labelZpomalitPCH2.IsEnabled = false;
        }

        // osetrim si klikani na popisek u checkBoxu : taky bude menit zaskrtavaci policko:
        private void labelZpomalitPCH1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (checkBoxZpomalitPCH1.IsChecked == true)
                checkBoxZpomalitPCH1.IsChecked = false;
            else
            {
                if (checkBoxZpomalitPCH1.IsChecked == false)
                    checkBoxZpomalitPCH1.IsChecked = true;
            }            
        }

        private void labelZpomalitPCH2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (checkBoxZpomalitPCH2.IsChecked == true)
                checkBoxZpomalitPCH2.IsChecked = false;
            else
            {
                if (checkBoxZpomalitPCH2.IsChecked == false)
                    checkBoxZpomalitPCH2.IsChecked = true;
            }
        }
    }
}
