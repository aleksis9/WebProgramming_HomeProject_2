using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TuristickaAgencija.Models
{
    public class MestoNalazenja
    {
        public string Adresa { get; set; }
        public double GeografskaDuzina { get; set; }
        public double GeografskaSirina { get; set; }

        public MestoNalazenja() { }

        public MestoNalazenja(string adresa, double geografskaDuzina, double geografskaSirina)
        {
            Adresa = adresa;
            GeografskaDuzina = geografskaDuzina;
            GeografskaSirina = geografskaSirina;
        }

        public static MestoNalazenja Parse(string path)
        {
            MestoNalazenja ucitano = new MestoNalazenja();

            string lokacija = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\MestaNalazenja\\" + path;

            StreamReader sr = new StreamReader(lokacija);

            string[] parts = sr.ReadLine().Split('\t');

            ucitano.Adresa = parts[0];
            ucitano.GeografskaDuzina = double.Parse(parts[1]);
            ucitano.GeografskaSirina = double.Parse(parts[2]);

            sr.Close();

            return ucitano;
        }
    }
}