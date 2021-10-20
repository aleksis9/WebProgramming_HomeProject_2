using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuristickaAgencija.Models
{
    public class Rezervacija
    {
        public string Id { get; set; }
        public Korisnik Turista { get; set; }
        public bool StatusAktivna { get; set; }
        public Aranzman IzabraniAranzman { get; set; }
        public SmestajnaJedinica Jedinica { get; set; }

        public Rezervacija() { }

        public Rezervacija(string id, Korisnik turista, bool statusAktivna, Aranzman izabraniAranzman, SmestajnaJedinica jedinica)
        {
            Id = id;
            Turista = turista;
            StatusAktivna = statusAktivna;
            IzabraniAranzman = izabraniAranzman;
            Jedinica = jedinica;
        }

        public static Rezervacija Parse(string line)
        {
            Rezervacija nova = new Rezervacija();

            string[] parts = line.Split('\t');
            nova.Id = parts[0];
            if (parts[2] == "True")
                nova.StatusAktivna = true;
            else
                nova.StatusAktivna = false;
            nova.Turista = Controllers.HomeController.trenutnoObradjivan;
            foreach (Aranzman a in Controllers.HomeController.aranzmaniMenadzera)
            {
                if (a.Naziv == parts[3])
                {
                    nova.IzabraniAranzman = a;
                    foreach(SmestajnaJedinica j in a.SmestajAranzmana.Jedinice)
                    {
                        if(j.MaxBrojGostiju == Int32.Parse(nova.Id[14].ToString()))
                        {
                            //j.Rezervisana = true;
                            nova.Jedinica = j;
                            break;
                        }
                    }
                    break;
                }
            }

            return nova;
        }
    }
}