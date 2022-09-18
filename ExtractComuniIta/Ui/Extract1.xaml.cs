using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ExtractComuniIta.ComuneMode;

namespace ExtractComuniIta.Ui
{
    /// <summary>
    /// Interaction logic for Extract1.xaml
    /// </summary>
    public partial class Extract1 : UserControl
    {
        public string File1 = @"C:\Users\mboum\Downloads\comuni ita\ELENCO CODICI DEI COMUNI ITALIANI.csv";
        public string File2 = @"C:\Users\mboum\Downloads\comuni ita\e1.csv";

        private DatiComuneItalia datiComuneItalia = new DatiComuneItalia();

        public string CurrentRegion { get; set; }
        public string CurrentProvince { get; set; }
        public Extract1()
        {
            InitializeComponent();
            var checklist = new HashSet<string>();

            var list = File.ReadAllLines(File2, Encoding.UTF8).Skip(1);//.Take(1000);
            var count = 0;
            foreach (var line in list)
            {
                count++;

                var tokens = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 3)
                {
                    var comune = tokens[0];
                    var regione = tokens[1];
                    var provincia = tokens[2];

                    checklist.Add(comune);

                    var datiComune = new Comune(comune, provincia, regione);


                    datiComuneItalia.Add(datiComune);
                }
                else
                {
                    Console.WriteLine("check data");

                }
            }

            lbxRegion.ItemsSource = datiComuneItalia.DicoRegion.Keys;
            var all = datiComuneItalia.GetAllProvinces();
            var json = JsonSerializer.Serialize(all, new JsonSerializerOptions()
            {
                WriteIndented = true
            });
            Console.WriteLine(all);
            Console.WriteLine(json);
            File.WriteAllText(@"C:\Users\mboum\Downloads\comuni ita\comunesItajson.json", json, Encoding.UTF8);

        }

        private void lbxRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var nomRegion = lbxRegion.SelectedValue as string;
            if (nomRegion != null)
            {
                CurrentRegion = nomRegion;
                var regions = datiComuneItalia.DicoRegion[nomRegion].DicoProvince.Keys;
                lbxProvincia.ItemsSource = regions;
            }
        }

        private void lbxProvincia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var nomProvince = lbxProvincia.SelectedValue as string;
            if (nomProvince != null)
            {
                CurrentProvince = nomProvince;
                var comunes = datiComuneItalia.DicoRegion[CurrentRegion].DicoProvince[nomProvince].ListeComune;
                lbxComuni.ItemsSource = comunes;
            }
        }
    }
}
