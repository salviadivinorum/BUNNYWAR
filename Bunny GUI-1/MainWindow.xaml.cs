using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Microsoft.Win32;
using System.Diagnostics;

namespace Bunny_GUI_1
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    [Serializable]
    public partial class MainWindow : Window
    {
        [NonSerialized] // BACKGROUNDWORKER SE NESERIALIZUJE
        public static BackgroundWorker bw; // pro PC hrace
        public static BackgroundWorker bwNapoveda; // pro vypocet tahu Napovedy

        public static List<Tah> tahyCopy = new List<Tah>();
        public static bool zrusitVypocet = false; // DULEZITE PRO STOP BACKGROUNDWORKERU
       
        private static DispecerHry dispecerHry;
        Image cernaFigurka; // zakladni cerny kamen
        Image bilaFigurka; // zakladni bily kamen
        public List<UzivPrvekPolicko> seznamUzivPrvkuPolicko;
        public List<Brush> seznamBarevPozadiPolicek;
        public List<UzivPrvekPolicko> mujTahZeZelenehoDoPrazdneho = new List<UzivPrvekPolicko>();
        Deska deskaCopy = null;
        string vstup = "";
        Tah tah = null;
        string mujTah = "";        

        private bool prohlizetUkoncenouHru = false;
        private bool zacatekProgramu = true;
        private bool bylaChybaIO = false;               
        private string typHrace1CodeBehind;
        private string typHrace2CodeBehind;
        private string jmH1CodeBehind, jmH2CodeBehind;
        private string obtiznostHrace1CodeBehind;
        private string obtiznostHrace2CodeBehind;        
        private string otevreniSouboruSePodarilo = "";
        private string souborProUkladani = "";
        private bool stranyProhozeny = false;
        private bool hraSkoncila = false;           

        //prace s ObservableCollection - HistorieTahu:        
        public static HistorieTahu historieTahu = new HistorieTahu();
        public DispecerHry mujDispecerProSerializaci;

        // promenna na zpomalovani vypoctu z vedlejsiho okna - Chcecked
        public static bool zpomalenVypocetProPCHrace1;
        public static bool zpomalenVypocetProPCHrace2;

        // Proces help file:
        public Process procesUkazCHMsoubor;
       
        // procesy externich programu, ktere si hlidam:
        public List<Process> procesy = new List<Process>();  

        // Docela dlouhy konstruktor hlavniho okna hry
        public MainWindow()
        {            
            InitializeComponent();   
            
            // Vyroba seznamu vsech policek:
            seznamUzivPrvkuPolicko = new List<UzivPrvekPolicko>()
            {   A1,B1,C1,D1,E1,F1,G1,H1,
                A2,B2,C2,D2,E2,F2,G2,H2,
                A3,B3,C3,D3,E3,F3,G3,H3,
                A4,B4,C4,D4,E4,F4,G4,H4,
                A5,B5,C5,D5,E5,F5,G5,H5,
                A6,B6,C6,D6,E6,F6,G6,H6,
                A7,B7,C7,D7,E7,F7,G7,H7,
                A8,B8,C8,D8,E8,F8,G8,H8
            };

            // vyroba seznamu pozadi policek
            seznamBarevPozadiPolicek = new List<Brush>()
            {
                A1.Background,B1.Background,C1.Background,D1.Background,E1.Background,F1.Background,G1.Background,H1.Background,
                A2.Background,B2.Background,C2.Background,D2.Background,E2.Background,F2.Background,G2.Background,H2.Background,
                A3.Background,B3.Background,C3.Background,D3.Background,E3.Background,F3.Background,G3.Background,H3.Background,
                A4.Background,B4.Background,C4.Background,D4.Background,E4.Background,F4.Background,G4.Background,H4.Background,
                A5.Background,B5.Background,C5.Background,D5.Background,E5.Background,F5.Background,G5.Background,H5.Background,
                A6.Background,B6.Background,C6.Background,D6.Background,E6.Background,F6.Background,G6.Background,H6.Background,
                A7.Background,B7.Background,C7.Background,D7.Background,E7.Background,F7.Background,G7.Background,H7.Background,
                A8.Background,B8.Background,C8.Background,D8.Background,E8.Background,F8.Background,G8.Background,H8.Background,
            };

            dispecerHry = VytvorNovouHruBunnyWar();
            NastavStartovniPoziceFigurek();           
            dispecerHry.GetDeska.NastavStartovniKameny();
           
            // dulezite - tady mam nastavenu vazbu ListBoxu na tridu HistorieTahu, konkretne na vlastnost GetTahyVypsaneDetailne
            historieTahuListBox.ItemsSource = historieTahu.GetTahyVypsaneDetailne;

            // prace s BackGroundWorkerem:
            // prvni je pro pocitacoveho hrace:
            bw = new BackgroundWorker();        
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork); 
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            // druhy bwNapoveda je pro napovedu nej tahu pro cloveka (do 3-ti urovne):
            bwNapoveda = new BackgroundWorker();
            bwNapoveda.WorkerReportsProgress = true;
            bwNapoveda.WorkerSupportsCancellation = true;
            bwNapoveda.DoWork += new DoWorkEventHandler(bwNapoveda_DoWork);
            bwNapoveda.ProgressChanged += new ProgressChangedEventHandler(bwNapoveda_ProgressChanged);
            bwNapoveda.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bwNapoveda_RunWorkerCompleted);

            // uvitaci info radek ve status baru>
            hlaseniTextBlock.Text = "Pro start hry: volba Nová hra nebo Otevřít uloženou hru.";
        }


        // Prvni krok - vytvor vsechny instance mych hlavnich trid, ktere jednotne obsluhuje dispecer:     
        public static DispecerHry VytvorNovouHruBunnyWar()
        {            
            DispecerHry disp = VytvorDispeceraHry();
            disp.SetHrac1 = VytvorHrace();
            disp.SetHrac2 = VytvorHrace();
            disp.SetHra = VytvorHru();
            disp.SetGenerator = VytvorGeneratorTahu();
            disp.SetDeska = VytvorDesku();
            disp.SetHistorieTahu = VytvorHistoriiTahu(); // Tady mi jde o to, aby se soucasne s dispecerem ukladala i historie tahu            
            return disp;
        }           
        
        
        // Tady si vyrabim instance vsech mych trid, ktere jednotne pak obsluhuju v Dispecerovi:

        public static HistorieTahu VytvorHistoriiTahu()
        {
            return new HistorieTahu();
        }

        public static HistorieTahu GetHistorii
        {
            get { return historieTahu; }
        }

        public static DispecerHry VytvorDispeceraHry()
        {
            return new DispecerHry();
        }

        public static DispecerHry GetDispecerHry()
        {
            return dispecerHry;
        }
       
        public static BackgroundWorker GetBW
        {
            get { return bw; }
        } 

        public static Hrac VytvorHrace()
        {
            return new Hrac();
        }

        public static Hra VytvorHru()
        {
            return new Hra();
        }

        public static Deska VytvorDesku()
        {
            return new Deska();
        }

        public static GeneratorTahu VytvorGeneratorTahu()
        {
            return new GeneratorTahu();
        }


        // Zpomaluju si vypocet:
        public static bool GetZpomalenVypocetProPCHrace1
        {
            get { return zpomalenVypocetProPCHrace1; }
        }
        public static bool SetZpomalenVypocetProPCHrace1
        {
            set { zpomalenVypocetProPCHrace1 = value; }
        }


        public static bool GetZpomalenVypocetProPCHrace2
        {
            get { return zpomalenVypocetProPCHrace2; }
        }
        public static bool SetZpomalenVypocetProPCHrace2
        {
            set { zpomalenVypocetProPCHrace2 = value; }
        }

        // Rusim vypocet 
        public static bool SetZrusVypocet
        {
            set { zrusitVypocet = value; }
        }

        public static bool GetZrusVypocet
        {
            get { return zrusitVypocet; }
        }


        // Nekdy potrebuju zavolat "univerzalni" metody, ktere si umistim sem pro prehled:
        public static Pozice VytvorPozici(object sirka, object vyska)
        {
            Pozice pozice = new Pozice();
            pozice.SetVyska(vyska);
            pozice.SetSirka(sirka);
            return pozice;
        }

        public static Tah VytvorTah(List<Pozice> seznamPozic)
        {
            Tah tah = new Tah();
            tah.SetSeznamPozic = seznamPozic;
            return tah;
        }

        public static int VratOpacnyTypKamene(int typKamene)
        {
            if (typKamene == 1)
                return 2;
            if (typKamene == 2)
                return 1;
            else
                throw new Exception("Typ kamene " + typKamene + " neni evidovan v konstantach Programu!");
        }

     
       // pomocna vnorena trida na vyrobu textu do textboxu v xaml:
       public class TextBoxText
        {
            public string NastavText { get; set; } // pomocna trida a hned i  vlastnost NastavText
        }   


        // kopiravani dat z nastaveni okna do hlavniho okna:
        public void OkopirujDataZOknaNastaveniDoHlavnihoOkna(NastaveniHracuNabidka nabidka)
        {
            //Tady si kopiruju polozky z vedlejsiho okna NastaveniHracuNabidka:            
            if (nabidka.jmenoHrac1TextBox.Text == "")
                jmH1CodeBehind = " ";
            else
                jmH1CodeBehind = nabidka.jmenoHrac1TextBox.Text;

            jmH2CodeBehind = nabidka.jmenoHrac2TextBox.Text;
            obtiznostHrace1CodeBehind = nabidka.obtiznostHrace1ComboBox.SelectionBoxItem.ToString();
            obtiznostHrace2CodeBehind = nabidka.obtiznostHrace2ComboBox.SelectionBoxItem.ToString();

            if (nabidka.checkBoxZpomalitPCH1.IsChecked == true)
                SetZpomalenVypocetProPCHrace1 = true;
            else SetZpomalenVypocetProPCHrace1 = false;           

            if (nabidka.checkBoxZpomalitPCH2.IsChecked == true)
                SetZpomalenVypocetProPCHrace2 = true;
            else SetZpomalenVypocetProPCHrace2 = false;            

            if (nabidka.clovek1RadioButton.IsChecked == true)
                typHrace1CodeBehind = "člověk";
            else typHrace1CodeBehind = "počítač";

            if (nabidka.clovek2RadioButton.IsChecked == true)
                typHrace2CodeBehind = "člověk";
            else typHrace2CodeBehind = "počítač";

            // Tady jsem si nastavil DATA CONTEXT promennych na policka
            typHrace1.DataContext = new TextBoxText() { NastavText = typHrace1CodeBehind };
            typHrace2.DataContext = new TextBoxText() { NastavText = typHrace2CodeBehind };
            obtiznost1.DataContext = new TextBoxText() { NastavText = obtiznostHrace1CodeBehind };
            obtiznost2.DataContext = new TextBoxText() { NastavText = obtiznostHrace2CodeBehind };
            jmenoH1.DataContext = new TextBoxText() { NastavText = jmH1CodeBehind };
            jmenoH2.DataContext = new TextBoxText() { NastavText = jmH2CodeBehind };

            // Docela dulezite pak pro historii tahu:
            dispecerHry.GetHrac1.SetJmeno(jmH1CodeBehind);
            dispecerHry.GetHrac2.SetJmeno(jmH2CodeBehind);

            foreach (UIElement elem in MrizHlavni.Children)
            {
                elem.Refresh();
            }

            // pokud je hracem pocitac, musim to i zadat = true:
            // hodne dulezite - podle obtiznosti zadavam hloubku prochazeni:
            if (typHrace1.Text.Substring(0, 1) == "p")
            {
                dispecerHry.GetHrac1.SetJePocitacovyHrac = true;
                dispecerHry.GetHrac2.SetObtiznost = int.Parse(obtiznost1.Text.Substring(0, 1));
            }

            if (typHrace2.Text.Substring(0, 1) == "p")
            {
                dispecerHry.GetHrac2.SetJePocitacovyHrac = true;
                dispecerHry.GetHrac1.SetObtiznost = int.Parse(obtiznost2.Text.Substring(0, 1));
            }

            // pozor, musim upozornit, na to, ze i hraci jsou lidmi a ne pocitacem:
            if (typHrace1.Text.Substring(0, 1) == "č")
                dispecerHry.GetHrac1.SetJePocitacovyHrac = false;
            if (typHrace2.Text.Substring(0, 1) == "č")
                dispecerHry.GetHrac2.SetJePocitacovyHrac = false;
            
            // tady ukladam info pro dalsi kontroly - pri zmenach nastaveni hracu:
            UlozInfoOHracich();
        }

        /******************************************** UKONCENI HRY *************************************************************************/
              
        // UKONCENI CELEHO PROGRAMU:
        private void menuUkoncitPogram_Click(object sender, RoutedEventArgs e)
        {
            tlacitkoPause_Click(sender, e);
            Application.Current.MainWindow.Close();
        }       
        
        // udalost Closing:
        private void hlavniOkno_Closing(object sender, CancelEventArgs e)
        {  
            if (typHrace1.Text != "")
            {
                if (bw.IsBusy || bwNapoveda.IsBusy)
                {
                    tlacitkoPause_Click(this, new RoutedEventArgs());
                }

                string zprava = "Chcetet uložit rozehranou partii hry ?";
                string nadpis = "Ukončení programu";
                MessageBoxButton tlacitka = MessageBoxButton.YesNoCancel;
                MessageBoxImage ikona = MessageBoxImage.Information;
                MessageBoxResult result = MessageBox.Show(zprava, nadpis, tlacitka, ikona);

                if (result == MessageBoxResult.Yes)
                {
                    SaveFileDialog dlg = new SaveFileDialog();

                    dlg.FileName = "Bunnywar1";
                    dlg.DefaultExt = ".bin";
                    dlg.Filter = "Binnary File (.bin)|*.bin";

                    Nullable<bool> vysledek = dlg.ShowDialog();

                    if (vysledek == true)
                    {
                        SetZrusVypocet = true;
                        dispecerHry.SetHistorieTahu = historieTahu;
                        UlozInfoOHracich();

                        string filename = dlg.FileName;
                        souborProUkladani = filename;
                        SerializujBinarne(filename);
                        if (bylaChybaIO)
                        {
                            e.Cancel = true;
                        }

                        else
                        {
                            ZavriOknoNapovedy();
                            e.Cancel = false;
                        }
                    }
                    else
                    {
                        druheHlaseniTextBlock.Text = "Ukládání hry zrušeno !";
                        e.Cancel = true;
                    }
                }


                else
                if (result == MessageBoxResult.Cancel) // dal jsem cancel a chci zpet do hry
                {
                    e.Cancel = true;
                    return;
                }

                else  // dam nechci ulozit zadnou hru jenom ukoncit cely program
                {
                    ZavriOknoNapovedy();
                    e.Cancel = false;
                }
            }
            else
            {
                ZavriOknoNapovedy();
                e.Cancel = false;
            }
        }

       

        // UKONCENI PARTIE HRY - tlacitkem Stop:
        private void tlacitkoStopPartie_Click(object sender, RoutedEventArgs e)
        {
            if (GetDispecerHry().GetHracNaTahu == null)
                return;
            if (dispecerHry.GetZahajenaHRa == false)
                return;

            tlacitkoPause_Click(sender, e);            
            string zprava = "Chcete uložit rozehranou partii hry ?";
            string nadpis = "Ukončení rozehrané partie";
            MessageBoxButton tlacitka = MessageBoxButton.YesNoCancel;
            MessageBoxImage ikona = MessageBoxImage.Question;
            MessageBoxResult result = MessageBox.Show(zprava, nadpis, tlacitka, ikona);

            if (result == MessageBoxResult.Yes)
            {                
                SaveFileDialog dlg = new SaveFileDialog();

                dlg.FileName = "Bunnywar1";
                dlg.DefaultExt = ".bin";
                dlg.Filter = "Binnary File (.bin)|*.bin";

                Nullable<bool> vysledek = dlg.ShowDialog();

                if (vysledek == true)
                {
                    SetZrusVypocet = true;
                    dispecerHry.SetHistorieTahu = historieTahu;
                    UlozInfoOHracich();

                    string filename = dlg.FileName;
                    souborProUkladani = filename;
                    SerializujBinarne(filename);

                    historieTahu.GetTahyVypsaneDetailne.Clear();
                    historieTahu.GetCistySeznamOdehranychTahu.Clear();
                    NastavDoPocatecnihoStavu();                    
                }
                else
                {
                    zprava = "Hra nebyla uložena !";
                    nadpis = "Uložení hry se nezdařilo";
                    tlacitka = MessageBoxButton.OK;
                    ikona = MessageBoxImage.Information;
                    MessageBox.Show(zprava, nadpis, tlacitka, ikona);
                    return;
                }
            }
            else
            {
                if (result == MessageBoxResult.No)
                {
                    NastavDoPocatecnihoStavu();                    
                }

                else
                {
                    return;
                }
            }                         
        }


        private void NastavDoPocatecnihoStavu()
        { 
            InicializaceHlavnihoOknadoPocatecnihoStavu();
            dispecerHry.GetDeska.NastavStartovniKameny();
            hraSkoncila = false;

            // smaze starou desku a vykresli novou plus dodela vedlejsi efekty:
            SmazDeskuCislemHraceNula(seznamUzivPrvkuPolicko);
            SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
            ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);
            mujTahZeZelenehoDoPrazdneho.Clear();
           
            GetDispecerHry().GetHra.VynulujPocitadloTahuBezPreskoku();
            pocetTahuBPTextBlock.Text = "0";
            GetDispecerHry().GetHra.VynulujTahy();
            dispecerHry.GetHrac1.SetJePocitacovyHrac = false;
            dispecerHry.GetHrac2.SetJePocitacovyHrac = false;
            dispecerHry.SetHracNaTahu = null;
            dispecerHry.SetZahajenaHra = false;
            zacatekProgramu = true;           

            // Mazu si historii:
            historieTahu.GetTahyVypsaneDetailne.Clear();
            historieTahu.GetCistySeznamOdehranychTahu.Clear();         
            
            // nastavuju cista policka v xamlu:
            typHrace1.DataContext = new TextBoxText() { NastavText = "" };
            typHrace2.DataContext = new TextBoxText() { NastavText = "" };
            obtiznost1.DataContext = new TextBoxText() { NastavText = "" };
            obtiznost2.DataContext = new TextBoxText() { NastavText = "" };
            jmenoH1.DataContext = new TextBoxText() { NastavText = "" };
            jmenoH2.DataContext = new TextBoxText() { NastavText = "" };
            zbyvaKamenuH1.Text = "16";
            zbyvaKamenuH2.Text = "16";
            hlaseniTextBlock.Text = "Pro start hry: volba Nová hra nebo Otevřít uloženou hru.";

            foreach (UIElement elem in MrizHlavni.Children)
            {
                elem.Refresh();
            }
            if (stranyProhozeny)
                dispecerHry.ProvedZmenuHracu();

            VykresliDesku(); /// Start nove desky 
        }               
        

        // Pokud stoji hrac na policku, musi ho informovat jakou barvou ma ten hrac
        public void NastavObsazenostPolickaBilymHracem(UzivPrvekPolicko policko)
        {
            policko.SetJakouBarvouJsemObsazeno = BarvaHrace.bila;
        }


        public void NastavObsazenostPolickaCernymHracem(UzivPrvekPolicko policko)
        {
            policko.SetJakouBarvouJsemObsazeno = BarvaHrace.cerna;
        }


        // Zvyrazneni pozadi policka zelenou barvou - jenom tehdy kdyz ma policko slot oznecenoZelene = true:
        public void NastavJsemOznacenoZeleneFalse(UzivPrvekPolicko policko)
        {
            policko.SetJsemOznacenoZelene = false;
        }

        public void NastavJsemOznacenoZeleneTrue(UzivPrvekPolicko policko)
        {
            policko.SetJsemOznacenoZelene = true;
        }

               
        /**************************************NASTAVENI A ZMENA HRY - TLACITKO NASTAVENI ***********************************************/
       
        private void tlacitkoNastaveni_Click(object sender, RoutedEventArgs e)
        {
            if (!dispecerHry.GetZahajenaHRa || typHrace1.Text == "")
                return;               

            SetZrusVypocet = false;
            if (bw.IsBusy || bwNapoveda.IsBusy)
            {
                SetZrusVypocet = true;
                bw.CancelAsync();
                bwNapoveda.CancelAsync();
            }            

            NastaveniHracuNabidka nabidka2 = new NastaveniHracuNabidka(false);
            nabidka2.ShowDialog();
           
            if (nabidka2.DialogResult.HasValue && nabidka2.DialogResult.Value)
            {
                
                SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
                ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);
                VykresliDesku(); /// Start nove desky
                if (dispecerHry.GetHracNaTahu != null)
                {
                    OkopirujDataZOknaNastaveniDoHlavnihoOkna(nabidka2);
                }
                else
                    NastavDoPocatecnihoStavu();

                foreach (UIElement elem in MrizHlavni.Children)
                {
                    elem.Refresh();
                }

                if (dispecerHry.GetHracNaTahu != null)
                {
                    hlaseniTextBlock.Text = "";
                    if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                        HrejZaPocitac();                    
                } 
            }
            else
            {
                return;
            }                
        }
             
        /****************************************************NOVA HRA - TLACITKO *******************************************************/
      
        // Spusteni nabidky Nova hra
        private void tlacitkoNovaHra_Click(object sender, RoutedEventArgs e)
        {
            if (!dispecerHry.GetZahajenaHRa && zacatekProgramu)
                dispecerHry.SetZahajenaHra = true;

            if (dispecerHry.GetZahajenaHRa)
            {
                tlacitkoPause_Click(sender, e);

                if (!(typHrace1.Text == ""))
                { 
                    if (MessageBox.Show("Jedna hra uz běží. Chcete zahájit novou hru ?", "Zahájení nové hry !",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    {
                        return;
                    }
                    else
                    {
                        tlacitkoPause_Click(sender, e);
                        VyvolejStartNoveHry();
                    }
                }
                else
                {
                    zacatekProgramu = false;
                    VyvolejStartNoveHry();
                }
            }         
        }



        private void VyvolejStartNoveHry()
        {            
            NastaveniHracuNabidka nabidka = new NastaveniHracuNabidka(true);
            nabidka.ShowDialog();
            if (nabidka.DialogResult.HasValue && nabidka.DialogResult.Value)            
            {
                SetZrusVypocet = false;
                if (bw.IsBusy || bwNapoveda.IsBusy)
                {
                    SetZrusVypocet = true;
                    bw.CancelAsync();
                    bwNapoveda.CancelAsync();
                }
                
                souborProUkladani = "";
                InicializaceHlavnihoOknadoPocatecnihoStavu();
                dispecerHry.GetDeska.NastavStartovniKameny();
                SmazDeskuCislemHraceNula(seznamUzivPrvkuPolicko);
                SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
                ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);
                GetDispecerHry().GetHra.VynulujPocitadloTahuBezPreskoku();
                pocetTahuBPTextBlock.Text = "0";
                GetDispecerHry().GetHra.VynulujTahy();
                historieTahu.GetTahyVypsaneDetailne.Clear();
                historieTahu.GetCistySeznamOdehranychTahu.Clear();

                if (stranyProhozeny)
                    dispecerHry.ProvedZmenuHracu();


                VykresliDesku(); /// Start nove desky
                if (dispecerHry.GetHracNaTahu != null)
                {
                    OkopirujDataZOknaNastaveniDoHlavnihoOkna(nabidka);
                }
                else
                    NastavDoPocatecnihoStavu();

                foreach (UIElement elem in MrizHlavni.Children)
                {
                    elem.Refresh();
                }

                if (dispecerHry.GetHracNaTahu != null)
                {
                    if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                        HrejZaPocitac();
                }
            }
            else
            {               
                return;
            }
        }


        
        // Nastaveni figur na startu, vyroba barevnych figurek:
        public void NastavStartovniPoziceFigurek()
        {
            cernaFigurka = new Image();
            cernaFigurka.Source = new BitmapImage(new Uri("Ikonky/Pawn-blackPNG-Clear.png", UriKind.Relative));
            cernaFigurka.Height = 50;
            cernaFigurka.Width = 50;

            bilaFigurka = new Image();
            bilaFigurka.Source = new BitmapImage(new Uri("Ikonky/Pawn-whitePNG-Clear.png", UriKind.Relative));
            bilaFigurka.Height = 50;
            bilaFigurka.Width = 50;

            List<UzivPrvekPolicko> startovniPoziceCernych;
            startovniPoziceCernych = new List<UzivPrvekPolicko>()
            {
                A7, B7,C7,D7,E7,F7,G7,H7,
                A8,B8,C8,D8,E8,F8,G8,H8
            };

            List<UzivPrvekPolicko> startovniPoziceBilych;
            startovniPoziceBilych = new List<UzivPrvekPolicko>()
            {
                A1, B1, C1, D1, E1, F1, G1, H1,
                A2, B2, C2, D2, E2, F2, G2, H2
            };

            foreach (UzivPrvekPolicko policko in startovniPoziceCernych)
            {
                policko.platno.Children.Add(klonujCernou());
                NastavObsazenostPolickaCernymHracem(policko);
            }

            foreach (UzivPrvekPolicko policko in startovniPoziceBilych)
            {
                policko.platno.Children.Add(klonujBilou());
                NastavObsazenostPolickaBilymHracem(policko);
            }

            foreach (UzivPrvekPolicko policko in seznamUzivPrvkuPolicko)
            {
                NastavJsemOznacenoZeleneFalse(policko);
            }
        }

        // Metody, ktere mi vyrabeji klony bilych a cernych figurek:
        public Image klonujBilou()
        {
            Image klonovany = new Image();
            klonovany.Source = bilaFigurka.Source;
            klonovany.Height = 50;
            klonovany.Width = 50;
            return klonovany;
        }

        public Image klonujCernou()
        {
            Image klonovany = new Image();
            klonovany.Source = cernaFigurka.Source;
            klonovany.Height = 50;
            klonovany.Width = 50;
            return klonovany;
        }

        // Test na oznacenost policka Zelenou barvou:
        public bool TestZdaUzNejakePolickoJeZelene(List<UzivPrvekPolicko> seznam)
        {            
                foreach (UzivPrvekPolicko policko in seznam)
                {
                    if (policko.GetJsemOznacenoZelene == true)
                        return true;
                }            
            
            return false;
        }

        // Test na prazdnotu policka:
        public bool TestZdaLiJePolickoPrazdne(UzivPrvekPolicko policko)
        {
            if (policko.GetJakouBarvouJsemObsazeno == BarvaHrace.zadna)
                return true;
            else
                return false;
        }

        public bool TestZdaliJePolickoObsazenoMymKamenem(UzivPrvekPolicko policko, List<UzivPrvekPolicko> mujTahZeZelenehoDoPrazdneho)
        {
            BarvaHrace barvahraceNaTahu = dispecerHry.GetHracNaTahu.GetBarvuHrace;
            if (policko.GetJakouBarvouJsemObsazeno != BarvaHrace.zadna && policko.GetJakouBarvouJsemObsazeno == barvahraceNaTahu 
                && mujTahZeZelenehoDoPrazdneho.Count()>1)
                return true;
            else
                return false;
        }

        // Smazu Zelene oznaceni vsem polickum v seznamu:
        public void SmazZeleneOznaceniSeznamuPolicek(List<UzivPrvekPolicko> seznam)
        {
            foreach (UzivPrvekPolicko policko in seznam)
                policko.SetJsemOznacenoZelene = false;
        }

        //Obnov pozadi policka dle puvodniho rozvrhu:
        public void ObnovMiBarvuPozadi(List<UzivPrvekPolicko> seznam)
        {
            for (int i = 0; i < seznamUzivPrvkuPolicko.Count(); i++)
                seznam[i].Background = seznamBarevPozadiPolicek[i];
        }

        // Premazani desky obsazenosti - cislo hrace 0
        public void SmazDeskuCislemHraceNula(List<UzivPrvekPolicko> seznam)
        {
            foreach (UzivPrvekPolicko policko in seznam)
                policko.SetChrace = 0;
        }
        
        // Tady toto je ma osobni vyhra, jelikoz mam vyrobeny tah po vsem to klikani do desky
        public string PrevedSeznaUzivPrvkuPolickoNaString(List<UzivPrvekPolicko> seznam)
        {
            string mt = "";
            foreach (UzivPrvekPolicko policko in seznam)
            {
                mt = mt + policko.Name;
            }
            return mujTah+mt;
        }


        // prevod Tah na policka:
        public List<UzivPrvekPolicko> PrevedTahNaSeznamUzivPrvkuPolicko(Tah tah)
        {
            string prevedenyTah = tah.VratJakoText();
            List<UzivPrvekPolicko> seznamPolicekKVykresleni = new List<UzivPrvekPolicko>();            
            int pocetPolicek = (prevedenyTah.Count() - 2) / 2;
            string orezany = prevedenyTah.Substring(1, prevedenyTah.Count() - 2);          

            for (int i = 0; i < orezany.Count(); ++i)
            {
                UzivPrvekPolicko po = new UzivPrvekPolicko();
                po.Name = string.Concat(orezany[i], orezany[i + 1]).ToUpper(); // spojuje dohormady v retezec "Concatenate" - spojovat                
                seznamPolicekKVykresleni.Add(po);
                i = i + 1;
            }
            return seznamPolicekKVykresleni;
        }



        // Prekresleni tech vybranych policek jinym pozadim - zlute jako napoveda nej tahu:
        public void VystupZlute(List<UzivPrvekPolicko> seznamKVykresleni)
        {
            foreach (UzivPrvekPolicko po in seznamKVykresleni)
            {
                foreach (UzivPrvekPolicko policko in seznamUzivPrvkuPolicko)
                {
                    if (po.Name == policko.Name)
                    {
                        policko.Background = Brushes.Yellow;                        
                    } 
                }
            }
        }


        // tlacitko nejlepsi tah:
        private void tlacitkoNejlepsiTah_Click(object sender, RoutedEventArgs e)
        {           

            int pocetKamenuBileho = dispecerHry.GetHrac1.VratPocetKamenu();
            int pocetKamenuCerneho = dispecerHry.GetHrac2.VratPocetKamenu();            

            if (jmenoH1.Text == "" || KonecHry() || pocetKamenuBileho <= 1 || pocetKamenuCerneho <= 1)
                return;

                if (dispecerHry.GetZahajenaHRa)
            {
                //AktualizujKamenyVyhruNeboRemizu();
                if (KonecHry())
                {
                    if (!prohlizetUkoncenouHru)
                        NastavDoPocatecnihoStavu();
                }                   

                ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);
                SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
                mujTahZeZelenehoDoPrazdneho.Clear();

                if (!dispecerHry.GetHracNaTahu.GetJePocitacovyHrac && !bw.IsBusy)
                {
                    SetZrusVypocet = false;
                    hlaseniTextBlock.Text = "";
                    NastartujVypocetNejlepsihoTahuNaPozadi();
                }
            }
        }


        // Vypisuje nej tah na obrazovku:
        public void bwNapoveda_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                hlaseniTextBlock.Text = "Výpočet nápovědy zrušen !";
                SetZrusVypocet = false;
            }

            else if (!(e.Error == null))
            {
                hlaseniTextBlock.Text = "Error: " + e.Error.Message;
            }
            else
            {
                tah = (Tah)e.Result;
                vypocetTextBlock.Text = "";
                mujProgressBar.Value = 0;
                hlaseniTextBlock.Text = "Nápověda nejlepšího tahu: " + tah.VratJakoText();
                string nejlepsiProCloveka = tah.VratJakoText();
                List<UzivPrvekPolicko> nejtahSeznam = new List<UzivPrvekPolicko>();
                nejtahSeznam = PrevedTahNaSeznamUzivPrvkuPolicko(tah);
                VystupZlute(nejtahSeznam);
                VykresliDesku();
                SetZrusVypocet = false;
            }
        }


        /******************************************************************************************************************************/

        // pomocna metoda na swap hracu:
        internal Hrac VratProtihrace(Hrac hrac)
        {
            if (hrac.Equals(dispecerHry.GetHrac1))
                return dispecerHry.GetHrac2;
            if (hrac.Equals(dispecerHry.GetHrac2))
                return dispecerHry.GetHrac1;
            else
                throw new Exception("Hrac neni hracem hry");
        }

        public void ProhodHraceNaTahu()
        {
            if (dispecerHry.GetHracNaTahu.Equals(dispecerHry.GetHrac1))
                dispecerHry.SetHracNaTahu = dispecerHry.GetHrac2;
            else
                dispecerHry.SetHracNaTahu = dispecerHry.GetHrac1;            
        }

        // kontrola na klikani v listboxu:
        public void KontrolaTahuVListboxu()
        {
            if (historieTahuListBox.Items.Count != 0)
            {
                if (dispecerHry.GetHra.GetTahy.Count() < historieTahuListBox.Items.Count)
                {
                    int konecIndexuListBoxu = historieTahuListBox.Items.Count - 1;                    
                    int konevIndexuGetTahy = dispecerHry.GetHra.GetTahy.Count() - 1;  

                    for (int i = konecIndexuListBoxu; i > konevIndexuGetTahy; i--)
                    {
                        ObservableCollection<string> tahy = new ObservableCollection<string>();
                        tahy = historieTahu.GetTahyVypsaneDetailne;                       
                        string tahoun = tahy.ElementAt(i);
                        Tah tahek = historieTahu.GetCistySeznamOdehranychTahu.ElementAt(i);
                        historieTahu.OdeberZHistorie(tahoun, tahek);
                    } 
                }
            }
        }
   

        // vstup mysi na policko
        private void VsechnaPolicka_MouseEnter(object sender, MouseEventArgs e)
        {           
            if (GetDispecerHry().GetHracNaTahu == null)
                return;
            if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                return;

            UzivPrvekPolicko policko = (UzivPrvekPolicko)sender;   

            if (policko.Background != Brushes.LightGreen &&
                 policko.Background != Brushes.OrangeRed)

            { 
                policko.Background = Brushes.LightBlue;
                policko.Refresh();
            }
        }
               

        // odcchod mysi z policka
        private void VsechnaPolicka_MouseLeave(object sender, MouseEventArgs e)
        {
            if (GetDispecerHry().GetHracNaTahu == null)
                return;
            if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                return;

            UzivPrvekPolicko policko = (UzivPrvekPolicko)sender;            

            if (policko.Background == Brushes.LightBlue)
            {                
                UzivPrvekPolicko vysl = seznamUzivPrvkuPolicko.Find(item => item.Name == policko.Name);              

                for (int i= 0; i<seznamUzivPrvkuPolicko.Count(); i++)
                {
                    if (seznamUzivPrvkuPolicko[i].Name == vysl.Name)
                        policko.Background = seznamBarevPozadiPolicek[i];
                }
                policko.Refresh();
            }
        }
                
        /*************************************** K L I K   LEVYM TLACITKEM MYSI    N A  P O L I C K O ***********************************/
         
        // Tady je hlavni obsluzna metoda odalosti Levy klik na policko:     
        private void VsechnaPolicka_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            druheHlaseniTextBlock.Text = "";
            if (GetDispecerHry().GetHracNaTahu == null || dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                return;
            if (bwNapoveda.IsBusy)
                tlacitkoPause_Click(sender, e);          
            
            // Kontrola na konec hry
            if(historieTahuListBox.Items.Count >= 1)
            {
                int posledni = historieTahuListBox.Items.Count - 1;
                int pocetKamenuBileho = dispecerHry.GetHrac1.VratPocetKamenu();
                int pocetKamenuCerneho = dispecerHry.GetHrac2.VratPocetKamenu();

                if (KonecHry() || pocetKamenuBileho <= 1 || pocetKamenuCerneho <= 1)
                {
                    hlaseniTextBlock.Text = "Konec partie hry";
                    return;
                }
            }       
          
            KontrolaTahuVListboxu(); 

            hlaseniTextBlock.Text = ""; // tady si mazu ma chybova hlaseni dole ve status baru
            UzivPrvekPolicko policko = (UzivPrvekPolicko)sender;
            tah = null;

            if (policko.GetJakouBarvouJsemObsazeno == GetDispecerHry().GetHracNaTahu.GetBarvuHrace &&
                   !TestZdaUzNejakePolickoJeZelene(seznamUzivPrvkuPolicko))
            {
                policko.Background = Brushes.LightGreen;
                policko.Refresh();
                policko.SetJsemOznacenoZelene = true;
                mujTahZeZelenehoDoPrazdneho.Add(policko);
                return;
            }


            // Tady jsem testoval: 1 policko je uz zelene, na druhe jsem kliknul a je prazdne a pouze 1x jsem kliknul na to prazdne
            if (TestZdaUzNejakePolickoJeZelene(seznamUzivPrvkuPolicko) &&
               (TestZdaLiJePolickoPrazdne(policko) || 
               TestZdaliJePolickoObsazenoMymKamenem(policko, mujTahZeZelenehoDoPrazdneho)) &&
               mujTahZeZelenehoDoPrazdneho.Count() >= 1)
            {

                mujTahZeZelenehoDoPrazdneho.Add(policko);
                // Tady mam svuj textovy vstup tahu:
                vstup = PrevedSeznaUzivPrvkuPolickoNaString(mujTahZeZelenehoDoPrazdneho);                
                tah = ZpracujRetezecNaTah(vstup);                
                deskaCopy = dispecerHry.GetDeska.ZkusZahratTah(tah);

                // moznost dalsiho preskoku  ....
                if (deskaCopy != null)
                {
                    if (tah.GetPreskoceneKameny.Count() != 0 && dispecerHry.JeMoznostPreskoku(deskaCopy, tah.GetSeznamPozic.Last()))
                    {
                        hlaseniTextBlock.Text = "Je možno skákat dále. Tah musíš doplnit kliknutím na další políčko.";
                        policko.Background = Brushes.OrangeRed;
                        return;
                    }
                }

                // univerzalni odpoved na nesmyslny tah ....
                if (deskaCopy == null)
                {
                    hlaseniTextBlock.Text = "Nepovolený tah !";
                    mujTah = "";
                }


                // No a tady konecne vykresluji spravne provedeny tah:
                if (deskaCopy != null)
                {

                    dispecerHry.SetDeska = deskaCopy;
                    VykresliDesku();
                    ProvedVedlejsiEfektyPoZahraniTahu(tah); // Info do okenka hlaseniTextBlock  
                    AktualizujKamenyVyhruNeboRemizu();
                    if (KonecHry())
                    {
                        dispecerHry.ProhodHraceNaTahu();
                        if (!prohlizetUkoncenouHru)
                            NastavDoPocatecnihoStavu();
                        return;
                    }
                    else
                    {
                        dispecerHry.ProhodHraceNaTahu();
                        if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                            HrejZaPocitac();
                    }
                }
                // Po uspesnem provedeni tahu se mazou promenne:
                SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
                ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);
                mujTahZeZelenehoDoPrazdneho.Clear();
                
                // Tady si osetrim kdyz vyhral pocitac:
                if (dispecerHry.GetHracNaTahu != null)
                {
                    AktualizujKamenyVyhruNeboRemizu();
                    if (KonecHry())
                    {

                        if (!prohlizetUkoncenouHru)
                            NastavDoPocatecnihoStavu();
                    }
                }
            }
            else
            {
                if (TestZdaUzNejakePolickoJeZelene(seznamUzivPrvkuPolicko) &&
                   (policko.GetJakouBarvouJsemObsazeno == BarvaHrace.bila ||
                    policko.GetJakouBarvouJsemObsazeno == BarvaHrace.cerna) && 
                    mujTahZeZelenehoDoPrazdneho.Count() >= 1)
                {
                    hlaseniTextBlock.Text = "Nepovolený tah !";
                    SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
                    ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);
                    mujTahZeZelenehoDoPrazdneho.Clear();
                }
                else
                {

                    if (policko.GetJakouBarvouJsemObsazeno != GetDispecerHry().GetHracNaTahu.GetBarvuHrace)
                    {
                        hlaseniTextBlock.Text = "Musíš hrát svými kameny !";
                    }
                }
            } 
        }


        public void DotazNaUlozeniHryPoVyhre()
        {
            string zprava = "Chcete uložit tuto dohranou partii hry?";
            string nadpis = "Uložení hry";
            MessageBoxButton tlacitka = MessageBoxButton.YesNoCancel;
            MessageBoxImage ikona = MessageBoxImage.Question;
            MessageBoxResult result = MessageBox.Show(zprava, nadpis, tlacitka, ikona);

            if (result == MessageBoxResult.Yes)
            {                
                dispecerHry.SetHistorieTahu = historieTahu;
                UlozInfoOHracich();

                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = "Bunnywar1";
                dlg.DefaultExt = ".bin";
                dlg.Filter = "Binnary File (.bin)|*.bin";

                Nullable<bool> vysledek = dlg.ShowDialog();

                if (vysledek == true)
                {
                    string filename = dlg.FileName;
                    souborProUkladani = filename;
                    SerializujBinarne(filename);
                    SetZrusVypocet = false;
                    prohlizetUkoncenouHru = true;
                }
                else
                {
                    SetZrusVypocet = false;
                    prohlizetUkoncenouHru = true;
                    return;
                }
            }
            else 
                if (result == MessageBoxResult.Cancel)
            {
                prohlizetUkoncenouHru = true;
                return;
            }
            else
                if (result == MessageBoxResult.No)
            {

                SetZrusVypocet = true;
                dispecerHry.SetZahajenaHra = false;
                prohlizetUkoncenouHru = false;
                return;
            }            
        }
            
               
        /******************************************PRAVE TLACITKO MYSI ****************************************************************/
     
        // Pravym tlacitkem pouze od-znacuji vsem polickum pozadi na default
        private void VsechnaPolicka_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!dispecerHry.GetZahajenaHRa || typHrace1.Text == "")
                return;
            if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                return;

            hlaseniTextBlock.Text = ""; // mazu ma hlaseni dole ve status baru
            druheHlaseniTextBlock.Text = "";
            SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
            ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);
            mujTahZeZelenehoDoPrazdneho.Clear();
        }


       
        // jednoduse - hrej za pocitac + kontroly
        private void HrejZaPocitac()
        {
            if (KonecHry())
            {
                if (!prohlizetUkoncenouHru)
                    NastavDoPocatecnihoStavu();
                return;
            }
            else
            {
                KontrolaTahuVListboxu();
                NastartujVypocetTahuNaPozadi();
            }
        }  
        
               
        // ******************************************* VYKRESLI DESKU *******************************************************************/
      
        public void VykresliDesku()
        {
            Deska deska = dispecerHry.GetDeska;

            A8.SetChrace = deska.GetHraciDeska[7, 0];           
            B8.SetChrace = deska.GetHraciDeska[7, 1];
            C8.SetChrace = deska.GetHraciDeska[7, 2];
            D8.SetChrace = deska.GetHraciDeska[7, 3];
            E8.SetChrace = deska.GetHraciDeska[7, 4];
            F8.SetChrace = deska.GetHraciDeska[7, 5];
            G8.SetChrace = deska.GetHraciDeska[7, 6];
            H8.SetChrace = deska.GetHraciDeska[7, 7];

            A7.SetChrace = deska.GetHraciDeska[6, 0];
            B7.SetChrace = deska.GetHraciDeska[6, 1];
            C7.SetChrace = deska.GetHraciDeska[6, 2];
            D7.SetChrace = deska.GetHraciDeska[6, 3];
            E7.SetChrace = deska.GetHraciDeska[6, 4];
            F7.SetChrace = deska.GetHraciDeska[6, 5];
            G7.SetChrace = deska.GetHraciDeska[6, 6];
            H7.SetChrace = deska.GetHraciDeska[6, 7];

            A6.SetChrace = deska.GetHraciDeska[5, 0];
            B6.SetChrace = deska.GetHraciDeska[5, 1];
            C6.SetChrace = deska.GetHraciDeska[5, 2];
            D6.SetChrace = deska.GetHraciDeska[5, 3];
            E6.SetChrace = deska.GetHraciDeska[5, 4];
            F6.SetChrace = deska.GetHraciDeska[5, 5];
            G6.SetChrace = deska.GetHraciDeska[5, 6];
            H6.SetChrace = deska.GetHraciDeska[5, 7];

            A5.SetChrace = deska.GetHraciDeska[4, 0];
            B5.SetChrace = deska.GetHraciDeska[4, 1];
            C5.SetChrace = deska.GetHraciDeska[4, 2];
            D5.SetChrace = deska.GetHraciDeska[4, 3];
            E5.SetChrace = deska.GetHraciDeska[4, 4];
            F5.SetChrace = deska.GetHraciDeska[4, 5];
            G5.SetChrace = deska.GetHraciDeska[4, 6];
            H5.SetChrace = deska.GetHraciDeska[4, 7];

            A4.SetChrace = deska.GetHraciDeska[3, 0];
            B4.SetChrace = deska.GetHraciDeska[3, 1];
            C4.SetChrace = deska.GetHraciDeska[3, 2];
            D4.SetChrace = deska.GetHraciDeska[3, 3];
            E4.SetChrace = deska.GetHraciDeska[3, 4];
            F4.SetChrace = deska.GetHraciDeska[3, 5];
            G4.SetChrace = deska.GetHraciDeska[3, 6];
            H4.SetChrace = deska.GetHraciDeska[3, 7];

            A3.SetChrace = deska.GetHraciDeska[2, 0];
            B3.SetChrace = deska.GetHraciDeska[2, 1];
            C3.SetChrace = deska.GetHraciDeska[2, 2];
            D3.SetChrace = deska.GetHraciDeska[2, 3];
            E3.SetChrace = deska.GetHraciDeska[2, 4];
            F3.SetChrace = deska.GetHraciDeska[2, 5];
            G3.SetChrace = deska.GetHraciDeska[2, 6];
            H3.SetChrace = deska.GetHraciDeska[2, 7];

            A2.SetChrace = deska.GetHraciDeska[1, 0];
            B2.SetChrace = deska.GetHraciDeska[1, 1];
            C2.SetChrace = deska.GetHraciDeska[1, 2];
            D2.SetChrace = deska.GetHraciDeska[1, 3];
            E2.SetChrace = deska.GetHraciDeska[1, 4];
            F2.SetChrace = deska.GetHraciDeska[1, 5];
            G2.SetChrace = deska.GetHraciDeska[1, 6];
            H2.SetChrace = deska.GetHraciDeska[1, 7];

            A1.SetChrace = deska.GetHraciDeska[0, 0];
            B1.SetChrace = deska.GetHraciDeska[0, 1];
            C1.SetChrace = deska.GetHraciDeska[0, 2];
            D1.SetChrace = deska.GetHraciDeska[0, 3];
            E1.SetChrace = deska.GetHraciDeska[0, 4];
            F1.SetChrace = deska.GetHraciDeska[0, 5];
            G1.SetChrace = deska.GetHraciDeska[0, 6];
            H1.SetChrace = deska.GetHraciDeska[0, 7];
            

            foreach (UzivPrvekPolicko prvek in seznamUzivPrvkuPolicko)
                prvek.platno.Children.Clear();

            foreach (UzivPrvekPolicko prvek in seznamUzivPrvkuPolicko)
            {
                if (prvek.GetCHrace == 1)
                {
                    prvek.platno.Children.Add(klonujBilou());
                    prvek.SetJakouBarvouJsemObsazeno = BarvaHrace.bila;
                }
                if (prvek.GetCHrace == 2)
                {
                    prvek.platno.Children.Add(klonujCernou());
                    prvek.SetJakouBarvouJsemObsazeno = BarvaHrace.cerna;
                }
                    
                if (prvek.GetCHrace == 0)
                {
                    prvek.platno.Children.Clear();
                    prvek.SetJakouBarvouJsemObsazeno = BarvaHrace.zadna;
                }  
            } 
            
            zbyvaKamenuH1.Text = dispecerHry.GetHrac1.VratPocetKamenu().ToString(); // aktualizuje se mi pocet kamenu v okenkach
            zbyvaKamenuH2.Text = dispecerHry.GetHrac2.VratPocetKamenu().ToString();

            // refresh Mrize hlavni + jejich deti v GUI:
            foreach (UIElement elem in MrizHlavni.Children)
            {
                elem.Refresh();
            }
        }
        
        
       

        // Osetreni zadani vstupu (tahu) - puvodni metoda z konzolove varinaty hry:
        private Tah ZpracujRetezecNaTah(string vstup)
        {
            Pozice p = null;
            List<Pozice> seznamPozic = new List<Pozice>();
            char sirka = '\u0000'; // null hodnota pro typ char = '\u0000'
            char vyska = '\u0000';


            for (int i = 0; i < vstup.Length; ++i)
            {
                char znak = vstup.ElementAt(i);
                if (znak == 32) continue;
                if (sirka == '\u0000')
                {
                    sirka = znak;
                    continue;
                }
                vyska = znak;
                try
                {
                    p = VytvorPozici(sirka, vyska);
                    seznamPozic.Add(p);
                    sirka = '\u0000';
                    vyska = '\u0000';
                    p = null;
                    continue;
                }
                catch
                {
                    MessageBox.Show("Souradnice " + sirka + vyska + " je mimo rozsah desky");
                    return null;
                }
            }

            if (sirka != '\u0000' && vyska == '\u0000')
            {
                MessageBox.Show("Retezec " + vstup + " neni korektne zadany tah !");
                return null;
            }

            try
            {
                return VytvorTah(seznamPozic); // vysledkem je nova instance tridy Tah
            }

            catch
            {
                MessageBox.Show("Pocet " + seznamPozic.Count() + " projitych tahu v poli neni povolen !");
                return null;
            }
        }

        // Aktualizuju si pocty kamenu a podminky vyhry a remizy + delam vedlejsi efekty (oknoVitezstvi, listbox, status bar):
        private void AktualizujKamenyVyhruNeboRemizu()
        {
            if (historieTahu.GetTahyVypsaneDetailne.Count() == 0)
                return;
            zbyvaKamenuH1.Text = dispecerHry.GetHrac1.VratPocetKamenu().ToString();
            zbyvaKamenuH2.Text = dispecerHry.GetHrac2.VratPocetKamenu().ToString();           

            string posledni = historieTahu.GetTahyVypsaneDetailne.Last();
            int indexPosledniho = historieTahu.GetTahyVypsaneDetailne.Count() - 1;

            // podminka na vyhru splnena
            if (dispecerHry.GetHra.HracVyhral(dispecerHry.GetHracNaTahu))
            {
                SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
                ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);
                mujTahZeZelenehoDoPrazdneho.Clear();
                hlaseniTextBlock.Text = ""; 

                Vitezstvi oknoVitezstvi = new Vitezstvi();
                Image bilaF = new Image();
                bilaF.Source = new BitmapImage(new Uri("Ikonky/Pawn-whitePNG-Clear.png", UriKind.Relative));
                Image cernaF = new Image();
                cernaF.Source = new BitmapImage(new Uri("Ikonky/Pawn-blackPNG-Clear.png", UriKind.Relative));                

                if (dispecerHry.GetHracNaTahu == dispecerHry.GetHrac1)
                {                    
                    oknoVitezstvi.vitezstviImage.Source = bilaF.Source;
                    if (dispecerHry.GetHrac1.GetJmeno == "" || dispecerHry.GetHrac1.GetJmeno == " ")
                        oknoVitezstvi.kdoVyhralTextBlock.Text = "Hráč 1";
                    else
                        oknoVitezstvi.kdoVyhralTextBlock.Text = dispecerHry.GetHrac1.GetJmeno;
                    
                    oknoVitezstvi.textORemizeTextBlock.Text = "";
                }
                else
                {                    
                    oknoVitezstvi.vitezstviImage.Source = cernaF.Source;
                    if (dispecerHry.GetHrac2.GetJmeno == "" || dispecerHry.GetHrac2.GetJmeno == " ")
                        oknoVitezstvi.kdoVyhralTextBlock.Text = "Hráč 2";
                    else
                        oknoVitezstvi.kdoVyhralTextBlock.Text = dispecerHry.GetHrac2.GetJmeno;
                   
                    oknoVitezstvi.textORemizeTextBlock.Text = "";
                }

                posledni += " Vyhrál " + oknoVitezstvi.kdoVyhralTextBlock.Text;
                historieTahu.GetTahyVypsaneDetailne.RemoveAt(indexPosledniho);
                historieTahu.GetTahyVypsaneDetailne.Add(posledni);
                historieTahuListBox.SelectedIndex = historieTahuListBox.Items.Count - 1;
                historieTahuListBox.ScrollIntoView(historieTahuListBox.SelectedItem);
                var listBoxItem = (ListBoxItem)historieTahuListBox.ItemContainerGenerator.ContainerFromItem(historieTahuListBox.SelectedItem);
                if (listBoxItem != null)
                    listBoxItem.Focus();  

                oknoVitezstvi.ShowDialog();                          
                DotazNaUlozeniHryPoVyhre(); 
            }                

            // podminka na remizu splnena
            if (dispecerHry.GetHra.JeRemiza())
            {
                SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
                ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);
                mujTahZeZelenehoDoPrazdneho.Clear();
                hlaseniTextBlock.Text = "";                

                posledni += " Remíza !";
                historieTahu.GetTahyVypsaneDetailne.RemoveAt(indexPosledniho);
                historieTahu.GetTahyVypsaneDetailne.Add(posledni);
                historieTahuListBox.SelectedIndex = historieTahuListBox.Items.Count - 1;
                historieTahuListBox.ScrollIntoView(historieTahuListBox.SelectedItem);
                var listBoxItem = (ListBoxItem)historieTahuListBox.ItemContainerGenerator.ContainerFromItem(historieTahuListBox.SelectedItem);
                if (listBoxItem != null)
                    listBoxItem.Focus();
                hlaseniTextBlock.Text = "Konec partie hry. Je to Remíza.";                

                Vitezstvi oknoVitezstvi = new Vitezstvi();
                Image remiza30 = new Image();
                remiza30.Source = new BitmapImage(new Uri("Ikonky/30modre.png", UriKind.Relative));
                oknoVitezstvi.vitezstviImage.Source = remiza30.Source;
                oknoVitezstvi.hlavniMrizGrid.Background = null;
                oknoVitezstvi.Icon = remiza30.Source;          
                oknoVitezstvi.uvodKdoVyhralTextBlock.Text = "Je to ";
                oknoVitezstvi.kdoVyhralTextBlock.Text = "Remíza !";
                oknoVitezstvi.textORemizeTextBlock.Text = "Bylo dosaženo 30-ti tahů bez přeskoku";
                oknoVitezstvi.ShowDialog();               
                DotazNaUlozeniHryPoVyhre();
            }
        }

        // pomocna otazka na konec hry:
        private bool KonecHry()
        {
            if (dispecerHry.GetHra.HracVyhral(dispecerHry.GetHracNaTahu) 
                || dispecerHry.GetHra.JeRemiza())                                         
                return true;                        
            else
                return false;
        }
        

        // Nastaveni GUI do startovnich barev a textu:
        private void InicializaceHlavnihoOknadoPocatecnihoStavu()
        {
            hrac1GroupBox.BorderBrush = Brushes.Red;
            hrac2GroupBox.BorderBrush = Brushes.LightGray;
            cisloHraceTextBlock.Text = "1";
            pocetTahuBPTextBlock.Text = "0";
            hlaseniTextBlock.Text = "";

            foreach (UIElement elem in MrizHlavni.Children)
            {
                elem.Refresh();
            }
        }
                
       
        /************************************ OBSLUBA BACKGROUND WORKERU **************************************************************/
        
        // bw pro pocitac:
        public void bw_DoWork(object sender, DoWorkEventArgs e)
        {            
            if (GetZpomalenVypocetProPCHrace1 && dispecerHry.GetHracNaTahu.Equals(dispecerHry.GetHrac1))
                dispecerHry.SetZpomaleni = true;
            else
            {
                dispecerHry.SetZpomaleni = false;
                if (GetZpomalenVypocetProPCHrace2 && dispecerHry.GetHracNaTahu.Equals(dispecerHry.GetHrac2))
                {
                    dispecerHry.SetZpomaleni = true;
                }
                else dispecerHry.SetZpomaleni = false;
            }

            BackgroundWorker worker = sender as BackgroundWorker;
            tah = dispecerHry.GetGenerator.GenerujNejlepsiTah(dispecerHry.GetDeska, dispecerHry.VratProtihrace(dispecerHry.GetHracNaTahu).GetObtiznost, dispecerHry.GetHracNaTahu);
            
            if (bw.CancellationPending == true)
            {
                bw.ReportProgress(0);
                e.Cancel = true;
                return;
            }
            else
                e.Result = tah;
        }


        // bw nej pro cloveka:
        public void bwNapoveda_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            tah = dispecerHry.GetGenerator.GenerujNejlepsiTah(dispecerHry.GetDeska, 3, dispecerHry.GetHracNaTahu);
            if (bwNapoveda.CancellationPending == true)
            {
                bwNapoveda.ReportProgress(0);
                e.Cancel = true;                
                return;
            }
            else
                e.Result = tah;
        }
        

        // Nejdulezitejsi na testovani Backgroundworkeru zda-li uz skoncil:
        public void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        { 
            if ((e.Cancelled == true))
            {   
                hlaseniTextBlock.Text = "Pozastaveno ! Pro výpočet tahu stiskni Pokračovat";               
                if (historieTahuListBox.Items.Count >= 1)
                    historieTahuListBox_SelectionChanged(null, null);               
                SetZrusVypocet = false;
            }

            else if (!(e.Error == null))
            {
                hlaseniTextBlock.Text = ("Error: " + e.Error.Message);
            }            

            else
            {                   
                tah = (Tah)e.Result;
                vypocetTextBlock.Text = "";
                mujProgressBar.Value = 0;
                
                if (!GetZrusVypocet)
                deskaCopy = dispecerHry.GetDeska.ZkusZahratTah(tah);           

                if (!KonecHry())
                {
                    if (deskaCopy != null)
                    {
                        dispecerHry.SetDeska = deskaCopy;
                        VykresliDesku();
                        ProvedVedlejsiEfektyPoZahraniTahu(tah); // Info do okenka hlaseniTextBlock                   

                        if (!KonecHry())
                        {
                            dispecerHry.ProhodHraceNaTahu();
                            if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                            {
                                HrejZaPocitac();
                            }
                        }
                        else
                        {
                            AktualizujKamenyVyhruNeboRemizu();
                            dispecerHry.ProhodHraceNaTahu();
                            if (!prohlizetUkoncenouHru)
                                NastavDoPocatecnihoStavu();
                            return;
                        }
                    }
                }
            }
        }


        //*****************************************PROGRESS BAR ***********************************************************//

        // pro bw
        public void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            vypocetTextBlock.Text = (e.ProgressPercentage.ToString() + "%");
            mujProgressBar.Value = e.ProgressPercentage; 
        }

        // pro bwNapoveda
        public void bwNapoveda_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            vypocetTextBlock.Text = (e.ProgressPercentage.ToString() + "%");
            mujProgressBar.Value = e.ProgressPercentage;
        }


                
        /*************************************************** NEJLEPSI TAH **************************************************************/
     
        // Tady si dam metodu pro vypocet tahu na pozadi pro bw:
        public void NastartujVypocetTahuNaPozadi()
        {
            if (bw.IsBusy != true)
            {
                bw.RunWorkerAsync(); // to nejdulezitejsi
            }
        }

        // pro bwNapoveda
        public void NastartujVypocetNejlepsihoTahuNaPozadi()
        {
            if (bwNapoveda.IsBusy == false)
            {
                bwNapoveda.RunWorkerAsync();
            }
        }


        /*************************************************** TLACITKO HRAJ **************************************************************/ 
               
        public void tlacitkoHraj_Click(object sender, RoutedEventArgs e)
        {
            if (jmenoH1.Text == "")
                return;
            
            if (hraSkoncila)
                return;
            

            hlaseniTextBlock.Text = "";
            druheHlaseniTextBlock.Text = "";
            hlaseniTextBlock.Refresh();
            druheHlaseniTextBlock.Refresh();
            if (dispecerHry.GetZahajenaHRa)
            {
                SetZrusVypocet = false;
                hlaseniTextBlock.Text = "";

                if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                {
                    KontrolaTahuVListboxu();
                    NastartujVypocetTahuNaPozadi();
                }
            } 
        }

        /*************************************************** TLACITKO PAUSE **************************************************************/ 
               
        public void tlacitkoPause_Click(object sender, RoutedEventArgs e)
        {
            if (dispecerHry.GetZahajenaHRa)
            {
                SetZrusVypocet = true;                
            }
        }

    
       /************************* sereializace a deserializace ***********************************************************************/
    
        private void tlacitkoUlozit_Click(object sender, RoutedEventArgs e)
        {
            if (!dispecerHry.GetZahajenaHRa)
                return;
            if (typHrace1.Text == "")
                return;

            // presmerovani na jinou udalost - kdyz mam zatim prazdny soubor;
            if (souborProUkladani == "")
            {
                tlacitkoUlozitJako_Click(sender, e);
            }

            else
            {
                SetZrusVypocet = true;
                dispecerHry.SetHistorieTahu = historieTahu;
                UlozInfoOHracich();
                SerializujBinarne(souborProUkladani);
            }
        }

        
        // Ukladani dispecera
        private void tlacitkoUlozitJako_Click(object sender, RoutedEventArgs e)
        {
            if (!dispecerHry.GetZahajenaHRa)
                return;
            if (typHrace1.Text == "")
                return;

            SetZrusVypocet = true;
            dispecerHry.SetHistorieTahu = historieTahu;
            UlozInfoOHracich();

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "Bunnywar1";
            dlg.DefaultExt = ".bin";
            dlg.Filter = "Binnary File (.bin)|*.bin";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                souborProUkladani = filename;
                SerializujBinarne(filename);
            } 
        }

        // Otevreni ulozeneho dispecera
        private void tlacitkoOtevrit_Click(object sender, RoutedEventArgs e)
        {
            if (dispecerHry.GetZahajenaHRa)
                SetZrusVypocet = true;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".bin";
            dlg.Filter = "Bin File (.bin)|*.bin";
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;              
                DeserializujBinarne(filename);

                if (bylaChybaIO == false)
                {
                    souborProUkladani = filename;
                    // tahyCopy je dulezita promenna:
                    tahyCopy.Clear();
                    tahyCopy.AddRange(historieTahu.cistySeznamOdehranychTahu);
                    NactiUlozenaInfoOHracich();
                    historieTahuListBox.SelectedIndex = historieTahuListBox.Items.Count - 1;
                    historieTahuListBox.ScrollIntoView(historieTahuListBox.SelectedItem);

                    var listBoxItem = (ListBoxItem)historieTahuListBox.ItemContainerGenerator.ContainerFromItem(historieTahuListBox.SelectedItem);
                    if( listBoxItem != null)
                        listBoxItem.Focus();

                    if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                    {   
                        hlaseniTextBlock.Text = "Na tahu je počítačový hráč - stiskni tlačítko Pokračovat";
                    }
                    else
                    {
                        hlaseniTextBlock.Text = "";
                    }

                    string nazevS = System.IO.Path.GetFileName(otevreniSouboruSePodarilo);
                    druheHlaseniTextBlock.Text = "Otevřeno: " + nazevS;                    
                    
                }
                else
                    return;
            }
            else
                return;
        }


        // Maze se mi pozadi policek, historie tahu, nuluji se tahy - vse radeji pro jistotu:
        private void NastavDoMezistavuPoOtevreniSouboru()
        {
            dispecerHry.GetDeska.NastavStartovniKameny();         
            SmazDeskuCislemHraceNula(seznamUzivPrvkuPolicko);
            SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
            ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);          
            GetDispecerHry().GetHra.VynulujPocitadloTahuBezPreskoku();
            pocetTahuBPTextBlock.Text = "0";
            GetDispecerHry().GetHra.VynulujTahy();           
            historieTahu.GetTahyVypsaneDetailne.Clear();
            historieTahu.GetCistySeznamOdehranychTahu.Clear();
            tlacitkoPause_Click(null, null);         
            VykresliDesku(); 
        }

        
        // Funkcni pokus na serializaci binarne srozumitelneji:
        public void SerializujBinarne(string jmenoSouboru)
        {
            FileStream fs = new FileStream(jmenoSouboru, FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, GetDispecerHry());
                string nazevS = System.IO.Path.GetFileName(jmenoSouboru);
                druheHlaseniTextBlock.Text = "Uloženo do: "+nazevS;
                bylaChybaIO = false;

            }
            catch (Exception ex)
            {
                druheHlaseniTextBlock.Text = "";
                string zprava = "Došlo k chybě při ukládání souboru:" + Environment.NewLine +
                                jmenoSouboru + Environment.NewLine +
                                "Soubor nebyl uložen !";
                string nadpis = "Chyba při ukládání souboru";
                MessageBoxButton tlacitka = MessageBoxButton.OK;
                MessageBoxImage ikona = MessageBoxImage.Exclamation;
                MessageBox.Show(zprava, nadpis, tlacitka, ikona);
                bylaChybaIO = true;
                return;
            }
            finally
            {
                fs.Close();
            }
        }


        // Deserializace - znovunacteni Dispecera :
        public void DeserializujBinarne(string jmenoSouboru)
        {
            FileStream fs = new FileStream(jmenoSouboru, FileMode.Open);
            DispecerHry disp = new DispecerHry();
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                disp = (DispecerHry)formatter.Deserialize(fs);
                otevreniSouboruSePodarilo = jmenoSouboru;
                bylaChybaIO = false;
                NastavDoMezistavuPoOtevreniSouboru();

                //nastavuju si nacteneho dispecera a prekresluju okno:
                dispecerHry = disp;
                otevreniSouboruSePodarilo = jmenoSouboru;                

                // tady jsem si nacetl historii tahu z ulozeneho dispecera !!! a priradil ji opetovne do ListBox.ItemsSource:
                historieTahu = disp.GetHistorieTahu;
                historieTahuListBox.ItemsSource = historieTahu.GetTahyVypsaneDetailne;
                
                VykresliDesku();

                foreach (UIElement elem in MrizHlavni.Children)
                {
                    elem.Refresh();
                }
            }
            catch (SerializationException e)
            {               
                hlaseniTextBlock.Text = "";
                druheHlaseniTextBlock.Text = "";
                string zprava = "Došlo k chybě při otevírání souboru:" + Environment.NewLine +
                                jmenoSouboru + Environment.NewLine +
                                "Soubor nebyl načten !";
                string nadpis = "Chyba při otevírání souboru";
                MessageBoxButton tlacitka = MessageBoxButton.OK;
                MessageBoxImage ikona = MessageBoxImage.Exclamation;
                MessageBox.Show(zprava, nadpis, tlacitka, ikona);
                bylaChybaIO = true;

                return;             
            }
            finally
            {
                fs.Close();
            }
        }



        // Nacte ulozene info o hracich z binarni instance dispecera (po stisku tlacitka otevrit) - po deserializaci:
        private void NactiUlozenaInfoOHracich()
        {
            jmenoH1.DataContext = new TextBoxText() { NastavText = dispecerHry.jmH1CvDispecerovi };
            jmenoH2.DataContext = new TextBoxText() { NastavText = dispecerHry.jmH2vDispecerovi };
            typHrace1.DataContext = new TextBoxText() { NastavText = dispecerHry.typHrace1vDispecerovi };
            typHrace2.DataContext = new TextBoxText() { NastavText = dispecerHry.typHrace2vDispecerovi };
            obtiznost1.DataContext = new TextBoxText() { NastavText = dispecerHry.obtiznostHrace1vDispecerovi };
            obtiznost2.DataContext = new TextBoxText() { NastavText = dispecerHry.obtiznostHrace2vDispecerovi };
            zpomalenVypocetProPCHrace1 = dispecerHry.zpomaleniProPCH1;
            zpomalenVypocetProPCHrace2 = dispecerHry.zpomaleniProPCH2;             
        
            if (dispecerHry.GetHracNaTahu == dispecerHry.GetHrac1)
            {
                cisloHraceTextBlock.Text = "1";
                hrac1GroupBox.BorderBrush = Brushes.Red;
                hrac2GroupBox.BorderBrush = Brushes.LightGray;
            }

            else
            {
                cisloHraceTextBlock.Text = "2";
                hrac2GroupBox.BorderBrush = Brushes.Red;
                hrac1GroupBox.BorderBrush = Brushes.LightGray;
            }  
        }


        // po stisku tl. Ulozit - ukladam informace o hracich do dispecera, ktery se serializuje-uklada  na disk:
        public void UlozInfoOHracich()
        {
            dispecerHry.jmH1CvDispecerovi = jmenoH1.Text;
            dispecerHry.jmH2vDispecerovi = jmenoH2.Text;
            dispecerHry.typHrace1vDispecerovi = typHrace1.Text;
            dispecerHry.typHrace2vDispecerovi = typHrace2.Text;
            dispecerHry.obtiznostHrace1vDispecerovi = obtiznost1.Text;
            dispecerHry.obtiznostHrace2vDispecerovi = obtiznost2.Text;
            dispecerHry.zpomaleniProPCH1 = zpomalenVypocetProPCHrace1;
            dispecerHry.zpomaleniProPCH2 = zpomalenVypocetProPCHrace2;
        }

        // Tahy zpet - dopredu:
        public void ZahrajTahVpred()
        {
            int k = dispecerHry.GetHra.GetTahy.Count();
            Tah t = tahyCopy.ElementAt(k);
            dispecerHry.ProvedPosunutiTahuVpred(t);
            VykresliDesku();

            ProvedVedlejsiEfektPoAkciZpetDopredu();

            // dulezite - nasatvuju si focus klavesnice - up/down
            var listBoxItem = (ListBoxItem)historieTahuListBox.ItemContainerGenerator.ContainerFromItem(historieTahuListBox.SelectedItem);
            if (listBoxItem != null)
                listBoxItem.Focus();
            druheHlaseniTextBlock.Text = "";

            int posledni = historieTahuListBox.Items.Count - 1;
            int pocetKamenuBileho = dispecerHry.GetHrac1.VratPocetKamenu();
            int pocetKamenuCerneho = dispecerHry.GetHrac2.VratPocetKamenu();
            if (historieTahuListBox.SelectedItem == historieTahuListBox.Items.GetItemAt(posledni))
            {
                if (pocetKamenuBileho <=1 || pocetKamenuCerneho<=1 || 
                    historieTahu.GetTahyVypsaneDetailne.ElementAt(posledni).Contains("Remíza !"))
                {
                    string kdoVyhral;                    
                    if (historieTahu.GetTahyVypsaneDetailne.ElementAt(posledni).Contains("Remíza !"))
                    {
                        hlaseniTextBlock.Text = "Konec partie hry. Je to Remíza";
                    }                    
                    else if (historieTahu.GetTahyVypsaneDetailne.ElementAt(posledni).Contains(dispecerHry.GetHrac2.GetJmeno))
                        {
                            kdoVyhral = dispecerHry.GetHrac2.GetJmeno;
                            hlaseniTextBlock.Text = "Konec partie hry. Vyhrál " + kdoVyhral;
                        }
                    else if (historieTahu.GetTahyVypsaneDetailne.ElementAt(posledni).Contains(dispecerHry.GetHrac1.GetJmeno))
                        {
                            kdoVyhral = dispecerHry.GetHrac1.GetJmeno;
                            hlaseniTextBlock.Text = "Konec partie hry. Vyhrál " + kdoVyhral;
                        }
                    hraSkoncila = true;
                } 
                ProhodHraceNaTahu();              
                return;
            }
            else
            {
                ProhodHraceNaTahu();
                if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                    hlaseniTextBlock.Text = "Na tahu je počítačový hráč, stiskni tlačítko Pokračovat";
                else
                    hlaseniTextBlock.Text = "";
            }
        }


        public void ZahrajTahZpet()
        {
            dispecerHry.ProvedVraceniTahuZpet();
            VykresliDesku();

            ProvedVedlejsiEfektPoAkciZpetDopredu();

            // dulezite - nasatvuju si focus klavesnice - up/down
            var listBoxItem = (ListBoxItem)historieTahuListBox.ItemContainerGenerator.ContainerFromItem(historieTahuListBox.SelectedItem);
            if (listBoxItem != null)
                listBoxItem.Focus();
            druheHlaseniTextBlock.Text = "";

            ProhodHraceNaTahu();
            if (dispecerHry.GetHracNaTahu.GetJePocitacovyHrac)
                hlaseniTextBlock.Text = "Na tahu je počítačový hráč, stiskni tlačítko Pokračovat";
            else
                hlaseniTextBlock.Text = "";
        }


        // pouzivam pro vymenu kamenu - swap na desce:
        public void VyrobNovouHistorii()
        {
            HistorieTahu hist = new HistorieTahu();
            List<Tah> tahyZDispecera = new List<Tah>();

            hist.GetCistySeznamOdehranychTahu.Clear();
            hist.GetTahyVypsaneDetailne.Clear();
            tahyZDispecera = dispecerHry.GetHra.GetTahy;
            bool kopie = false;
            foreach (Tah tah in tahyZDispecera)
            {
                hist.PridejDoHistorie(tah, kopie);
                kopie = !kopie;
            }
            historieTahu = hist;
            historieTahuListBox.ItemsSource = historieTahu.GetTahyVypsaneDetailne;
            historieTahuListBox.SelectedIndex = historieTahuListBox.Items.Count - 1;
            historieTahuListBox.ScrollIntoView(historieTahuListBox.SelectedItem);
        }


    

        // Dulezite!!, historieTahuListBox a vsechny jeho zmeny uvnitr co se neustale deji :
        private void historieTahuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)            
        {
            if (historieTahuListBox.Items.Count < dispecerHry.GetHra.GetTahy.Count)
                return;

            int pocetKamenuBileho = dispecerHry.GetHrac1.VratPocetKamenu();
            int pocetKamenuCerneho = dispecerHry.GetHrac2.VratPocetKamenu();
            int posledni = historieTahuListBox.Items.Count - 1;
           
            if ((pocetKamenuBileho <= 1 || pocetKamenuCerneho <= 1) &&
                historieTahuListBox.SelectedItem == historieTahuListBox.Items.GetItemAt(posledni))
            {
                hraSkoncila = true;
                string kdoVyhral; ;
                if (historieTahu.GetTahyVypsaneDetailne.ElementAt(posledni).Contains("Remíza !"))
                {
                    hlaseniTextBlock.Text = "Konec partie hry. Je to Remíza";
                }
                else if (historieTahu.GetTahyVypsaneDetailne.ElementAt(posledni).Contains(dispecerHry.GetHrac2.GetJmeno))
                    {
                        kdoVyhral = dispecerHry.GetHrac2.GetJmeno;
                        hlaseniTextBlock.Text = "Konec partie hry. Vyhrál " + kdoVyhral;
                    }
                else if (historieTahu.GetTahyVypsaneDetailne.ElementAt(posledni).Contains(dispecerHry.GetHrac1.GetJmeno))
                    {
                        kdoVyhral = dispecerHry.GetHrac1.GetJmeno;
                        hlaseniTextBlock.Text = "Konec partie hry. Vyhrál " + kdoVyhral;
                    }
            }                
            else
            {
                hraSkoncila = false;
            } 

            if (!bw.IsBusy && !bwNapoveda.IsBusy)
            {                
                int si = historieTahuListBox.SelectedIndex;               
                int ri = dispecerHry.GetHra.GetTahy.Count() - 1;

                if (si <= ri)
                { 
                    for (int i = ri - si; i > 0; i--)
                    {
                        ZahrajTahZpet();                       
                    }
                }
                else
                {
                  
                    for (int i = si - ri; i > 0; i--)
                    {
                        ZahrajTahVpred();                       
                    }
                }
            }
            else
            {
                tlacitkoPause_Click(sender, e);               
                historieTahuListBox.ScrollIntoView(historieTahuListBox.SelectedItem); 
                SetZrusVypocet = true;
                historieTahuListBox.Refresh();  
            }
        }



        // tlačítko UNDO/REDO
        private void tlacitkoUndo_Click(object sender, RoutedEventArgs e)
        {
            if (!bw.IsBusy && !bwNapoveda.IsBusy)
            {
                SetZrusVypocet = true;
                if (historieTahuListBox.SelectedIndex > -1)
                {
                    historieTahuListBox.SelectedIndex = historieTahuListBox.SelectedIndex - 1;
                    historieTahuListBox.ScrollIntoView(historieTahuListBox.SelectedItem);                    
                }
            }
            else
            {               
                tlacitkoPause_Click(sender, e);
            }
        }


        private void tlacitkoRedo_Click(object sender, RoutedEventArgs e)
        {  
            SetZrusVypocet = true;            
            int indexPosledniho = historieTahuListBox.Items.Count;
            if (historieTahuListBox.SelectedIndex < indexPosledniho)
            {
                historieTahuListBox.SelectedIndex = historieTahuListBox.SelectedIndex + 1;
                historieTahuListBox.ScrollIntoView(historieTahuListBox.SelectedItem);
            } 
        }
        
      
        /*****************************************VEDLEJSI EFEKTY**********************************************************************/
       
        // Tady se mi budou provadet vedlejsi ukony v graficke podobe, v List.Boxu

        internal void ProvedVedlejsiEfektyPoZahraniTahu(Tah tah)
        {
            List<Tah> seznamTahu = new List<Tah>();
            seznamTahu = dispecerHry.GetHra.GetTahy;
            seznamTahu.Add(tah);            
            SetZrusVypocet = false;
            mujTah = "";

            historieTahu.PridejDoHistorie(tah, false);

            // tady toto kvuli akci zpet a dopredu:
            tahyCopy.Clear();
            tahyCopy.AddRange(historieTahu.cistySeznamOdehranychTahu);

            historieTahuListBox.SelectedIndex = historieTahuListBox.Items.Count - 1;
            historieTahuListBox.ScrollIntoView(historieTahuListBox.SelectedItem);            
            historieTahuListBox.Refresh();
            
            /// chci updatovat policko kdo hraje:
            if (dispecerHry.GetHracNaTahu == dispecerHry.GetHrac1)
            {
                cisloHraceTextBlock.Text = "2";
                hrac2GroupBox.BorderBrush = Brushes.Red;
                hrac1GroupBox.BorderBrush = Brushes.LightGray;
            }

            else
            {
                cisloHraceTextBlock.Text = "1";
                hrac1GroupBox.BorderBrush = Brushes.Red;
                hrac2GroupBox.BorderBrush = Brushes.LightGray;
            }

            cisloHraceTextBlock.Refresh();

            if (tah.GetPreskoceneKameny.Count() == 0)
            {
                dispecerHry.GetHra.PridejTahBezPreskoku();
                pocetTahuBPTextBlock.Text = dispecerHry.GetHra.GetPocetTahuBezPreskoku.ToString();
                pocetTahuBPTextBlock.Refresh();
            }
            else
            {             
                pocetTahuBPTextBlock.Text = "0";
                dispecerHry.GetHra.VynulujPocitadloTahuBezPreskoku();
                zbyvaKamenuH1.Text = dispecerHry.GetHrac1.VratPocetKamenu().ToString(); // aktualizuje se mi pocet kamenu v okenkach
                zbyvaKamenuH2.Text = dispecerHry.GetHrac2.VratPocetKamenu().ToString();
            }
        }
        
        

        // refresh okennIch prvku po akci zpet - dopredu:
        public void ProvedVedlejsiEfektPoAkciZpetDopredu()
        {
            if (dispecerHry.GetHracNaTahu == dispecerHry.GetHrac1)
            {
                cisloHraceTextBlock.Text = "2";
                hrac2GroupBox.BorderBrush = Brushes.Red;
                hrac1GroupBox.BorderBrush = Brushes.LightGray;
            }
            else
            {
                cisloHraceTextBlock.Text = "1";
                hrac1GroupBox.BorderBrush = Brushes.Red;
                hrac2GroupBox.BorderBrush = Brushes.LightGray;
            }

            pocetTahuBPTextBlock.Text = dispecerHry.GetHra.GetPocetTahuBezPreskoku.ToString();
            zbyvaKamenuH1.Text = dispecerHry.GetHrac1.VratPocetKamenu().ToString(); 
            zbyvaKamenuH2.Text = dispecerHry.GetHrac2.VratPocetKamenu().ToString();

            // musim pripadne smazat pozadi policek:
            SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
            ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);

            foreach (UIElement elem in MrizHlavni.Children)
            {
                elem.Refresh();
            }
        }


        
        // Prohozeni stran hracum
        private void menuProhoditStrany_Click(object sender, RoutedEventArgs e)
        {
            if (!dispecerHry.GetZahajenaHRa || typHrace1.Text == "")
            {
                dispecerHry.ProvedZmenuHracu();                
                mujTahZeZelenehoDoPrazdneho.Clear();
                SmazZeleneOznaceniSeznamuPolicek(seznamUzivPrvkuPolicko);
                ObnovMiBarvuPozadi(seznamUzivPrvkuPolicko);
                VykresliDesku();
                stranyProhozeny = !stranyProhozeny;
            }
            else
                return;           
        }

        

        // O aplikaci okno
        private void menuOApp_Click(object sender, RoutedEventArgs e)
        {
            Oaplikaci nabidka = new Oaplikaci();
            nabidka.ShowDialog();
        }        


        // Zakladni F1 help aplikace + osetreni vsech stavu co me napadly
        private void menuHelp_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Bunnyhelp.chm")))
            {
                hlaseniTextBlock.Text = "Soubor nápovědy nebyl nalezen !";
                return;
            }
            else
            {
                string cesta;
                cesta = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "Bunnyhelp.chm");

                if (procesy.Count() == 0)
                {  
                    try
                    {
                        ProcessStartInfo p = new ProcessStartInfo();
                        p.FileName = cesta;
                        p.WindowStyle = ProcessWindowStyle.Normal;                        
                        procesUkazCHMsoubor = Process.Start(p);                          
                        procesy.Add(procesUkazCHMsoubor);
                    }
                    catch(Exception ex)
                    {                       
                        string zprava = "Nepovedlo se otevřít soubor nápovědy" + Environment.NewLine + "Nastala chyba:"
                            + Environment.NewLine + ex.Message;
                        string nadpis = "Chyba při otevírání nápovědy";
                        MessageBoxButton tlacitka = MessageBoxButton.OK;
                        MessageBoxImage ikona = MessageBoxImage.Information;
                        MessageBox.Show(zprava, nadpis, tlacitka, ikona);                                          
                    } 
                }
                else
                {
                    if (procesy.ElementAt(0).HasExited)
                    {
                        procesy.ElementAt(0).Start();
                    }
                    else
                    {
                        return; 
                    }
                }
            }
        }
        
        
        // zaviram napovedu na konci programu:
        private void ZavriOknoNapovedy()
        {
            if (procesy == null)
                return;

            if (procesy.Count() > 0)
            {
                foreach (Process proc in procesy)
                {
                    if (!proc.HasExited)
                    {                        
                        proc.CloseMainWindow();                        
                        proc.Kill();                        
                    }                    
                }
            }
            else
                return;           
        }


        // Zakladni KeyDown na hlavni okno:
        private void hlavniOkno_KeyDown(object sender, KeyEventArgs e)
        { 
            switch(e.Key)
            {
                case (Key.Space):
                    tlacitkoPause_Click(null, null);
                    break;
                case (Key.F1):
                    menuHelp_Click(null, null);
                    break;
                case (Key.F12):
                    tlacitkoUlozitJako_Click(null, null);
                    break;
            }

            if (e.Key == Key.N && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoNovaHra_Click(null, null);
            else
                if (e.Key == Key.O && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoOtevrit_Click(null, null);
            else
                if (e.Key == Key.S && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoUlozit_Click(null, null);
            else
                 if (e.Key == Key.O && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoOtevrit_Click(null, null);
            else
                if (e.Key == Key.F4 && e.KeyboardDevice.Modifiers == ModifierKeys.Alt)
                Close();
            else
                if (e.Key == Key.O && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoOtevrit_Click(null, null);
            else
                if (e.Key == Key.P && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoHraj_Click(null, null);
            else
                if (e.Key == Key.K && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoStopPartie_Click(null, null);
            else
                if (e.Key == Key.H && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoNastaveni_Click(null, null);
            else
                if (e.Key == Key.W && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                menuProhoditStrany_Click(null, null);
            else
                if (e.Key == Key.B && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoNejlepsiTah_Click(null, null);
            else
                if (e.Key == Key.U && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoUndo_Click(null, null);
            else
                if (e.Key == Key.R && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                tlacitkoRedo_Click(null, null);
            else
                  return;
        }
    }
}

