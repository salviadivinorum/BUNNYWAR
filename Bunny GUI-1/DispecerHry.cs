using System;
using System.Linq;

namespace Bunny_GUI_1
{
    [Serializable]
    public class DispecerHry // odsud probiha rizeni cele hry
    {
        bool zpomaleni = false;
        bool zahajenaHra = false;
        private Hrac hrac1;
        private Hrac hrac2;
        Hra hra;
        GeneratorTahu generator;
        Deska deska;
        Hrac hracNaTahu;
        HistorieTahu historie; 
        public string typHrace1vDispecerovi;
        public string typHrace2vDispecerovi;
        public string jmH1CvDispecerovi, jmH2vDispecerovi;
        public string obtiznostHrace1vDispecerovi;
        public string obtiznostHrace2vDispecerovi;
        public string kdoHrajePolickovDispecerovi;
        public bool zpomaleniProPCH1;
        public bool zpomaleniProPCH2;
       
        // Konstruktor dispecera:
        public DispecerHry()
        {            
        }        

        //Vlastnosti:    

        // budu pouzivat pri ukoncovani/zahajovani hry a pro dalsi obsluhy hry
        public bool GetZpomaleni
        {
            get { return zpomaleni; }
        }

        public bool SetZpomaleni
        {
            set { zpomaleni = value; }
        }

        public bool GetZahajenaHRa
        {
            get { return zahajenaHra; }
        }

        public bool SetZahajenaHra
        {
            set { zahajenaHra = value; }
        }

        // Slot hrac1 a hrac2:
        public Hrac GetHrac1
        {
            get { return hrac1; }
        }

        public Hrac GetHrac2
        {
            get { return hrac2; }
        }

        // DispecerHry - ridi celou hru - vsechny tridy musi mit na nej odkaz
        public Hrac SetHrac1
        {
            set
            {
                hrac1 = value;
                hrac1.SetDispecerHry = this;  // nastavuju tohoto Dispecera pro Hráče č. 1
            }
        }

       
        public Hrac SetHrac2
        {
            set
            {
                hrac2 = value;
                hrac2.SetDispecerHry = this; // nastavuju tohoto Dispecera pro Hráče č. 2
            }
        }


        // Slot hra:
        public Hra GetHra
        {
            get { return hra; }
        }


        public Hra SetHra
        {
            set
            {
                hra = value;
                hra.SetDispecerHry = this; // nastavuju i Hře tohoto Dipsečera
            }
        }

        // Slot generator:
        public GeneratorTahu GetGenerator
        {
            get { return generator; }
        }

        public GeneratorTahu SetGenerator
        {
            set
            {
                generator = value;
                generator.SetDispecerHry = this; // nastavuju Generátorovi tahů tohoto Dispečera
            }
        }

        // Slot deska:
        public Deska GetDeska
        {
            get { return deska; }
        }

        public Deska SetDeska
        {
            set
            {
                deska = value;
                deska.SetDispecerHry = this; // nastuvuju i Desce tohoto Dispecera !
            }
        }

        // Slot hracNaTahu:
        public Hrac GetHracNaTahu
        {
            get { return hracNaTahu; }
        }

        public Hrac SetHracNaTahu
        {
            set { hracNaTahu = value; }
        }      

        // Tento getter a setter: kvuli historii tahu
        public HistorieTahu SetHistorieTahu
        {
            set { historie = value; }
        }

        public HistorieTahu GetHistorieTahu
        {
            get { return historie; }
        }
        

        /*************************************************************************************************/
        // HLAVNI CHOD DISPECERA ZDE:


        // Tato metoda volána z NastaveniHracuNabidka pro zahajeni hry:
        public void ZahajHru()
        {
            zahajenaHra = true;
            deska = MainWindow.VytvorDesku();
            deska.NastavStartovniKameny(); 
            hrac1.SetBarvuHrace = BarvaHrace.bila;
            hrac2.SetBarvuHrace = BarvaHrace.cerna;
            SetHracNaTahu = hrac1;                       
        }

        public void ProhodHraceNaTahu()
        {
            if (hracNaTahu.Equals(hrac1))
            {
                SetHracNaTahu = hrac2;
            }
            else
                SetHracNaTahu = hrac1;
        }


        // Kompletni zamena hracu
        public void ProvedZmenuHracu()
        {
            Deska newDeska = new Deska();
            for (int vyska = 0; vyska < 8; ++vyska)
            {
                for (int sirka = 0; sirka < 8; ++sirka)
                {
                    newDeska.GetHraciDeska[vyska, sirka] = deska.GetHraciDeska[(7 - vyska), (7 - sirka)];
                }
            }
            deska = newDeska;

            foreach (Tah tah in hra.GetTahy)
            {
                foreach (Pozice p2 in tah.GetSeznamPozic)
                {
                    p2.SetSirka(7 - p2.GetSirka);
                    p2.SetVyska(7 - p2.GetVyska);
                }
                foreach (Pozice p2 in tah.GetPreskoceneKameny)
                {
                    p2.SetSirka(7 - p2.GetSirka);
                    p2.SetVyska(7 - p2.GetVyska);
                }
            }
        }

        // Metoda, kterou jsem schopen menit barvy figur - nevyuzita metoda, ale necham ji zde
        public void ProvedZmenuBarevFigur()
        {
            Deska newDeska = new Deska();
            for (int vyska = 0; vyska < 8; ++vyska)
            {
                for (int sirka = 0; sirka < 8; ++sirka)
                {
                    int a = deska.GetHraciDeska[vyska, sirka];
                    if (a == 1)
                    {
                        newDeska.GetHraciDeska[vyska, sirka] = 2;
                    }
                    else
                        if (a == 2)
                    {
                        newDeska.GetHraciDeska[vyska, sirka] = 1;
                    }
                    else
                        newDeska.GetHraciDeska[vyska, sirka] = 0;                    
                }
            }
            deska = newDeska;
        }

        // Vrat hodnotu - protihrace:
        public Hrac VratProtihrace(Hrac hrac)
        {
            if (hrac.Equals(hrac1))
                return hrac2;
            if (hrac.Equals(hrac2))
                return hrac1;
            else
                throw new Exception("Hrac neni hracem hry");
        } 

        // Metoda na kontrolu souperovych kamenu vedle - VratPoziceKamJdeSkocit->ZjisitiPozice VeSmeru->JePosunPreskokemKameneProtihrace->seznamPozic
        public bool JeMoznostPreskoku(Deska deska, Pozice vychoziPozice)
        {
            return !(generator.VratPoziceKamJdeSkocit(deska, vychoziPozice, true).Count() == 0);
        }
          

        // Metoda na redo
        public void ProvedPosunutiTahuVpred(Tah tah)
        {            
            hra.PridejTah(tah);           
            deska.ZahrajTah(tah);
            hra.AktualizujPocetTahuBezPreskoku();
        }

        // Metoda na undo
        public void ProvedVraceniTahuZpet()
        {            
            HistorieTahu hist = new HistorieTahu();
            hist = MainWindow.GetHistorii;  
            if (hra.GetTahy.Count() == 0)
                return;           
            deska.vratTahZpet(hra.GetTahy.Last());
            hra.OdeberPosledniTah();
            hra.AktualizujPocetTahuBezPreskoku();
           
        }

    }
}
