using System;
using System.Collections.Generic;
using System.Linq;

namespace Bunny_GUI_1
{
    [Serializable]
    public class Hrac
    {
        string jmeno = null;
        int obtiznost; // slot jak to bude mit hrac1/hrac2 obtizne - do jake hloubky tahu proti nemu pujde protihrac(pocitac)
        bool jePocitacovyHrac;
        DispecerHry dispecerHry;        
        BarvaHrace barvaHrace; 

        /* VLASTNOSTI: */
        public BarvaHrace SetBarvuHrace
        {
            set { barvaHrace = value; }
        }

        public BarvaHrace GetBarvuHrace
        {
            get { return barvaHrace; }
        }


        /* J M E N O */
        private string Jmeno
        {
            set { jmeno = value; }
        }

        public string GetJmeno
        {
            get { return jmeno; }
        }
        public void SetJmeno(string jmeno)
        {
            if (jmeno == null)
            {
                throw new Exception("Jmeno nemuze byt null !");
            }
            Jmeno = jmeno;
        }


        /* D I S P E C E R */
        public DispecerHry GetDispecerHry
        {
            get { return dispecerHry; }
        }

        public DispecerHry SetDispecerHry
        {
            set { dispecerHry = value; }
        }       


        /* O B T I Z N O S T */
        public int GetObtiznost
        {
            get { return obtiznost; }
        }

        public int SetObtiznost
        {
            set { obtiznost = value; }
        }


        /* POCITACOVY HRAC */
        public bool GetJePocitacovyHrac
        {
            get { return jePocitacovyHrac; }
        }

        public bool SetJePocitacovyHrac
        {
            set { jePocitacovyHrac = value; }
        }
        

        /* BEZNE METODY */

        // Vrat pocet kamenu hrace:
        public int VratPocetKamenu()
        {
            if (dispecerHry.GetHrac1.Equals(this))
                return dispecerHry.GetDeska.VratPocetPozic(1);
            if (dispecerHry.GetHrac2.Equals(this))
                return dispecerHry.GetDeska.VratPocetPozic(2);
            else
                throw new Exception("Hrac " + GetJmeno + " neni evidovany v dispecerovi hry !");
        }

        // Vrat seznam pozic kamenu hrace:
        public List<Pozice> VratKameny()
        {
            return dispecerHry.GetDeska.VratPoziceProTypKamene(VratTypKameneHrace());
        }

        // Test na to, zda-li hrac hraje svymi kameny 
        // metodu volam prostrednictvim dispecera, tam mam vytvoreny a obslouzeny instance trid Hrac (this)
        public bool HrajeHracSvymKamenem(Tah tah)
        {
            if (dispecerHry.GetHrac1.Equals(this))
                return VratTypKameneHrace() == dispecerHry.GetDeska.VratObsahPole(tah.GetSeznamPozic.First());
            if (dispecerHry.GetHrac2.Equals(this))
                return VratTypKameneHrace() == dispecerHry.GetDeska.VratObsahPole(tah.GetSeznamPozic.First());
            else
                throw new Exception("Hrac " + GetJmeno + " neni evidovany v dispecerovi hry !");
        }


        // Jaky kamen ma prave hrac co hraje:
        // metodu volam prostrednictvim dispecera, tam mam vytvoreny a obslouzeny instance trid Hrac (this)
        public int VratTypKameneHrace()
        {
            if (dispecerHry.GetHrac1.Equals(this))
                return 1;
            if (dispecerHry.GetHrac2.Equals(this))
                return 2;
            else
                throw new Exception("Hrac " + GetJmeno + " neni evidovany v dispecerovi hry !");
        }
    }
}
