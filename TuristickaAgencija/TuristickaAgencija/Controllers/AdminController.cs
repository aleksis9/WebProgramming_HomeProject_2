using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TuristickaAgencija.Models;

namespace TuristickaAgencija.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            //spisak svih korisnika
            return View();
        }

        public ActionResult Sumnjivi()
        {
            return View();
        }

        public ActionResult Blokiraj()
        {
            string username = Request["username"];

            //izmeniti tog korisnika svugde
            foreach(Korisnik k in HomeController.turisti)
            {
                if(k.KorisnickoIme == username)
                {
                    k.Blokiran = true;
                }
            }

            HomeController.sviKorisnici = HomeController.admini.Concat(HomeController.menadzeri).ToList().Concat(HomeController.turisti).ToList();

            List<Komentar> komN = new List<Komentar>();
            foreach(Komentar k in HomeController.komentari)
            {
                if(k.Turista.KorisnickoIme == username)
                {
                    Korisnik novi = k.Turista;
                    novi.Blokiran = true;
                    Komentar nk = k;
                    nk.Turista = novi;
                    komN.Add(nk);
                }
                else
                {
                    komN.Add(k);
                }
            }

            List<Rezervacija> rezN = new List<Rezervacija>();
            foreach (Rezervacija r in HomeController.rezervacije)
            {
                if (r.Turista.KorisnickoIme == username)
                {
                    Korisnik novi = r.Turista;
                    novi.Blokiran = true;
                    Rezervacija nr = r;
                    nr.Turista = novi;
                    rezN.Add(nr);
                }
                else
                {
                    rezN.Add(r);
                }
            }

            HttpContext.Application["turisti"] = HomeController.turisti;
            HttpContext.Application["rezervacije"] = HomeController.rezervacije;
            HttpContext.Application["komentari"] = HomeController.komentari;

            //izmeniti u fajlu
            string path = Server.MapPath("~/App_Data/turisti.tsv");
            string temp = Path.GetTempFileName();

            StreamReader sr = new StreamReader(path);
            StreamWriter sw = new StreamWriter(temp);

            string line;
            while((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split('\t');
                if(parts[0] == username)
                {
                    string newline = line.Substring(0, line.Length - 5) + "True";
                    sw.WriteLine(newline);
                }
                else
                {
                    sw.WriteLine(line);
                }
            }

            sr.Close();
            sw.Close();

            System.IO.File.Delete(path);
            System.IO.File.Move(temp, path);

            return View("Index");
        }

        public ActionResult FilterK()
        {
            string ime, prezime, uloga;
            ime = Request["ime"];
            prezime = Request["prezime"];
            uloga = Request["uloga"];

            List<Korisnik> filter = new List<Korisnik>();
            foreach(Korisnik k in HomeController.sviKorisnici)
            {
                if(k.Ime.ToLower().Contains(ime.ToLower()) && k.Prezime.ToLower().Contains(prezime.ToLower()) && (k.Uloga.ToString() == uloga || uloga == "/"))
                {
                    filter.Add(k);
                }
            }

            string sortOpcija = Request["sortOpcija"];
            string sortNacin = Request["sortNacin"];

            if(sortOpcija == "ime")
            {
                if(sortNacin == "rastuce")
                {
                    filter = filter.OrderBy(x => x.Ime).ToList();
                }
                else
                {
                    filter = filter.OrderByDescending(x => x.Ime).ToList();
                }
            }
            else if(sortOpcija == "prezime")
            {
                if (sortNacin == "rastuce")
                {
                    filter = filter.OrderBy(x => x.Prezime).ToList();
                }
                else
                {
                    filter = filter.OrderByDescending(x => x.Prezime).ToList();
                }
            }
            else
            {
                if (sortNacin == "rastuce")
                {
                    filter = filter.OrderBy(x => x.Uloga.ToString()).ToList();
                }
                else
                {
                    filter = filter.OrderByDescending(x => x.Uloga.ToString()).ToList();
                }
            }

            Session["filterK"] = filter;

            return View("Index");
        }

        public ActionResult FilterS()
        {
            string ime, prezime, uloga;
            ime = Request["ime"];
            prezime = Request["prezime"];
            uloga = Request["uloga"];

            List<Korisnik> filter = new List<Korisnik>();
            foreach (Korisnik k in HomeController.sviKorisnici)
            {
                if (k.Ime.ToLower().Contains(ime.ToLower()) && k.Prezime.ToLower().Contains(prezime.ToLower()) && (k.Uloga.ToString() == uloga || uloga == "/"))
                {
                    filter.Add(k);
                }
            }

            string sortOpcija = Request["sortOpcija"];
            string sortNacin = Request["sortNacin"];

            if (sortOpcija == "ime")
            {
                if (sortNacin == "rastuce")
                {
                    filter = filter.OrderBy(x => x.Ime).ToList();
                }
                else
                {
                    filter = filter.OrderByDescending(x => x.Ime).ToList();
                }
            }
            else if (sortOpcija == "prezime")
            {
                if (sortNacin == "rastuce")
                {
                    filter = filter.OrderBy(x => x.Prezime).ToList();
                }
                else
                {
                    filter = filter.OrderByDescending(x => x.Prezime).ToList();
                }
            }
            else
            {
                if (sortNacin == "rastuce")
                {
                    filter = filter.OrderBy(x => x.Uloga.ToString()).ToList();
                }
                else
                {
                    filter = filter.OrderByDescending(x => x.Uloga.ToString()).ToList();
                }
            }

            Session["filterK"] = filter;

            return View("Sumnjivi");
        }

    }
}