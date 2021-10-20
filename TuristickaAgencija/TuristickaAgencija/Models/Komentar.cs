using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuristickaAgencija.Models
{
    public class Komentar
    {
        public Korisnik Turista { get; set; }
        public Aranzman AranzmanKomentarisan { get; set; }
        public string Tekst { get; set; }
        public int Ocena { get; set; }
        public string Odobren { get; set; }

        public Komentar() { }

        public Komentar(Korisnik turista, Aranzman aranzmanKomentarisan, string tekst, int ocena)
        {
            Turista = turista;
            AranzmanKomentarisan = aranzmanKomentarisan;
            Tekst = tekst;
            Ocena = ocena;
            Odobren = "Ceka";
        }

        public static Komentar Parse(string line)
        {
            string[] parts = line.Split('\t');
            Komentar k = new Komentar();

            foreach(Korisnik t in Controllers.HomeController.turisti)
            {
                if (t.KorisnickoIme == parts[0])
                    k.Turista = t;
            }

            foreach (Aranzman a in Controllers.HomeController.aranzmaniMenadzera)
            {
                if (a.Naziv == parts[1])
                    k.AranzmanKomentarisan = a;
            }

            k.Tekst = parts[2];
            k.Ocena = Int32.Parse(parts[3]);
            k.Odobren = parts[4];

            return k;
        }
    }
}