using System;


namespace Bunny_GUI_1
{
    [Serializable]
    public class Pozice
    {
        // Datove slozky:
        int vyska;
        int sirka;

        // V Y S K A:
        // Zjistuji vysku - varianta, kdyz chci hodnotu int:
        public int GetVyska
        {
            get { return vyska; }
        }

        // Varianta kdyz chci znak - char:
        public char GetVyskaChar()
        {
            return PrevodNaZnakProVysku(vyska);
        }


        // Nastavuju vysku - to je uz musim osetrit, musim mit 2 cesty - nasatvuju pomoci hodnoty int nebo char:
        // prvni cesta - muzu jit ke slotu int hodnoty primo:

        // Obycejna vlastnost - nastav slot vyska objektu Pozice:
        private int SetV
        {
            set { vyska = value; }
        }


        // druha cesta - osetruji si nejprve 2 ruzne vstupy - int/char a pak teprve jdu ke slotu primo:     
        public void SetVyska(object velikost)
        {
            Type prvniVarianta = typeof(int);
            Type druhaVarianta = typeof(char);

            if (velikost.GetType() == prvniVarianta)
            {
                int hodnota = (int)velikost;
                if (!OdpovidaRozsahu(hodnota))
                {
                    throw new Exception("Hodnota " + hodnota + " souradnice vyska je mimo povoleny rozsah.");
                }
                SetV = hodnota;
            }
            else
            {
                if (velikost.GetType() == druhaVarianta)
                {
                    char prevedu = (char)velikost;
                    int hodnota = PrevodZeZnakuProVysku(prevedu);
                    if (!OdpovidaRozsahu(hodnota))
                    {
                        throw new Exception("Hodnota " + hodnota + " souradnice vyska je mimo povoleny rozsah.");
                    }

                    SetV = hodnota;
                }
                else
                {
                    throw new Exception("Zadanym typem objektu nelze nastavit hodnotu slotu vyska objektu Pozice");
                }
            }
        }


        // S I R K A:   
        // Stejnym zpusobem pristup - jako slot vyska:
        // chci int hodnotu slotu:          
        public int GetSirka
        {
            get { return sirka; }
        }


        // Varianta kdyz chci ziskat hodnotu slotu jako znak - char:
        public char GetSirkaChar()
        {
            return PrevodNaZnakProSirku(sirka);
        }


        // Prima modifikace slotu int hodnotou:
        private int SetS
        {
            set { sirka = value; }
        }


        // Osetruji 2 ruzne moznosti vstupu pri zadavani sirky - int/char:
        public void SetSirka(object velikost)
        {
            Type prvniVarianta = typeof(int);
            Type druhaVarianta = typeof(char);

            if (velikost.GetType() == prvniVarianta)
            {
                int hodnota = (int)velikost;
                if (!OdpovidaRozsahu(hodnota))
                {
                    throw new Exception("Hodnota " + hodnota + " souradnice sirka je mimo povoleny rozsah.");
                }
                SetS = hodnota;
            }
            else
            {
                if (velikost.GetType() == druhaVarianta)
                {
                    char prevedu = (char)velikost;
                    int hodnota = PrevodZeZnakuProSirku(prevedu);
                    if (!OdpovidaRozsahu(hodnota))
                    {
                        throw new Exception("Hodnota " + hodnota + " souradnice sirka je mimo povoleny rozsah.");
                    }

                    SetS = hodnota;
                }
                else
                {
                    throw new Exception("Zadanym typem objektu nelze nastavit hodnotu slotu sirka objektu Pozice");
                }
            }
        }




        // Pomocne metody na prevod ZE a NA znak:

        // Kontroluju zda-li hodnota int odpovida rozsahu (0-7):
        public static bool OdpovidaRozsahu(int hodnota)
        {
            return hodnota >= 0 && hodnota <= 7;
        }



        // Test, zda-li se jedna o Objekt Pozice a zda-li obsahuje sloty vyska, sirka:
        public bool equals(Object srovnavana)
        {
            if (typeof(Object) != typeof(Pozice))
                return false;

            Pozice sr = (Pozice)srovnavana;
            return vyska == sr.GetVyska && sirka == sr.GetSirka;
        }


        // Pomocna metoda Klonuj() - vyrabim si novou identickou instanci objektu Pozice
        public Pozice Klonuj()
        {
            Pozice poz = new Pozice();
            poz.SetVyska(vyska);
            poz.SetSirka(sirka);
            return poz;
        }


        //Prevod NA znak pro vysku:
        public static char PrevodNaZnakProVysku(int vyska)
        {
            return (char)(vyska + 49);
        }

        //Prevod NA znak pro sirku:
        public static char PrevodNaZnakProSirku(int sirka)
        {
            return (char)(sirka + 97);
        }

        //Prevod ZE znaku pro vysku:
        public static int PrevodZeZnakuProVysku(char znak)
        {
            return znak - 49;
        }

        //Prevod ZE znaku prosirku:
        public static int PrevodZeZnakuProSirku(char znak)
        {
            char z = char.ToLower(znak);
            return z - 97;
        }

        // Vraci hodnotu pozice jako text:
        public string VratJakoText()
        {
            return "" + GetSirkaChar() + GetVyskaChar();
        }
    }
}

