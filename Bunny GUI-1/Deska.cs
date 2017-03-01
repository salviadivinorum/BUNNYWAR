using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Bunny_GUI_1
{
    [Serializable]
    public class Deska // definuju si desku jako pole 8x8 intů
    {
        int[,] hraciDeska = new int[8, 8];
        DispecerHry dispecerHry;


        //Vlastnosti:
        public DispecerHry GetDispecerHry
        {
            get { return dispecerHry; }
        }
        public DispecerHry SetDispecerHry
        {
            set { dispecerHry = value; }
        }

        public int[,] GetHraciDeska
        {
            get { return hraciDeska; }
        }

        public int[,] SetHraciDeska
        {
            set { hraciDeska = value; }
        }


        //Konstruktor:
        public Deska()
        {
            naplnDeskuPrazdnymiPoly();
        }


        //Bezne metody:

        // naplňuji hrací desku nulama:
        public void naplnDeskuPrazdnymiPoly()
        {
            for (int vyska = 0; vyska < 8; vyska++)
            {
                for (int sirka = 0; sirka < 8; sirka++)
                {
                    hraciDeska[vyska, sirka] = 0;
                }
            }
        }


        // Hledam kolikrat je treba cerny hrac(2) umisten se svymi kameny na hraci plose:
        public int VratPocetPozic(int hledanyTyp)
        {
            int result = 0;
            for (int vyska = 0; vyska < 8; vyska++)
            {
                for (int sirka = 0; sirka < 8; sirka++)
                {
                    if (hraciDeska[vyska, sirka] == hledanyTyp)
                        result++;
                }
            }
            return result;

        }

        // Za zadanych [sirka,vyska] intů si vytvorim Pozici - objekt se sloty x,y:
        public Pozice VytvorPozici(int sirka, int vyska)
        {
            Pozice pozice = new Pozice();
            pozice.SetVyska(vyska);
            pozice.SetSirka(sirka);
            return pozice;
        }


        // Hledam pozice - treba cernych kamenu(2) (hrac1 = 1-bily, hrac2=2-cerny na hraci 
        // desce [x,y] (coz je dvourozmerne pole 8x8),
        // a ty pozice si pak strcim do seznamu pozic jako vysledek:
        public List<Pozice> VratPoziceProTypKamene(int hledanyTyp)
        {
            List<Pozice> result = new List<Pozice>();
            for (int vyska = 0; vyska < 8; ++vyska)
            {
                for (int sirka = 0; sirka < 8; ++sirka)
                {
                    if (hraciDeska[vyska, sirka] != hledanyTyp)
                        continue;
                    result.Add(MainWindow.VytvorPozici(sirka, vyska));
                }
            }
            return result;
        }

        
        // Nastavuju si startovni kameny - desku [8,8] zaplnim 16ti hodnotami
        // int 2(cerny hrac) a 16ti hodnotami int 1(bily hrac):
        public void NastavStartovniKameny()
        {
            int sirka;
            int vyska;
            int pocetRadku = 2;
            for (vyska = 0; vyska < pocetRadku; vyska++)
            {
                for (sirka = 0; sirka < 8; sirka++)
                {
                    hraciDeska[vyska, sirka] = 1;  // dole na plose jsou BILE kameny = oznaceno c.1

                }
            }

            for (vyska = 7; vyska >= 8 - pocetRadku; vyska--)
            {
                for (sirka = 0; sirka < 8; sirka++)
                {
                    hraciDeska[vyska, sirka] = 2; // nahore na plose jsou CERNE kameny = oznaceno c.2
                }
            }

            
            for (int zbytekVyska = 2; zbytekVyska < 6; zbytekVyska++)
            {
                for (int zbytekSirka = 0; zbytekSirka < 8; zbytekSirka++)
                {
                    hraciDeska[zbytekVyska, zbytekSirka] = 0;   // zbytek volnych policek musim nastavit na 0 

                }
            }
        }



        // Pozice je pomocny objekt, ma stejne cleny vyska(0-7), sirka(0-7) jako ma Deska, coz je pole [8,8]
        // na zadane pozici je bud cislo 1 (Bily) nebo cislo 2 (Cerny hrac):   
        public int VratObsahPole(Pozice pozice)
        {
            return hraciDeska[pozice.GetVyska, pozice.GetSirka];
        }


        // Kontrolni dotaz, jestli je pozice v poli prazdna [v,s] = 0:
        public bool JePolePrazdne(Pozice pozice)
        {
            return VratObsahPole(pozice) == 0;
        }

        // Rozdil 2-ou Pozic VYSKOVY (univerzalni pouziti):
        public int RozdilPozicVyska(Pozice odkud, Pozice kam, bool abs)
        {
            if (abs)
            {
                return Math.Abs(kam.GetVyska - odkud.GetVyska);
            }
            else
                return kam.GetVyska - odkud.GetVyska;
        }

        // Rozdil 2-ou pozic SIRKOVY (univerzalni pouziti):
        public int RozdilPozicSirka(Pozice odkud, Pozice kam, bool abs)
        {
            if (abs)
            {
                return Math.Abs(kam.GetSirka - odkud.GetSirka);
            }
            else
                return kam.GetSirka - odkud.GetSirka;
        }


        // Test na Legalni posun figurkou a zaroven Posun Bez preskoku:
        public bool JdeOLegalniPosunBezPreskoku(Pozice odkud, Pozice kam)
        {
            return !JePolePrazdne(odkud) && JePolePrazdne(kam) &&   // pole odkud neni prazdne a pole kam je prazdne a  zaroven musi byt splnena jedna z podminek:
                (RozdilPozicVyska(odkud, kam, true) == RozdilPozicSirka(odkud, kam, true) || // 1. absolutni rozdil vysek, sirek je stejny (Diagonalni posun)
                RozdilPozicVyska(odkud, kam, true) == 0 || // 2. rozdil vysek je 0, cili vodorovny posun
                RozdilPozicSirka(odkud, kam, true) == 0) &&  // 3. rozdil sirek je 0, cili svisly posun
                JsouPoleMeziPosunemPrazdne(odkud, kam); // 4. pole mezi posunem jsou prazdna
        }


        // Slozitejsi test, zda-li jsou policka mezi posunem prazdna - tzv. Posun Bez Preskoku:
        public bool JsouPoleMeziPosunemPrazdne(Pozice odkud, Pozice kam)
        {
            if (RozdilPozicVyska(odkud, kam, true) <= 1 && RozdilPozicSirka(odkud, kam, true) <= 1)
            {
                return true;  // Automaticky, kdyz je rozdil nulovy nebo jen o jednu pozici, vysledek je true
            }

            int pocet = Math.Max(RozdilPozicVyska(odkud, kam, true), RozdilPozicSirka(odkud, kam, true));
            for (int i = 1; i < pocet; ++i)
            {
                int znamenko;
                int sirka;
                int vyska;
                if (odkud.GetSirka == kam.GetSirka)
                {
                    sirka = odkud.GetSirka;
                }
                else
                {
                    znamenko = Math.Sign(kam.GetSirka - odkud.GetSirka);
                    sirka = odkud.GetSirka + i * znamenko;
                }
                if (odkud.GetVyska == kam.GetVyska)
                {
                    vyska = kam.GetVyska;
                }
                else
                {
                    znamenko = Math.Sign(kam.GetVyska - odkud.GetVyska);
                    vyska = odkud.GetVyska + i * znamenko;
                }
                if (!JePolePrazdne(VytvorPozici(sirka, vyska))) // pokud na zadane nove vytvorene pozici neni nula, ukonci cyklus a vysledek metody je false
                    return false;
            }
            return true; // pokud cyklus prosel cely, tak vysledek metody je true
        }

        // Skok o jedno policko (rakticky 8 ruznych smeru)
        public bool JePosunPreskokem(Pozice odkud, Pozice kam)
        {
            return
                ((RozdilPozicVyska(odkud, kam, true) == 2 && RozdilPozicSirka(odkud, kam, true) == 2) &&  // skacu ob-jedno policko // tady jsem mel chybu, bylo potreba osetrit 3 mozne stavy, kdy jsou bud obe s,v stejne
                !JePolePrazdne(odkud) && JePolePrazdne(kam)) || // a zaroven skacu z neprazdneho na prazdne policko
                ((RozdilPozicVyska(odkud, kam, true) == 2 && RozdilPozicSirka(odkud, kam, true) == 0) &&  // stav kdy je pouze v rozdilna o 2 policka
                !JePolePrazdne(odkud) && JePolePrazdne(kam)) ||
                ((RozdilPozicVyska(odkud, kam, true) == 0 && RozdilPozicSirka(odkud, kam, true) == 2) &&  // stav kdy je pouze s rozdilna o 2 policka
                !JePolePrazdne(odkud) && JePolePrazdne(kam));
        }


        // Vysledkem teto metody je Pozice, ktera se pozdeji ma vymazat - preskocit pri ztv. ob-skoku pres soupere:
        public Pozice VratPoziciMeziPosunem(Pozice odkud, Pozice kam)
        {
            if (!JePosunPreskokem(odkud, kam)) // Vyvolani vyjimky kdyz nejde o ob-skokem
            {
                MessageBox.Show("Doslo k vyjimce v metode VratPoziciMeziPosunem pro aktualni desku");                
                throw new Exception("Tento skok neni preskokem!");
            }

            int vyska = odkud.GetVyska + RozdilPozicVyska(odkud, kam, false) / 2;
            int sirka = odkud.GetSirka + RozdilPozicSirka(odkud, kam, false) / 2;
            return VytvorPozici(sirka, vyska);
        }

        // Test, jestli skacu ob-policko a na tom ob-policku byl souper:
        public bool JePosunPreskokemKameneProtihrace(Pozice odkud, Pozice kam)
        {
            return JePosunPreskokem(odkud, kam) && // jedna se o ob-skok a zaroven
                !JePolePrazdne(VratPoziciMeziPosunem(odkud, kam)) && // policko, ktere jsem ob-skocil neni prazdne a zaroven
                VratObsahPole(odkud) != VratObsahPole(VratPoziciMeziPosunem(odkud, kam)); // obsah pole odkud (ja, treba 1=bily) se nerovna obsahu pole policka preskoceneho (trebas souper 2=cerny)
        }

        // Test na celkovou legalnost meho pohybu po desce:
        public bool JePosunLegalni(Pozice odkud, Pozice kam)
        {
            return JdeOLegalniPosunBezPreskoku(odkud, kam) || JePosunPreskokemKameneProtihrace(odkud, kam);
            // mam jenom 2 druhy tahu: diagonalne-ortogonalni bez preskoků soupere 
            // nebo ciste jen preskok soupere ob-policko
        }


        // Prakticky presun, vymazani kamenu z desky, cili pole jako takove
        // meni hodnoty [8,8] svych prvku, muze to byt vzdy jen jedna ze tri hodnot:
        // 0 = volno, 1 = bily hrac, 2 = cerny hrac
        public bool ProvedPosunKamene(Pozice odkud, Pozice kam, Tah tah)
        {
            int odkudKamen = hraciDeska[odkud.GetVyska, odkud.GetSirka];
            if (JdeOLegalniPosunBezPreskoku(odkud, kam))
            {
                hraciDeska[kam.GetVyska, kam.GetSirka] = odkudKamen;
                hraciDeska[odkud.GetVyska, odkud.GetSirka] = 0;
                return true;
            }
            if (JePosunPreskokemKameneProtihrace(odkud, kam))
            {
                Pozice preskakovanyKamen = VratPoziciMeziPosunem(odkud, kam);
                hraciDeska[kam.GetVyska, kam.GetSirka] = odkudKamen;
                hraciDeska[odkud.GetVyska, odkud.GetSirka] = 0;
                hraciDeska[preskakovanyKamen.GetVyska, preskakovanyKamen.GetSirka] = 0;
                if (tah != null)
                {
                    tah.PridejPreskocenyKamen(preskakovanyKamen); // ve tride Tah je slot na preskocene kameny
                }
                return true;
            }
            return false;
        }


        // Zahraj tah:
        // Pomocna kontrola, kde se mi stala chyba
        public bool ZahrajTah(Tah tah)
        {
            List<Pozice> seznamPozic = tah.GetSeznamPozic;
            Pozice odkud = null;
            List<Pozice> odehranePozice = new List<Pozice>();
            foreach (Pozice kam in seznamPozic)
            {
                odehranePozice.Add(kam);
                if (odkud != null && !ProvedPosunKamene(odkud, kam, tah))
                {
                    MessageBox.Show("VYJIMKA: Metoda ProvedPosunKamene(odkud, kam) nebyla provedena !");                    
                    MessageBox.Show("ZahrajTah-vstupni parametr tah byl: " + tah.VratJakoText());
                    
                    throw new Exception("metoda ZahrajTah byla zavolana pro neplatny tah !");
                }
                odkud = kam;
            }
            return true;
        }


        // Vytvoreni nove instance Desky se stejne obsazenymi policky 
        // rozsireni metody Array.Clone() pro mou desku 8x8:
        public Deska cloneDJ()
        {
            int[,] klonObsah = (int[,])hraciDeska.Clone();
            Deska klonovana = new Deska();
            klonovana.SetHraciDeska = klonObsah;
            return klonovana;
        }


        // pomoci deskaCopy zkousim pozdeji legalnost mych tahu:
        public Deska ZkusZahratTah(Tah tah)
        {
            List<Pozice> seznamPozic = tah.GetSeznamPozic;
            Pozice odkud = null;
            Deska deskaCopy = cloneDJ();

            if (seznamPozic.Count() == 2)
            {
                Pozice kam;
                odkud = seznamPozic[0];
                if (deskaCopy.ProvedPosunKamene(odkud, kam = seznamPozic[1], tah))
                    return deskaCopy;
                return null;
            }

            foreach (Pozice kam in seznamPozic)
            {
                if (odkud != null && !deskaCopy.JePosunPreskokemKameneProtihrace(odkud, kam))
                    return null;
                if (odkud != null)
                    deskaCopy.ProvedPosunKamene(odkud, kam, tah);
                odkud = kam;
            }
            return deskaCopy;
        }

        // pouziti pozdeji v metodach GenerujTahy, GenerujNejlepsiTah
        public bool vratTahZpet(Tah tah)
        {
            Pozice odkud = null;
            List<Pozice> seznamPozic = new List<Pozice>(tah.GetSeznamPozic);
            seznamPozic.Reverse();
            List<Pozice> preskoceneKameny = new List<Pozice>(tah.GetPreskoceneKameny);
            foreach (Pozice kam in seznamPozic)
            {
                if (odkud != null)
                {
                    Pozice preskocena = new Pozice();

                    if (preskoceneKameny.Count() == 0)
                        preskocena = null;
                    else
                    {
                        preskocena = preskoceneKameny.Last();
                        int index = preskoceneKameny.Count() - 1;
                        preskoceneKameny.RemoveAt(index);
                    }
                    if (!VratZpetPosunKamene(odkud, kam, preskocena))
                    {
                        MessageBox.Show("VYJIMKA: metoda vratTahZpet byla zavolana pro neplatny tah!");
                        MessageBox.Show("vratTahZpet Odkud: " + odkud.VratJakoText() + "vratTahZpet Kam: " + kam.VratJakoText() + "vratTahZpet preskocena: " + preskocena);
                        
                        throw new Exception("metoda vratTahZpet byla zavolana pro neplatny tah!");
                    }
                }
                odkud = kam;
            }
            return true;
        }

        // pouziti pozdeji v metodach GenerujTahy a GenerujTahyPreskoku
        public bool VratZpetPosunKamene(Pozice odkud, Pozice kam, Pozice preskocena)
        {
            int odkudKamen = hraciDeska[odkud.GetVyska, odkud.GetSirka];
            if (preskocena == null && JdeOLegalniPosunBezPreskoku(odkud, kam))
            {
                hraciDeska[kam.GetVyska, kam.GetSirka] = odkudKamen;
                hraciDeska[odkud.GetVyska, odkud.GetSirka] = 0;
                return true;
            }
            if (JePosunPreskokem(odkud, kam))
            {
                Pozice preskakovanaPozice = VratPoziciMeziPosunem(odkud, kam);
                if (hraciDeska[preskakovanaPozice.GetVyska, preskakovanaPozice.GetSirka] == 0)
                {
                    hraciDeska[kam.GetVyska, kam.GetSirka] = odkudKamen;
                    hraciDeska[odkud.GetVyska, odkud.GetSirka] = 0;
                    hraciDeska[preskakovanaPozice.GetVyska, preskakovanaPozice.GetSirka] = MainWindow.VratOpacnyTypKamene(odkudKamen);
                    return true;
                }
                return false;
            }
            return false;
        }

    }

}

