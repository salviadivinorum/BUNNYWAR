using System;
using System.Collections.Generic;
using System.Linq;


namespace Bunny_GUI_1
{
    [Serializable] 
    public class Tah
    {
        //Datove slozky:
        public List<Pozice> seznamPozic = new List<Pozice>(); // Tah je seznam vsech pozic
        public List<Pozice> preskoceneKameny = new List<Pozice>(); // Preskocene = vyrazene kameny


        //Vlastnosti:
        public List<Pozice> GetSeznamPozic
        {
            get { return seznamPozic; }
        }

        public List<Pozice> SetSeznamPozic
        {
            set { seznamPozic = value; }
        }

        public List<Pozice> GetPreskoceneKameny
        {
            get { return preskoceneKameny; }
        }

        // Bezne metody:
        public void OdstranPreskoceneKameny()
        {
            preskoceneKameny = null;
        }

        public void PridejPreskocenyKamen(Pozice preskoceny)
        {
            preskoceneKameny.Add(preskoceny);
        }


        // Kdekoliv, kde si potrebuju vypsat seznamPozic v textovem formatu
        public string VratJakoText()
        {
            string result = "[";
            for (int i = 0; i < seznamPozic.Count(); ++i)
            {
                Pozice p = seznamPozic[i];
                result = result + p.VratJakoText();
            }
            result = result + "]";

            return result.ToUpper();
        }

        // byly pripady, kdy jsem chtel pouzit standardni metodu ToString() na libovolny tah
        public override string ToString()
        {            
            return VratJakoText();
        }
    }
}
