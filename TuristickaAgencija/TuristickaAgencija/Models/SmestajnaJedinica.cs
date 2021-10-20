using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuristickaAgencija.Models
{
    public class SmestajnaJedinica
    {
        public int MaxBrojGostiju { get; set; }
        public bool DozvoljeniKucniLjubimci { get; set; }
        public double CenaZaCeluJedinicu { get; set; }
        public bool Rezervisana { get; set; }
        public bool Obrisana { get; set; }

        public SmestajnaJedinica() { }

        public SmestajnaJedinica(int maxBrojGostiju, bool dozvoljeniKucniLjubimci, double cenaZaCeluJedinicu, bool rezervisana, bool obrisana)
        {
            MaxBrojGostiju = maxBrojGostiju;
            DozvoljeniKucniLjubimci = dozvoljeniKucniLjubimci;
            CenaZaCeluJedinicu = cenaZaCeluJedinicu;
            Rezervisana = rezervisana;
            Obrisana = obrisana;
        }
    }
}