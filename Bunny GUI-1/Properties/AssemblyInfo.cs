using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// Obecné informace o sestavení se řídí přes následující 
// sadu atributů. Změnou hodnot těchto atributů se upraví informace
// přidružené k sestavení.
[assembly: AssemblyTitle("Bunny GUI")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Bunny GUI")]
[assembly: AssemblyCopyright("Copyright ©  2016 David Jaroš")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Nastavení atributu ComVisible na hodnotu False udělá typy v tomto sestavení neviditelné 
// komponent modelu COM.  Pokud potřebujete přistoupit k typu v tomto sestavení z 
// modelu COM, nastavte atribut ComVisible daného typu na hodnotu True.
[assembly: ComVisible(false)]

//Pro tvorbu lokalizovatelných aplikací, nastavte 
//<UICulture>JazykováVerzeVeKteréPíšeteKód</UICulture> v souboru .csproj
//uvnitř <PropertyGroup>.  Pokud například používáte jazykovou verzi US english
//ve zdrojových souborech, nastavte <UICulture> na en-US.  Pak zrušte komentář
//pro atribut NeutralResourceLanguage.  Aktualizujte hodnotu "en-US" na
//dalším řádku, aby se shodovala s nastavením UICulture v souboru projektu.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //kde se nacházejí zdrojové slovníky pro konkrétní motiv
                                     //(použité, pokud prostředek není nalezen na stránce, 
                                     // nebo ve zdrojových slovnících aplikace)
    ResourceDictionaryLocation.SourceAssembly //kde se nachází obecný zdrojový slovník
                                              //(použité, pokud prostředek není nalezen na stránce, 
                                              // v aplikaci nebo libovolných zdrojových slovnících pro konkrétní motiv)
)]


// Informace o verzi sestavení se skládá z těchto čtyř hodnot:
//
//      Hlavní verze
//      Dílčí verze 
//      Číslo sestavení
//      Revize
//
// Můžete zadat všechny hodnoty nebo nechat nastavená výchozí čísla sestavení a revize 
// pomocí zástupného znaku * takto:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
