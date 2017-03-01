using System;
using System.Windows.Controls;


namespace Bunny_GUI_1
{
    /// <summary>
    /// Interakční logika pro UzivPrvekPolicko.xaml
    /// </summary>
    [Serializable]
    public partial class UzivPrvekPolicko : UserControl
    {
        bool oznacenoZelene = false;
        BarvaHrace jakouBarvouJsemObsazeno = BarvaHrace.zadna;
        int cisloHraceNaPolicku = 0;        
                
        
        // Konstruktor
        public UzivPrvekPolicko()
        {
            InitializeComponent();
        }

        // Vlastnosti:

        // Cislo hrace 1,2,0
        public int SetChrace
        {
            set { cisloHraceNaPolicku = value; }
        }

        public int GetCHrace
        {
            get { return cisloHraceNaPolicku; }
        }       


        // Barva hrace - jake barvy je hrac na tom policku
        // Barva muze byt zadna(0), bila(1), cerna(2)
        public BarvaHrace SetJakouBarvouJsemObsazeno
        {
            set { jakouBarvouJsemObsazeno = value; }
        }

        public BarvaHrace GetJakouBarvouJsemObsazeno
        {
            get { return jakouBarvouJsemObsazeno; }
        }
        

        // Ano - Ne nejsem oznaceno zelene=prvni klik mysi Odkud tahnu kamenem
        public bool SetJsemOznacenoZelene
        {
            set { oznacenoZelene = value; }
        }

        public bool GetJsemOznacenoZelene
        {
            get { return oznacenoZelene; }
        }

    }
}
