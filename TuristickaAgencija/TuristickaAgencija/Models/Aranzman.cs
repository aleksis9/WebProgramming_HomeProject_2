using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using static TuristickaAgencija.Models.Enums;

namespace TuristickaAgencija.Models
{
    public class Aranzman
    {
        public string Naziv { get; set; }
        public string TipAranzmana { get; set; }
        public string TipPrevoza { get; set; }
        public string Lokacija { get; set; }
        public DateTime DatumPocetka { get; set; }
        public DateTime DatumZavrsetka { get; set; }
        public MestoNalazenja MestoPolaska { get; set; }
        public string VremePolaska { get; set; }
        public int MaxBrojPutnika { get; set; }
        public string OpisAranzmana { get; set; }
        public string ProgramPutovanja { get; set; }
        public string PosterAranzmana { get; set; }
        public Smestaj SmestajAranzmana { get; set; }
        public bool Obrisan { get; set; }

        public Aranzman() { }

        public Aranzman(string naziv, string tipAranzmana, string tipPrevoza, string lokacija, DateTime datumPocetka, DateTime datumZavrsetka,
            MestoNalazenja mestoPolaska, string vremePolaska, int maxBrojPutnika, string opisAranzmana, string programPutovanja, string posterAranzmana, Smestaj smestajAranzmana, bool obrisan)
        {
            Naziv = naziv;
            TipAranzmana = tipAranzmana;
            TipPrevoza = tipPrevoza;
            Lokacija = lokacija;
            DatumPocetka = datumPocetka;
            DatumZavrsetka = datumZavrsetka;
            MestoPolaska = mestoPolaska;
            VremePolaska = vremePolaska;
            MaxBrojPutnika = maxBrojPutnika;
            OpisAranzmana = opisAranzmana;
            ProgramPutovanja = programPutovanja;
            PosterAranzmana = posterAranzmana;
            SmestajAranzmana = smestajAranzmana;
            Obrisan = obrisan;
        }

        public static Aranzman Parse(string line)
        {
            Aranzman ucitan = new Aranzman();

            string[] parts = line.Split('\t');
            ucitan.Naziv = parts[0];
            ucitan.TipAranzmana = parts[1];
            ucitan.TipPrevoza = parts[2];
            ucitan.Lokacija = parts[3];
            string[] datumDelovi = parts[4].Split('/');
            ucitan.DatumPocetka = new DateTime(Int32.Parse(datumDelovi[2]), Int32.Parse(datumDelovi[1]), Int32.Parse(datumDelovi[0]));
            datumDelovi = parts[5].Split('/');
            ucitan.DatumZavrsetka = new DateTime(Int32.Parse(datumDelovi[2]), Int32.Parse(datumDelovi[1]), Int32.Parse(datumDelovi[0]));            
            ucitan.MestoPolaska = MestoNalazenja.Parse(parts[6]);
            ucitan.VremePolaska = parts[7];
            ucitan.MaxBrojPutnika = Int32.Parse(parts[8]);
            ucitan.OpisAranzmana = Aranzman.GetOpis(parts[9]);
            ucitan.ProgramPutovanja = Aranzman.GetProgram(parts[10]);
            ucitan.PosterAranzmana = parts[11];
            ucitan.SmestajAranzmana = Smestaj.Parse(parts[12]);
            if(parts[13] == "True")
            {
                ucitan.Obrisan = true;
            }
            else
            {
                ucitan.Obrisan = false;
            }

            return ucitan;
        }

        public static string GetOpis(string path)
        {
            string ucitano = "";

            string lokacija = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\Opisi\\" + path;

            StreamReader sr = new StreamReader(lokacija);

            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                ucitano += line;
            }

            sr.Close();

            return ucitano;
        }

        public static string GetProgram(string path)
        {
            string ucitano = "";

            string lokacija = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\Programi\\" + path;

            StreamReader sr = new StreamReader(lokacija);

            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                ucitano += line;
            }

            sr.Close();

            return ucitano;
        }

    }
}