using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Bunny_GUI_1
{   
    [Serializable]
    public class HistorieTahu : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private DispecerHry dispecer2;

        // promenne tridy:
        public ObservableCollection<Tah> cistySeznamOdehranychTahu;       
        public ObservableCollection<string> tahyVypsaneDetailne;
        public int pocetTahuCelkem;        

        // vlastnosti:        
        public  ObservableCollection<Tah> GetCistySeznamOdehranychTahu
        {
            get { return cistySeznamOdehranychTahu; }
        }

        public  ObservableCollection<Tah> SetCistySeznamOdehranychTahu
        {
            set { cistySeznamOdehranychTahu = value; }
        }


        // metody:
        public int GetPocetTahuCelkem
        {
            get { return pocetTahuCelkem; }
        }

        public int SetPocetTahuCelkem
        {
            set { pocetTahuCelkem = value; }
        }
        

        public ObservableCollection<string> GetTahyVypsaneDetailne
        {
            get { return tahyVypsaneDetailne; }
        }

        public ObservableCollection<string> SetTahyVypsaneDetilane
        {
            set { tahyVypsaneDetailne = value; }
        }

        

        // konstruktor tridy:
        public HistorieTahu()
        {
            cistySeznamOdehranychTahu = new ObservableCollection<Tah>();
            tahyVypsaneDetailne = new ObservableCollection<string>(); 
        }


        // to je z knizky:
        protected void VyvolejZmenu(string vlastnost)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(vlastnost));
        }
                

        public void PridejDoHistorie(Tah tah, bool kopie) 
        {            
            ObservableCollection<string> tahy = new ObservableCollection<string>();
            ObservableCollection<string> stavajiciTahy = new ObservableCollection<string>();
            dispecer2 = MainWindow.GetDispecerHry();

            // Vypis preskocenych kamenu:
            string preskoceneKameny = "";
            foreach (Pozice pozice in tah.GetPreskoceneKameny)
            {
                preskoceneKameny += pozice.VratJakoText();
            }

            // muj oficialni seznam tahu v Historii tahu tvoreny ciste tridami Tah
            cistySeznamOdehranychTahu.Add(tah);
            SetPocetTahuCelkem = cistySeznamOdehranychTahu.Count();

            // tady v historii budu ukladat pocet tahu bez preskoku:
            int pocetBP = dispecer2.GetHra.GetPocetTahuBezPreskoku;
            
            int aktualniIndexTahu = cistySeznamOdehranychTahu.Count();
            string jmenoHrace;
            Hrac kdoJeNaTahu = dispecer2.GetHracNaTahu;
            Hrac protiHrac = dispecer2.VratProtihrace(kdoJeNaTahu);

            if (kopie == true)
            {
                jmenoHrace = protiHrac.GetJmeno;
            }
            else
            {
                jmenoHrace = kdoJeNaTahu.GetJmeno;

            }
            string jedenTah = aktualniIndexTahu + ".tah " + jmenoHrace + " " + tah.ToString();
            if (preskoceneKameny != "")
            {
                int pulka = preskoceneKameny.Length / 2;
                if (dispecer2.GetHracNaTahu.GetJePocitacovyHrac)
                {
                    preskoceneKameny = preskoceneKameny.Substring(0, pulka);
                }
                jedenTah += " odstraněno " + preskoceneKameny.ToUpper();
            }           
            
            tahy = GetTahyVypsaneDetailne;
            tahy.Add(jedenTah);            
            SetTahyVypsaneDetilane = tahy;
            VyvolejZmenu("TahyDoH");
            VyvolejZmenu("GetTahyVypsaneDetailne");
        }
                

        public void OdeberZHistorie(string tahTextove, Tah tah)
        { 
            ObservableCollection<string> tahyZHistorieTahu = new ObservableCollection<string>();
            tahyZHistorieTahu = GetTahyVypsaneDetailne;

            if (tahyZHistorieTahu.Contains(tahTextove))
            {
                tahyZHistorieTahu.Remove(tahTextove);
                cistySeznamOdehranychTahu.Remove(tah); 
            } 
        }
    }
}
