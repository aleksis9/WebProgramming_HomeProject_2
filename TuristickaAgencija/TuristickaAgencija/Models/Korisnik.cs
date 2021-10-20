using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using static TuristickaAgencija.Models.Enums;

namespace TuristickaAgencija.Models
{
    public class Korisnik
    {
        public string KorisnickoIme { get; set; }
        public string Lozinka { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Pol { get; set; }
        public string Email { get; set; }
        public DateTime DatumRodjenja { get; set; }
        public UlogaKorisnika Uloga { get; set; }
        public List<Aranzman> ListaAranzmana { get; set; }
        public List<Rezervacija> ListaRezervacija { get; set; }
        public bool LoggedIn { get; set; }
        public bool Deleted { get; set; }
        public int BrojOtkazivanja { get; set; }
        public bool Blokiran { get; set; }

        public Korisnik() { }

        public Korisnik(string korisnickoIme, string lozinka, string ime, string prezime, string pol, string email, DateTime datumRodjenja, UlogaKorisnika uloga, List<Aranzman> listaAranzmana, List<Rezervacija> listaRezervacija , bool loggedIn, bool deleted, int brojOtkazivanja, bool blokiran)
        {
            KorisnickoIme = korisnickoIme;
            Lozinka = lozinka;
            Ime = ime;
            Prezime = prezime;
            Pol = pol;
            Email = email;
            DatumRodjenja = datumRodjenja;
            Uloga = uloga;
            ListaAranzmana = listaAranzmana;
            ListaRezervacija = listaRezervacija;
            LoggedIn = loggedIn;
            Deleted = deleted;
            BrojOtkazivanja = brojOtkazivanja;
            Blokiran = blokiran;
        }

        public static Tuple<string, Korisnik> Parse(string s)
        {
            Tuple<string, Korisnik> par;

            string[] tokens = s.Split('\t');
            string username = tokens[0];
            string lozinka = tokens[1];
            string ime = tokens[2];
            string prezime = tokens[3];
            string pol = tokens[4];
            string email = tokens[5];
            string[] datumDelovi = tokens[6].Split('/');
            DateTime datum = new DateTime(int.Parse(datumDelovi[2]), int.Parse(datumDelovi[1]), int.Parse(datumDelovi[0]));
            UlogaKorisnika uloga;
            if(tokens[7] == "Administrator")
            {
                uloga = UlogaKorisnika.Administrator;
            }
            else if(tokens[7] == "Menadzer")
            {
                uloga = UlogaKorisnika.Menadzer;
            }
            else
            {
                uloga = UlogaKorisnika.Turista;
            }
            string putanjaDoListe = tokens[8];
            int obrisan = Int32.Parse(tokens[9]);
            bool del;
            if (obrisan == 1)
                del = true;
            else
                del = false;

            List<Aranzman> listaA = null;
            List<Rezervacija> listaR = null;

            if (putanjaDoListe != "null")
            {
                if (uloga == UlogaKorisnika.Menadzer)
                {
                    listaA = UcitajListuAranzmana(putanjaDoListe);
                }
            }

            int brojOtkaza = Int32.Parse(tokens[10]);
            string blok = tokens[11];
            bool blokiran = false;
            if(blok == "True")
            {
                blokiran = true;
            }


            Korisnik p = new Korisnik(username, lozinka, ime, prezime, pol, email, datum, uloga, listaA, listaR, false, del, brojOtkaza, blokiran);

            par = new Tuple<string, Korisnik>(putanjaDoListe, p);

            return par;
        }

        public static List<Aranzman> UcitajListuAranzmana(string path)
        {
            List<Aranzman> ucitani = new List<Aranzman>();

            string lokacija = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\Aranzmani\\" + path;

            StreamReader sr = new StreamReader(lokacija);

            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                Aranzman a = Aranzman.Parse(line);
                ucitani.Add(a);
            }
            sr.Close();

            return ucitani;
        }

        public static List<Rezervacija> UcitajListuRezervacija(string path)
        {
            List<Rezervacija> lista = new List<Rezervacija>();

            string lokacija = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\Rezervacije\\" + path;

            StreamReader sr = new StreamReader(lokacija);

            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                Rezervacija r = Rezervacija.Parse(line);
                lista.Add(r);
            }
            sr.Close();

            return lista;
        }

    }
}