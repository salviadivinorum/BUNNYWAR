using System;
using System.Collections.Generic;
using System.Linq;

namespace Bunny_GUI_1
{
    [Serializable]
    public class Hra
    {        
        public int pocetTahuBezPreskoku = 0;
        public DispecerHry dispecerHry;
        public List<Tah> tahy = new List<Tah>();
        
        // konstruktor
        public Hra()
        {
            tahy = new List<Tah>();
        }
        

        // Vlastnosti a metody:

        //Dispečer hry:
        public DispecerHry getDispecerHry
        {
            get { return dispecerHry; }
        }

        public DispecerHry SetDispecerHry
        {
            set
            {
                if (value.GetType() != typeof(DispecerHry))
                    throw new Exception("Dispecer hry nemuze byt zadanym objektem: " + value.GetType());
                dispecerHry = value;
            }
        }

        // Slot na tahy:
        public List<Tah> GetTahy
        {
            get { return tahy; }
        }

        public List<Tah> SetTahy
        {
            set { tahy = value; }
        }

        public void VynulujTahy()
        {
            tahy.Clear();
        }

        public void PridejTah(Tah tah)
        {
            tahy.Add(tah);
        }

        public void OdeberPosledniTah()
        {
            int index = tahy.Count() - 1;
            tahy.RemoveAt(index);
        }


        // Tahy bez preskoku:
        public int GetPocetTahuBezPreskoku
        {
            get { return pocetTahuBezPreskoku; }
        }

        public int SetPocetTahuBezPreskoku
        {
            set { pocetTahuBezPreskoku = value; }
        }

        public void VynulujPocitadloTahuBezPreskoku()
        {
            SetPocetTahuBezPreskoku = 0;
        }

        public void PridejTahBezPreskoku()
        {
            SetPocetTahuBezPreskoku = GetPocetTahuBezPreskoku + 1;
        }

        // dulezite - hlidam si pri akci zpet/dopredu Pocet tahu bez preskoku
        public void AktualizujPocetTahuBezPreskoku()
        {
            int result = 0;
            int pocetTahu = tahy.Count;
            for (int i =0; i<pocetTahu; i++)
            {
                int maPreskoceneKameny = tahy[i].GetPreskoceneKameny.Count;
                if (maPreskoceneKameny == 0)
                {
                    result++;
                }
                else
                    result = 0;
            }
            SetPocetTahuBezPreskoku = result;
        }


        // výsledky hry:
        public bool JeRemiza()
        {
            return GetPocetTahuBezPreskoku >= 30;
        }

        // Podminky Prohry:
        public bool HracProhral(Hrac hrac)
        {
            bool maloKamenu = hrac.VratPocetKamenu() < 2;
            return maloKamenu || NeniKamTahnout(hrac);
        }

        // Podminky kdy neni kam dal tahnout:        
        private bool NeniKamTahnout(Hrac hrac)
        {
            Deska deska = dispecerHry.GetDeska;
            List<Pozice> kamenyHrace = new List<Pozice>(deska.VratPoziceProTypKamene(hrac.VratTypKameneHrace()));
            foreach (Pozice p in kamenyHrace)
            {
                if (dispecerHry.GetGenerator.VratPoziceKamJdeSkocit(deska, p, false).Count() == 0)
                    continue;
                return false;
            }
            return true;
        }

        // Vyhra je prohra druheho:
        public bool HracVyhral(Hrac hrac)
        {
            return HracProhral(dispecerHry.VratProtihrace(hrac));

        }
    }
}
