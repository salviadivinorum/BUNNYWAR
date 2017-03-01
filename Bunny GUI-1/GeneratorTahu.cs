using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;


namespace Bunny_GUI_1
{
    [Serializable]
    // Generator obsahuje to dulezite pro pocitacoveho hrace  - hlavne algoritmus Alfa-beta, metodu GenerujNejlepsiTah
    public class GeneratorTahu
    {
        DispecerHry dispecerHry;

        [NonSerialized] //  BackgroundWorker se NESERIALIZUJE !!
        public BackgroundWorker bw2 = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true } ;
        
        //konstruktor
        public GeneratorTahu()
        { 
        }
        
        public double pocetProcentTahu = 0;
        public double jednoProcento = 0;
        
        private int MNOHO = 900;
        private int MAX = 990;        
        
        public DispecerHry GetDispecerHry
        {
            get { return dispecerHry; }
        }

        public DispecerHry SetDispecerHry
        {
            set { dispecerHry = value; }
        }

        // pomocne funkce pro alfa-beta algoritmus:
        private int Dal(int cena)
        {
            if (cena > MNOHO)
                return cena + 1;
            if (cena < -MNOHO)
                return cena - 1;
            return cena;
        }

        private int Bliz(int cena)
        {
            if (cena > MNOHO)
                return cena - 1;
            if (cena < -MNOHO)
                return cena + 1;
            return cena;
        }

        // algoritmus alfa-beta
        private int Alfabeta(Deska deska, int hloubka, Hrac hracKteryHral, int alfa, int beta)
        {            
            List<Tah> tahy = new List<Tah>();
            Hrac hracNaTahu = dispecerHry.VratProtihrace(hracKteryHral);
            
            // zakladni podminky:
            if (dispecerHry.GetHra.HracProhral(hracNaTahu))
                return -MAX;
            if (dispecerHry.GetHra.HracVyhral(hracNaTahu))
                return MAX;
            if (dispecerHry.GetHra.JeRemiza())
                return 0;
            if (hloubka == 0)
                return OhodnotPozici(deska, hracNaTahu);
            
            tahy = GenerujTahy(deska, hracNaTahu);

            for (int i = 0; i < tahy.Count(); ++i)
            {
                deska.ZahrajTah(tahy[i]);
                int cena = -Alfabeta(deska, hloubka - 1, hracNaTahu, Dal(-beta), Dal(-alfa));
                cena = Bliz(cena);
                deska.vratTahZpet(tahy[i]);
                if (cena > alfa)
                {
                    alfa = cena;
                    if (cena >= beta)
                    {
                        return beta;
                    }
                }
            }
            return alfa;
        }


        // Ohodnoceni pozice
        private int OhodnotPozici(Deska deska, Hrac hracNaTahu)
        {
            int pocetKamenuHraceNaTahu = deska.VratPocetPozic(hracNaTahu.VratTypKameneHrace());
            int pocetKamenuProtihrace = deska.VratPocetPozic(dispecerHry.VratProtihrace(hracNaTahu).VratTypKameneHrace());            
            return pocetKamenuHraceNaTahu - pocetKamenuProtihrace;
        }          
       

        // Zjisteni nejlepsiho tahu - sem vznasim dotaz z CodeBehind hlavniho okna
        public Tah GenerujNejlepsiTah(Deska deska, int hloubka, Hrac hracNaTahu)
        { 
            // musim si nastavit instanci bw2 na instanci bw hlavniho okna, jen kvuli progress baru         
            if (!MainWindow.GetDispecerHry().GetHracNaTahu.GetJePocitacovyHrac)
            {
                bw2 = MainWindow.bwNapoveda;
            }
            else
            {
                bw2 = MainWindow.bw;
            }            

            List<Tah> tahy = new List<Tah>();
            int indexNejlepsiho = 0;  
            tahy = GenerujTahy(deska, hracNaTahu);
            int alfa = -MAX;

            jednoProcento = tahy.Count() / 100.0; // pro komunikaci s hlavnim oknem - mujProgressBar

            for (int i = 0; i < tahy.Count(); ++i)
            {
                pocetProcentTahu = i / jednoProcento; // doplneno k pocitani procent

                if (MainWindow.GetDispecerHry().GetZpomaleni)
                {
                    switch (hloubka) // zpomaleni vypoctu tahu
                    {
                        case 1:
                            System.Threading.Thread.Sleep(30);
                            break;
                        case 2:
                            System.Threading.Thread.Sleep(27);
                            break;
                        case 3:
                            System.Threading.Thread.Sleep(18);
                            break;
                        case 4:
                            System.Threading.Thread.Sleep(18);
                            break;
                        default:
                            break;
                    } 
                }

                // Tady vystupuju z cyklu, kdyz jsem stisknul Stop tlacitko:
                if (MainWindow.GetZrusVypocet)
                {
                    MainWindow.bw.CancelAsync();
                    MainWindow.bwNapoveda.CancelAsync();                    
                    break;
                }

                bw2.ReportProgress((int)pocetProcentTahu); // komunikace s ovladacim prvkem mujProgressBar                
                deska.ZahrajTah(tahy[i]);                
                int cena = -Alfabeta(deska, hloubka - 1, hracNaTahu, -MAX, Dal(-alfa));
                cena = Bliz(cena);
                deska.vratTahZpet(tahy[i]);
                if (cena > alfa)
                {
                    alfa = cena;
                    indexNejlepsiho = i;
                }
            }
            return tahy[indexNejlepsiho];
        }        


        // Generuje vsechny mozne tahy:
        private List<Tah> GenerujTahy(Deska deska, Hrac hracNaTahu)
        {            
            List<Tah> vygenerovaneTahy = new List<Tah>();
            List<Pozice> kamenyHrace = hracNaTahu.VratKameny();
            List<Tah> vygenerovaneTahySPreskokem = new List<Tah>();
            foreach (Pozice odkud in kamenyHrace)
            {
                foreach (Pozice kam in VratPoziceKamJdeSkocit(deska, odkud, false))
                {
                    List<Pozice> poziceProTah = new List<Pozice>();
                    poziceProTah.Add(odkud);
                    poziceProTah.Add(kam);
                    if (deska.JePosunPreskokemKameneProtihrace(odkud, kam))
                    {
                        List<Tah> seznamVygenerovanychTahu = new List<Tah>();
                        deska.ProvedPosunKamene(odkud, kam, null);
                        GenerujTahyPreskoku(kam, deska, poziceProTah, seznamVygenerovanychTahu);
                        // tady se mi stradaji tahy, ktere jsou preskokem:
                        vygenerovaneTahySPreskokem.AddRange(seznamVygenerovanychTahu);                        
                        deska.VratZpetPosunKamene(kam, odkud, deska.VratPoziciMeziPosunem(kam, odkud));
                        continue;
                    }
                    vygenerovaneTahy.Add(MainWindow.VytvorTah(poziceProTah));
                }
            }

            // Zamicham vypocitane tahy bez preskoku
            Zamichej(vygenerovaneTahy);
            // A pro ucinnejsi orezavani v alfa-beta alg. pridam na zacatek seznamu nejsilnejsi tahy, zrychluje to vypocet pomerne hodne
            vygenerovaneTahy.InsertRange(0, vygenerovaneTahySPreskokem);            
            return vygenerovaneTahy;
        }



        // Generuje tahy  - Pouze Preskoku:
        public void GenerujTahyPreskoku(Pozice odkud, Deska deska, List<Pozice> seznamPozicPreskoku, List<Tah> seznamVygenerovanychTahu)
        {            
            List<Pozice> seznamPreskoku = new List<Pozice>(seznamPozicPreskoku);
            List<Pozice> poziceProPreskok = VratPoziceKamJdeSkocit(deska, odkud, true); // negace true je false - budu zjistovat preskoky
            if (poziceProPreskok.Count() == 0)
            {
                seznamVygenerovanychTahu.Add(MainWindow.VytvorTah(new List<Pozice>(seznamPreskoku)));
                return;
            }

            foreach (Pozice kam in poziceProPreskok)
            {
                seznamPreskoku.Add(kam);
                deska.ProvedPosunKamene(odkud, kam, null);
                GenerujTahyPreskoku(kam, deska, seznamPreskoku, seznamVygenerovanychTahu);
                int index = seznamPreskoku.Count() - 1;
                seznamPreskoku.RemoveAt(index);
                deska.VratZpetPosunKamene(kam, odkud, deska.VratPoziciMeziPosunem(kam, odkud));
            }
        }


        // Zjisti seznam pozic kam vsude jde z vychozi pozice skocit.
        // Prakticky se jedna o zjisteni skoku do 8-mi smeru (existuje 8 okolnich policek, lisicich se x,y o jednicku)
        // bud jsou obsazene, nebo volne a mozna i jde skakat dal nez o jedno .... viz seznamPozic
        public List<Pozice> VratPoziceKamJdeSkocit(Deska deska, Pozice vychoziPozice, bool jenPreskok)
        {            
            List<Pozice> seznamPozic = new List<Pozice>();
            Pozice pomocnaPozice = new Pozice();
            List<Pozice> generovanePozice = new List<Pozice>();

            int sirka;
            int vyska;

            pomocnaPozice.SetSirka(vychoziPozice.GetSirka);
            pomocnaPozice.SetVyska(vychoziPozice.GetVyska);

            for (int i = 0; i < 8; ++i)
            {
                switch (i)
                {
                    case 0:
                        vyska = 1;
                        sirka = 0;
                        generovanePozice = ZjistiPoziceVeSmeru(deska, vychoziPozice, pomocnaPozice, sirka, vyska, jenPreskok, false);
                        if (generovanePozice == null) continue;
                        seznamPozic.AddRange(generovanePozice);
                        continue;
                    case 1:
                        vyska = 1;
                        sirka = 1;
                        generovanePozice = ZjistiPoziceVeSmeru(deska, vychoziPozice, pomocnaPozice, sirka, vyska, jenPreskok, false);
                        if (generovanePozice == null) continue;
                        seznamPozic.AddRange(generovanePozice);
                        continue;
                    case 2:
                        vyska = 0;
                        sirka = 1;
                        generovanePozice = ZjistiPoziceVeSmeru(deska, vychoziPozice, pomocnaPozice, sirka, vyska, jenPreskok, false);
                        if (generovanePozice == null) continue;
                        seznamPozic.AddRange(generovanePozice);
                        continue;
                    case 3:
                        vyska = -1;
                        sirka = 1;
                        generovanePozice = ZjistiPoziceVeSmeru(deska, vychoziPozice, pomocnaPozice, sirka, vyska, jenPreskok, false);
                        if (generovanePozice == null) continue;
                        seznamPozic.AddRange(generovanePozice);
                        continue;
                    case 4:
                        vyska = -1;
                        sirka = 0;
                        generovanePozice = ZjistiPoziceVeSmeru(deska, vychoziPozice, pomocnaPozice, sirka, vyska, jenPreskok, false);
                        if (generovanePozice == null) continue;
                        seznamPozic.AddRange(generovanePozice);
                        continue;
                    case 5:
                        vyska = -1;
                        sirka = -1;
                        generovanePozice = ZjistiPoziceVeSmeru(deska, vychoziPozice, pomocnaPozice, sirka, vyska, jenPreskok, false);
                        if (generovanePozice == null) continue;
                        seznamPozic.AddRange(generovanePozice);
                        continue;
                    case 6:
                        vyska = 0;
                        sirka = -1;
                        generovanePozice = ZjistiPoziceVeSmeru(deska, vychoziPozice, pomocnaPozice, sirka, vyska, jenPreskok, false);
                        if (generovanePozice == null) continue;
                        seznamPozic.AddRange(generovanePozice);
                        continue;
                    case 7:
                        vyska = 1;
                        sirka = -1;
                        generovanePozice = ZjistiPoziceVeSmeru(deska, vychoziPozice, pomocnaPozice, sirka, vyska, jenPreskok, false);
                        if (generovanePozice == null) break;
                        seznamPozic.AddRange(generovanePozice);
                        break;
                }
            }
            return seznamPozic;
        }


        // zjistuje prazdne pozice nebo preskocitelne(protihracova pozice) v danem smeru sirka, vyska:
        private List<Pozice> ZjistiPoziceVeSmeru(Deska deska, Pozice vychoziPozice, Pozice pomocnaPozice, int sirka, int vyska, bool jenPreskok, bool bezPreskoku)
        { 
            // rekurzivne si pricitam nebo odecitam o jednicku (sirku a vysku Pozice)
            int novaVyska = vychoziPozice.GetVyska + vyska;
            int novaSirka = vychoziPozice.GetSirka + sirka;
            if (Pozice.OdpovidaRozsahu(novaSirka) && Pozice.OdpovidaRozsahu(novaVyska))
            {
                pomocnaPozice.SetSirka(novaSirka);
                pomocnaPozice.SetVyska(novaVyska);

                // Jestlize je vedlejsi policko prazdne 
                if (deska.JePolePrazdne(pomocnaPozice))
                {
                    // bool hodnota - kdyz chci zjistit jen ty pozice kdy neskacu pres protihrace:
                    if (!jenPreskok)
                    {
                        List<Pozice> seznamPozic = new List<Pozice>();
                        //rekurzivne volam tutez funkci, dokud parametry novaVyska a novaSirka odpovidaji rozsahu desky:
                        List<Pozice> vracenePozice = ZjistiPoziceVeSmeru(deska, pomocnaPozice, pomocnaPozice.Klonuj(), sirka, vyska, false, true);
                        seznamPozic.Add(pomocnaPozice.Klonuj());
                        if (vracenePozice != null)
                            seznamPozic.AddRange(vracenePozice);
                        return seznamPozic;
                    }
                    return null;
                }

                // jestlize je vedleji policko obsazene:
                novaSirka = vychoziPozice.GetSirka + sirka * 2;
                novaVyska = vychoziPozice.GetVyska + vyska * 2;

                if (Pozice.OdpovidaRozsahu(novaSirka) && Pozice.OdpovidaRozsahu(novaVyska))
                {
                    pomocnaPozice.SetSirka(novaSirka);
                    pomocnaPozice.SetVyska(novaVyska);

                    // Jestlize se jedna o preskok kamene soupere:
                    if (deska.JePosunPreskokemKameneProtihrace(vychoziPozice, pomocnaPozice))
                    {
                        List<Pozice> seznamPozic = new List<Pozice>();
                        seznamPozic.Add(pomocnaPozice.Klonuj());
                        return seznamPozic;
                    }
                    return null;
                }
                return null;
            }
            return null;
        }


        // Nahodne zamichani seznamu tahu, default = 5x zamichej
        public static void Zamichej(List<Tah> seznamTahuKZamichni, int kolikratZamichat = 5)
        {
            Random nahoda = new Random();

            List<Tah> novySeznam = new List<Tah>();
            for (int i = 0; i < kolikratZamichat; i++)
            {
                while (seznamTahuKZamichni.Count > 0)
                {
                    int index = nahoda.Next(seznamTahuKZamichni.Count);
                    novySeznam.Add(seznamTahuKZamichni[index]);
                    seznamTahuKZamichni.RemoveAt(index);
                }
                seznamTahuKZamichni.AddRange(novySeznam);
                novySeznam.Clear();
            }
        }        
        
    }
}



