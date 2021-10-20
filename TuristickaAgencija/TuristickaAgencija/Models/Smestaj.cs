using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace TuristickaAgencija.Models
{
    public class Smestaj
    {
        public string Naziv { get; set; }
        public string TipSmestaja { get; set; }
        public int BrojZvezdica { get; set; }
        public bool Bazen { get; set; }
        public bool SpaCentar { get; set; }
        public bool ZaOsobeSaInvaliditetom { get; set; }
        public bool WiFi { get; set; }
        public List<SmestajnaJedinica> Jedinice { get; set; }
        public bool Obrisan { get; set; }
        public int BrojSlobodnih { get; set; }

        public Smestaj() { }

        public Smestaj(string naziv, string tipSmestaja, int brojZvezdica, bool bazen, bool spaCentar, bool zaOsobeSaInvaliditetom, bool wiFi, List<SmestajnaJedinica> jedinice,bool obrisan)
        {
            Naziv = naziv;
            TipSmestaja = tipSmestaja;
            BrojZvezdica = brojZvezdica;
            Bazen = bazen;
            SpaCentar = spaCentar;
            ZaOsobeSaInvaliditetom = zaOsobeSaInvaliditetom;
            WiFi = wiFi;
            Jedinice = jedinice;
            Obrisan = obrisan;
        }

        public static Smestaj Parse(string path)
        {
            Smestaj ucitan = new Smestaj();

            string lokacija = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\Smestaji\\" + path;

            StreamReader sr = new StreamReader(lokacija);

            string line;
            string[] parts = sr.ReadLine().Split('\t');

            ucitan.Naziv = parts[0];
            ucitan.TipSmestaja = parts[1];
            ucitan.BrojZvezdica = Int32.Parse(parts[2]);
            if (parts[3] == "True")
                ucitan.Bazen = true;
            else
                ucitan.Bazen = false;

            if (parts[4] == "True")
                ucitan.SpaCentar = true;
            else
                ucitan.SpaCentar = false;

            if (parts[5] == "True")
                ucitan.ZaOsobeSaInvaliditetom = true;
            else
                ucitan.ZaOsobeSaInvaliditetom = false;

            if (parts[6] == "True")
                ucitan.WiFi = true;
            else
                ucitan.WiFi = false;

            if (parts[7] == "True")
                ucitan.Obrisan = true;
            else
                ucitan.Obrisan = false;

            ucitan.Jedinice = new List<SmestajnaJedinica>();

            while ((line = sr.ReadLine()) != null)
            {
                parts = line.Split('\t');
                SmestajnaJedinica unit = new SmestajnaJedinica();
                unit.MaxBrojGostiju = Int32.Parse(parts[0]);
                if (parts[1] == "True")
                    unit.DozvoljeniKucniLjubimci = true;
                else
                    unit.DozvoljeniKucniLjubimci = false;
                unit.CenaZaCeluJedinicu = double.Parse(parts[2]);
                if (parts[3] == "1")
                    unit.Rezervisana = true;
                else
                    unit.Rezervisana = false;
                if (parts[4] == "1")
                    unit.Obrisana = true;
                else
                    unit.Obrisana = false;
                ucitan.Jedinice.Add(unit);
            }

            sr.Close();

            return ucitan;
        }

    }
}