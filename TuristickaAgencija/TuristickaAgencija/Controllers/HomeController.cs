using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using TuristickaAgencija.Models;

namespace TuristickaAgencija.Controllers
{
    public class HomeController : Controller
    {
        public static Korisnik login;
        public static List<Korisnik> admini;
        public static List<Korisnik> menadzeri;
        public static List<Korisnik> turisti;
        public static List<Korisnik> sviKorisnici;
        public static List<Aranzman> aranzmaniMenadzera;
        public static List<Komentar> komentari;
        public static List<Rezervacija> rezervacije;
        public static Korisnik trenutnoObradjivan;

        // GET: Home
        public ActionResult Index()
        {
            if (HttpContext.Application["admini"] == null)
            {
                admini = UcitajKorisnike("~/App_Data/admini.tsv");
                HttpContext.Application["admini"] = admini;
            }
            else
            {
                admini = (List<Korisnik>)HttpContext.Application["admini"];
            }

            if (HttpContext.Application["menadzeri"] == null)
            {
                menadzeri = UcitajKorisnike("~/App_Data/menadzeri.tsv");
                HttpContext.Application["menadzeri"] = menadzeri;
            }
            else
            {
                menadzeri = (List<Korisnik>)HttpContext.Application["menadzeri"];
            }

            if (HttpContext.Application["aranzmani"] == null)
            {
                aranzmaniMenadzera = new List<Aranzman>();
                foreach(List<Aranzman> lista in UcitajAranzmane().Values)
                {
                    foreach(Aranzman a in lista)
                    {
                        aranzmaniMenadzera.Add(a);
                    }

                }
                aranzmaniMenadzera = aranzmaniMenadzera.OrderBy(x => x.DatumPocetka).ToList();
                HttpContext.Application["aranzmani"] = aranzmaniMenadzera;
            }
            else
            {
                aranzmaniMenadzera = aranzmaniMenadzera.OrderBy(x => x.DatumPocetka).ToList();
                HttpContext.Application["aranzmani"] = aranzmaniMenadzera;
            }

            if (HttpContext.Application["rezervacije"] == null)
            {
                rezervacije = new List<Rezervacija>();
            }

            if (HttpContext.Application["turisti"] == null)
            {
                turisti = UcitajKorisnike("~/App_Data/turisti.tsv");
                HttpContext.Application["turisti"] = turisti;
            }
            else
            {
                turisti = (List<Korisnik>)HttpContext.Application["turisti"];
            }

            HttpContext.Application["rezervacije"] = rezervacije;

            if (HttpContext.Application["komentari"] == null)
            {
                komentari = UcitajKomentare("~/App_Data/komentari.tsv");
                HttpContext.Application["komentari"] = komentari;
            }
            else
            {
                komentari = (List<Komentar>)HttpContext.Application["komentari"];
            }

            if (sviKorisnici == null)
            {
                sviKorisnici = new List<Korisnik>();
            }
            sviKorisnici = admini.Concat(menadzeri).ToList().Concat(turisti).ToList();
            

            if (Session["login"] == null)
            {
                login = null;
            }
            else
            {
                login = (Korisnik)Session["login"];
            }

            Session["filter"] = null;

            return View();
        }

        public ActionResult Login()
        {
            if(Session["login"] == null && login == null)
            {
                return View();
            }

            Session["filter"] = null;

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            Session["filter"] = null;

            return View();
        }

        public ActionResult Logout()
        {
            string username = Request["username"];
            if(Session["login"] != null && login != null && login.LoggedIn == true)
            {
                login.LoggedIn = false;
                login = null;
                Session["login"] = null;
            }

            Session["filter"] = null;

            return RedirectToAction("Index", "Home");
        }

        private List<Korisnik> UcitajKorisnike(string path)
        {
            List<Korisnik> ucitani = new List<Korisnik>();

            var lokacija = Server.MapPath(path);

            StreamReader sr = new StreamReader(lokacija);

            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                Tuple<string, Korisnik> par = Korisnik.Parse(line);
                trenutnoObradjivan = par.Item2;
                if (par.Item2.Uloga == Enums.UlogaKorisnika.Turista)
                {
                    par.Item2.ListaRezervacija = Korisnik.UcitajListuRezervacija(par.Item1);
                    foreach (Rezervacija r in par.Item2.ListaRezervacija)
                    {
                        rezervacije.Add(r);
                    }
                }

                ucitani.Add(par.Item2);
            }
            sr.Close();

            return ucitani;
        }

        private List<Komentar> UcitajKomentare(string path)
        {
            List<Komentar> ucitani = new List<Komentar>();

            var lokacija = Server.MapPath(path);

            StreamReader sr = new StreamReader(lokacija);

            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                Komentar k = Komentar.Parse(line);
                ucitani.Add(k);
            }
            sr.Close();

            return ucitani;
        }

        private Dictionary<string, List<Aranzman>> UcitajAranzmane()
        {
            Dictionary<string, List<Aranzman>> ucitani = new Dictionary<string, List<Aranzman>>();

            List<Korisnik> menadzeri = (List<Korisnik>)HttpContext.Application["menadzeri"];
            foreach(Korisnik m in menadzeri)
            {
                List<Aranzman> list = new List<Aranzman>();
                foreach(Aranzman a in m.ListaAranzmana)
                {
                    list.Add(a);
                }
                ucitani.Add(m.KorisnickoIme, list);
            }

            return ucitani;
        }

        public ActionResult LoginClick()
        {
            string username = Request["korisnickoIme"];
            string lozinka = Request["lozinka"];

            bool flag = false;

            foreach(Korisnik k in sviKorisnici)
            {
                if(k.KorisnickoIme == username && k.Lozinka == lozinka)
                {
                    if (k.Blokiran)
                    {
                        TempData["message"] = "Vas nalog je blokiran! Ne mozete se ulogovati.";
                        return View("Login");
                    }
                    flag = true;
                    k.LoggedIn = true;
                    login = k;
                    Session["login"] = k;
                    break;
                }   
            }

            if (!flag)
            {
                TempData["message"] = "Pogresno korisnicko ime ili lozinka!";
                return RedirectToAction("Login", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Protekli()
        {
            Session["filter"] = null;

            return View();
        }

        public ActionResult DetaljiAranzmana()
        {
            foreach(Aranzman a in aranzmaniMenadzera)
            {
                if (a.Naziv == Request["name"])
                    ViewBag.Aranzman = a;
            }

            Session["filter"] = null;

            return View();
        }

        public ActionResult OpisAranzmana()
        {
            foreach (Aranzman a in aranzmaniMenadzera)
            {
                if (a.Naziv == Request["name"])
                    ViewBag.Aranzman = a;
            }

            Session["filter"] = null;

            return View();
        }

        public ActionResult ProgramAranzmana()
        {
            foreach (Aranzman a in aranzmaniMenadzera)
            {
                if (a.Naziv == Request["name"])
                    ViewBag.Aranzman = a;
            }

            Session["filter"] = null;

            return View();
        }

        public ActionResult DetaljiSmestaja()
        {
            foreach (Aranzman a in aranzmaniMenadzera)
            {
                if (a.Naziv == Request["name"])
                {
                    ViewBag.Smestaj = a.SmestajAranzmana;
                }
            }

            Session["filter"] = null;

            return View();
        }

        public ActionResult RegisterClick()
        {
            Korisnik k = new Korisnik();

            try
            {
                if (ValidateRegistration(Request))
                {
                    k = CreateKorisnik(Request);
                    if (k.Uloga == Enums.UlogaKorisnika.Menadzer)
                        menadzeri.Add(k);
                    else
                        turisti.Add(k);
                    HttpContext.Application["menadzeri"] = menadzeri;
                    HttpContext.Application["turisti"] = turisti;
                    TempData["message"] = "Uspesno ste dodali korisnika " + k.Ime + " " + k.Prezime;
                }
                else
                {
                    return RedirectToAction("Register", "Home");
                }
            }
            catch (Exception e)
            {

            }

            //upis u fajl sistem
            string registrovaniFullPath = "";
            if (k.Uloga == Enums.UlogaKorisnika.Menadzer)
                registrovaniFullPath = Server.MapPath("~/App_Data/menadzeri.tsv");
            else
            {
                registrovaniFullPath = Server.MapPath("~/App_Data/turisti.tsv");
            }
            string tempFile = Path.GetTempFileName();

            StreamReader sr = new StreamReader(registrovaniFullPath);
            StreamWriter sw = new StreamWriter(tempFile);

            string line;
            string userPath = k.KorisnickoIme.ToLower() + ".tsv";
            //korisnicko ime, lozinka, ime, prezime, pol, email, datum, uloga, naziv fajla sa listom aranzmana/rezervacija, obrisan
            string newLine = k.KorisnickoIme + "\t" + k.Lozinka + "\t" + k.Ime + "\t" + k.Prezime + "\t"
                + k.Pol + "\t" + k.Email + "\t" + k.DatumRodjenja.ToString("dd'/'MM'/'yyy") + "\t" + k.Uloga.ToString() + "\t" + userPath + "\t" + "0" + "\t" + "0";

            while ((line = sr.ReadLine()) != null)
            {
                sw.WriteLine(line);
            }
            sw.WriteLine(newLine);

            sr.Close();
            sw.Close();

            System.IO.File.Delete(registrovaniFullPath);
            System.IO.File.Move(tempFile, registrovaniFullPath);

            string newUserPath = k.KorisnickoIme.ToLower() + ".tsv";
            string newUserFullPath = "";
            if (k.Uloga == Enums.UlogaKorisnika.Menadzer)
                newUserFullPath = Server.MapPath("~/App_Data/Aranzmani/" + newUserPath);
            else
                newUserFullPath = Server.MapPath("~/App_Data/Rezervacije/" + newUserPath);

            System.IO.File.WriteAllText(newUserPath, String.Empty);


            return RedirectToAction("Index", "Home");
        }

        [NonAction]
        private bool ValidateRegistration(HttpRequestBase request)
        {
            string ime = request["ime"];
            string prezime = request["prezime"];
            string korisnickoIme = request["korisnickoIme"];
            string lozinka = request["lozinka"];
            string email = request["email"];
            string datum = request["datumRodjenja"];

            if (ime == null || ime == "")
            {
                TempData["message"] = "Unesite ime!";
                return false;
            }

            if (prezime == null || prezime == "")
            {
                TempData["message"] = "Unesite prezime!";
                return false;
            }

            if (korisnickoIme == null || korisnickoIme == "")
            {
                TempData["message"] = "Unesite korisnicko ime!";
                return false;
            }

            if (lozinka == null || lozinka == "")
            {
                TempData["message"] = "Unesite lozinku!";
                return false;
            }

            if (email == null || email == "")
            {
                TempData["message"] = "Unesite email!";
                return false;
            }

            if (datum == null || datum == "")
            {
                TempData["message"] = "Unesite datum!";
                return false;
            }

            //ova validacija nije zadata u postavci zadatka, ali je meni neophodna zbog upisa u fajl
            if (korisnickoIme.Split(' ').Count() != 1)
            {
                TempData["message"] = "Korisnicko ime ne sme da sadrzi razmak!";
                return false;
            }

            if(request["uloga"] == "Menadzer")
            {
                foreach (Korisnik k in menadzeri)
                    if (k.KorisnickoIme == korisnickoIme)
                    {
                        TempData["message"] = "Korisnicko ime vec postoji!";
                        return false;
                    }
            }
            else
            {
                foreach (Korisnik k in turisti)
                    if (k.KorisnickoIme == korisnickoIme)
                    {
                        TempData["message"] = "Korisnicko ime vec postoji!";
                        return false;
                    }
            }

            Regex rgx = new Regex("[A-Za-z0-9]");
            if (!rgx.IsMatch(korisnickoIme))
            {
                TempData["message"] = "Korisnicko ime moze da sadrzi samo slova i brojeve!";
                return false;
            }

            int dan = int.Parse(datum.Substring(0, 2));
            int mesec = int.Parse(datum.Substring(3, 2));
            int godina = int.Parse(datum.Substring(6, 4));

            if (godina < 1900 || godina > DateTime.Today.Year)
            {
                TempData["message"] = "Pogresan datum!";
                return false;
            }

            if (mesec < 1 || mesec > 12)
            {
                TempData["message"] = "Pogresan datum!";
                return false;
            }

            if (mesec == 1 || mesec == 3 || mesec == 5 || mesec == 7 || mesec == 8 || mesec == 10 || mesec == 12)
            {
                if (dan < 1 || dan > 31)
                {
                    TempData["message"] = "Pogresan datum!";
                    return false;
                }
            }
            else if (mesec == 4 || mesec == 6 || mesec == 9 || mesec == 11)
            {
                if (dan < 1 || dan > 30)
                {
                    TempData["message"] = "Pogresan datum!";
                    return false;
                }
            }
            else if (mesec == 2)
            {
                if (godina % 4 == 0)
                {
                    if (dan < 1 || dan > 29)
                    {
                        TempData["message"] = "Pogresan datum!";
                        return false;
                    }
                }
                else if (dan < 1 || dan > 28)
                {
                    TempData["message"] = "Pogresan datum!";
                    return false;
                }
            }

            return true;
        }

        [NonAction]
        private Korisnik CreateKorisnik(HttpRequestBase request)
        {
            string ime = request["ime"];
            string prezime = request["prezime"];
            string korisnickoIme = request["korisnickoIme"];
            string lozinka = request["lozinka"];
            string email = request["email"];
            string p = request["pol"];
            Enums.UlogaKorisnika uloga;
            if(request["uloga"] == "Menadzer")
            {
                uloga = Enums.UlogaKorisnika.Menadzer;
            }
            else
            {
                uloga = Enums.UlogaKorisnika.Turista;
            }

            int dan = Int32.Parse(Request["datumRodjenja"].Substring(0, 2));
            int mesec = Int32.Parse(Request["datumRodjenja"].Substring(3, 2));
            int godina = Int32.Parse(Request["datumRodjenja"].Substring(6, 4)); ;
            DateTime datum = new DateTime(godina, mesec, dan);

            Korisnik k = new Korisnik(korisnickoIme, lozinka, ime, prezime, p, email, datum, uloga, null, null, false, false, 0, false);

            return k;
        }

        public ActionResult KomentariAranzmana()
        {
            //naziv aranzmana za koji se prikazuju komentari
            ViewBag.Aranzman = Request["name"];
            Aranzman ja = new Aranzman();
            List<Komentar> komentariAranzmana = new List<Komentar>();
            foreach(Aranzman a in aranzmaniMenadzera)
            {
                if (a.Naziv == Request["name"])
                    ja = a;
            }
            foreach(Komentar k in komentari)
            {
                if (k.AranzmanKomentarisan == ja)
                    komentariAranzmana.Add(k);
            }

            ViewBag.Komentari = komentariAranzmana;

            Session["filter"] = null;

            return View();
        }

        public ActionResult ProfilKorisnika()
        {
            Session["filter"] = null;

            return View();
        }
        public ActionResult IzmeniProfil()
        {
            return View();
        }

        public ActionResult IzmeniProfilClick()
        {
            Korisnik stari = (Korisnik)Session["login"];
            Korisnik novi = new Korisnik();
            if (!ValidateIzmenaProfila(Request))
            {
                return RedirectToAction("IzmeniProfil", "Home");
            }
            novi = CreateKorisnik(Request);
            novi.KorisnickoIme = stari.KorisnickoIme;
            novi.Uloga = stari.Uloga;
            novi.ListaAranzmana = stari.ListaAranzmana;
            novi.ListaRezervacija = stari.ListaRezervacija;
            novi.Deleted = false;
            novi.LoggedIn = true;
            novi.BrojOtkazivanja = stari.BrojOtkazivanja;
            novi.Blokiran = stari.Blokiran;

            //prepisati novog korisnika preko starog
            List<Korisnik> korisnici = new List<Korisnik>();
            if (novi.Uloga == Enums.UlogaKorisnika.Administrator)
            {
                foreach(Korisnik k in admini)
                {
                    if(k.KorisnickoIme == novi.KorisnickoIme)
                    {
                        korisnici.Add(novi);
                    }
                    else
                    {
                        korisnici.Add(k);
                    }
                }
                HttpContext.Application["admini"] = korisnici;
                admini = korisnici;
            }
            else if(novi.Uloga == Enums.UlogaKorisnika.Menadzer)
            {
                foreach (Korisnik k in menadzeri)
                {
                    if (k.KorisnickoIme == novi.KorisnickoIme)
                    {
                        korisnici.Add(novi);
                    }
                    else
                    {
                        korisnici.Add(k);
                    }
                }
                HttpContext.Application["menadzeri"] = korisnici;
                menadzeri = korisnici;
            }
            else
            {
                foreach (Korisnik k in turisti)
                {
                    if (k.KorisnickoIme == novi.KorisnickoIme)
                    {
                        korisnici.Add(novi);
                    }
                    else
                    {
                        korisnici.Add(k);
                    }
                }
                HttpContext.Application["turisti"] = korisnici;
                turisti = korisnici;
            }

            //promeniti korisnika u rezervacijama i komentarima
            if (novi.ListaRezervacija != null)
            {
                //samo za turiste
                foreach (Rezervacija r in novi.ListaRezervacija)
                {
                    r.Turista = novi;
                }
            }

            foreach(Komentar k in komentari)
            {
                if(k.Turista == stari)
                {
                    k.Turista = novi;
                }
            }

            //promeniti zapis u tsv fajlu
            string path = "";
            if(novi.Uloga == Enums.UlogaKorisnika.Administrator)
            {
                path = "admini.tsv";
            }
            else if(novi.Uloga == Enums.UlogaKorisnika.Menadzer)
            {
                path = "menadzeri.tsv";
            }
            else
            {
                path = "turisti.tsv";
            }
            string fullPath = Server.MapPath("~/App_Data/" + path);

            string line = "";
            string newLine = novi.KorisnickoIme + "\t" + novi.Lozinka + "\t" + novi.Ime + "\t" + novi.Prezime + "\t" + novi.Pol + "\t" + novi.Email + "\t" +
                novi.DatumRodjenja.ToString("dd'/'MM'/'yyyy") + "\t" + novi.Uloga.ToString() + "\t";
            if(novi.Uloga == Enums.UlogaKorisnika.Administrator)
            {
                newLine += "null";
            }
            else
            {
                newLine += novi.KorisnickoIme.ToLower() + ".tsv";
            }
            newLine += "\t" + novi.BrojOtkazivanja.ToString() + "\t" + "0" + "\t" + novi.Blokiran.ToString();

            string[] parts;
            string tempFile = Path.GetTempFileName();

            StreamReader sr = new StreamReader(fullPath);
            StreamWriter sw = new StreamWriter(tempFile);

            while ((line = sr.ReadLine()) != null)
            {
                parts = line.Split('\t');
                if (novi.KorisnickoIme == parts[0])
                {
                    //umesto ovog reda prepisati nove podatke
                    sw.WriteLine(newLine);
                }
                else
                    sw.WriteLine(line);
            }

            sr.Close();
            sw.Close();

            System.IO.File.Delete(fullPath);
            System.IO.File.Move(tempFile, fullPath);

            Session["login"] = novi;

            return RedirectToAction("ProfilKorisnika", "Home");
        }

        [NonAction]
        private bool ValidateIzmenaProfila(HttpRequestBase request)
        {
            string ime = request["ime"];
            string prezime = request["prezime"];
            string lozinka = request["lozinka"];
            string email = request["email"];
            string datum = request["datumRodjenja"];

            if (ime == null || ime == "")
            {
                TempData["message"] = "Unesite ime!";
                return false;
            }

            if (prezime == null || prezime == "")
            {
                TempData["message"] = "Unesite prezime!";
                return false;
            }

            if (lozinka == null || lozinka == "")
            {
                TempData["message"] = "Unesite lozinku!";
                return false;
            }

            if (email == null || email == "")
            {
                TempData["message"] = "Unesite email!";
                return false;
            }

            if (datum == null || datum == "")
            {
                TempData["message"] = "Unesite datum!";
                return false;
            }

            int dan = int.Parse(datum.Substring(0, 2));
            int mesec = int.Parse(datum.Substring(3, 2));
            int godina = int.Parse(datum.Substring(6, 4));

            if (godina < 1900 || godina > DateTime.Today.Year)
            {
                TempData["message"] = "Pogresan datum!";
                return false;
            }

            if (mesec < 1 || mesec > 12)
            {
                TempData["message"] = "Pogresan datum!";
                return false;
            }

            if (mesec == 1 || mesec == 3 || mesec == 5 || mesec == 7 || mesec == 8 || mesec == 10 || mesec == 12)
            {
                if (dan < 1 || dan > 31)
                {
                    TempData["message"] = "Pogresan datum!";
                    return false;
                }
            }
            else if (mesec == 4 || mesec == 6 || mesec == 9 || mesec == 11)
            {
                if (dan < 1 || dan > 30)
                {
                    TempData["message"] = "Pogresan datum!";
                    return false;
                }
            }
            else if (mesec == 2)
            {
                if (godina % 4 == 0)
                {
                    if (dan < 1 || dan > 29)
                    {
                        TempData["message"] = "Pogresan datum!";
                        return false;
                    }
                }
                else if (dan < 1 || dan > 28)
                {
                    TempData["message"] = "Pogresan datum!";
                    return false;
                }
            }

            return true;
        }

        public ActionResult FilterA()
        {
            string datumPocetkaOd = Request["datumPocetkaOd"];
            string datumPocetkaDo = Request["datumPocetkaDo"];
            string datumKrajaOd = Request["datumKrajaOd"];
            string datumKrajaDo = Request["datumKrajaDo"];
            string tipPrevoza = Request["tipPrevoza"];
            string tipAranzmana = Request["tipAranzmana"];
            string naziv = Request["naziv"];
            string page = Request["page"];
            try
            {
                DateTime datumPocetkaOdDT, datumPocetkaDoDT, datumKrajaOdDT, datumKrajaDoDT;
                if (datumPocetkaOd != "")
                {
                    string[] p1 = datumPocetkaOd.Split('/');
                    datumPocetkaOdDT = new DateTime(int.Parse(p1[0]), int.Parse(p1[1]), int.Parse(p1[2]));
                }
                else
                {
                    datumPocetkaOdDT = new DateTime(1900, 1, 1);
                }
                if (datumPocetkaDo != "")
                {
                    string[] p2 = datumPocetkaDo.Split('/');
                    datumPocetkaDoDT = new DateTime(int.Parse(p2[0]), int.Parse(p2[1]), int.Parse(p2[2]));
                }
                else
                {
                    datumPocetkaDoDT = new DateTime(2100, 1, 1);
                }
                if (datumKrajaOd != "")
                {
                    string[] p3 = datumKrajaOd.Split('/');
                    datumKrajaOdDT = new DateTime(int.Parse(p3[0]), int.Parse(p3[1]), int.Parse(p3[2]));
                }
                else
                {
                    datumKrajaOdDT = new DateTime(1900, 1, 1);
                }
                if (datumKrajaDo != "")
                {
                    string[] p4 = datumKrajaDo.Split('/');
                    datumKrajaDoDT = new DateTime(int.Parse(p4[0]), int.Parse(p4[1]), int.Parse(p4[2]));
                }
                else
                {
                    datumKrajaDoDT = new DateTime(2100, 1, 1);
                }

                if(tipPrevoza == "/")
                {
                    tipPrevoza = "";
                }
                if(tipAranzmana == "/")
                {
                    tipAranzmana = "";
                }

                List<Aranzman> filter = new List<Aranzman>();
                foreach (Aranzman a in HomeController.aranzmaniMenadzera)
                {
                    if (a.Naziv.ToLower().Contains(naziv.ToLower()) && a.DatumPocetka >= datumPocetkaOdDT && a.DatumPocetka <= datumPocetkaDoDT && a.DatumZavrsetka >= datumKrajaOdDT &&
                        a.DatumZavrsetka <= datumKrajaDoDT)
                    {
                        if(tipPrevoza == "")
                        {
                            if(tipAranzmana == "")
                            {
                                filter.Add(a);
                            }
                            else
                            {
                                if(a.TipAranzmana == tipAranzmana)
                                {
                                    filter.Add(a);
                                }
                            }
                        }
                        else
                        {
                            if (tipAranzmana == "")
                            {
                                if(a.TipPrevoza == tipPrevoza)
                                {
                                    filter.Add(a);
                                }
                            }
                            else
                            {
                                if(a.TipAranzmana == tipAranzmana && a.TipPrevoza == tipPrevoza)
                                {
                                    filter.Add(a);
                                }
                            }
                        }
                    }
                }

                string sortOpcija = Request["sortOpcija"];
                string sortNacin = Request["sortNacin"];

                if(sortOpcija == "naziv")
                {
                    if(sortNacin == "rastuce")
                    {
                        filter = filter.OrderBy(o => o.Naziv).ToList();
                    }
                    else
                    {
                        filter = filter.OrderByDescending(o => o.Naziv).ToList();
                    }
                }
                else if(sortOpcija == "datumPocetka")
                {
                    if (sortNacin == "rastuce")
                    {
                        filter = filter.OrderBy(o => o.DatumPocetka).ToList();
                    }
                    else
                    {
                        filter = filter.OrderByDescending(o => o.DatumPocetka).ToList();
                    }
                }
                else
                {
                    if (sortNacin == "rastuce")
                    {
                        filter = filter.OrderBy(o => o.DatumZavrsetka).ToList();
                    }
                    else
                    {
                        filter = filter.OrderByDescending(o => o.DatumZavrsetka).ToList();
                    }
                }

                Session["filter"] = filter;

            }
            catch
            {
                TempData["message"] = "Podaci pretrage/sortiranja uneti u pogresnom formatu!";
                Session["filter"] = null;
                if(page == "index")
                {
                    return View("Index");
                }
                if(page == "protekli")
                {
                    return View("Protekli");
                }
            }

            if (page == "index")
            {
                return View("Index");
            }
            if (page == "protekli")
            {
                return View("Protekli");
            }

            return View("Login");

        }

        public ActionResult FilterS()
        {
            string tipSmestaja = Request["tipSmestaja"];
            string bazen = Request["bazen"];
            string spa = Request["spa"];
            string invalidi = Request["invalidi"];
            string wifi = Request["wifi"];
            string naziv = Request["naziv"];

            List<Aranzman> filter = new List<Aranzman>();
            foreach (Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if (a.SmestajAranzmana.Naziv.ToLower().Contains(naziv.ToLower()))
                {
                    bool good = true;

                    if (a.SmestajAranzmana.Obrisan)
                    {
                        good = false;
                    }

                    if (bazen == "DA" && !a.SmestajAranzmana.Bazen)
                    {
                        good = false;
                    }
                    if (bazen == "NE" && a.SmestajAranzmana.Bazen)
                    {
                        good = false;
                    }

                    if (spa == "DA" && !a.SmestajAranzmana.SpaCentar)
                    {
                        good = false;
                    }
                    if (spa == "NE" && a.SmestajAranzmana.SpaCentar)
                    {
                        good = false;
                    }

                    if (invalidi == "DA" && !a.SmestajAranzmana.ZaOsobeSaInvaliditetom)
                    {
                        good = false;
                    }
                    if (invalidi == "NE" && a.SmestajAranzmana.ZaOsobeSaInvaliditetom)
                    {
                        good = false;
                    }

                    if (wifi == "DA" && !a.SmestajAranzmana.WiFi)
                    {
                        good = false;
                    }
                    if (wifi == "NE" && a.SmestajAranzmana.WiFi)
                    {
                        good = false;
                    }

                    if (tipSmestaja != "/" && a.SmestajAranzmana.TipSmestaja != tipSmestaja)
                    {
                        good = false;
                    }


                    if (good)
                    {
                        filter.Add(a);
                    }
                }
            }

            string sortOpcija = Request["sortOpcija"];
            string sortNacin = Request["sortNacin"];

            if (sortOpcija == "naziv")
            {
                if (sortNacin == "rastuce")
                {
                    filter = filter.OrderBy(o => o.SmestajAranzmana.Naziv).ToList();
                }
                else
                {
                    filter = filter.OrderByDescending(o => o.SmestajAranzmana.Naziv).ToList();
                }
            }
            else if (sortOpcija == "brojJedinica")
            {
                if (sortNacin == "rastuce")
                {
                    filter = filter.OrderBy(o => o.SmestajAranzmana.Jedinice.Count).ToList();
                }
                else
                {
                    filter = filter.OrderByDescending(o => o.SmestajAranzmana.Jedinice.Count).ToList();
                }
            }
            else
            {
                foreach(Aranzman a in filter)
                {
                    int slobodne = 0;
                    foreach (SmestajnaJedinica j in a.SmestajAranzmana.Jedinice)
                    {
                        if (!a.SmestajAranzmana.Obrisan && !a.Obrisan)
                            if (!j.Rezervisana && !j.Obrisana)
                                slobodne++;
                    }
                    a.SmestajAranzmana.BrojSlobodnih = slobodne;
                }

                if (sortNacin == "rastuce")
                {
                    filter = filter.OrderBy(o => o.SmestajAranzmana.BrojSlobodnih).ToList();
                }
                else
                {
                    filter = filter.OrderByDescending(o => o.SmestajAranzmana.BrojSlobodnih).ToList();
                }
            }


            Session["filter"] = filter;

            string page = Request["page"];

            if (page == "index")
            {
                return View("Index");
            }
            if (page == "protekli")
            {
                return View("Protekli");
            }

            return View("Login");
        }

        public ActionResult FilterJ()
        {
            string brojOd = Request["brojOd"];
            string brojDo = Request["brojDo"];
            string cenaOd = Request["cenaOd"];
            string cenaDo = Request["cenaDo"];
            string ljubimci = Request["ljubimci"];

            int brojOdI, brojDoI;
            double cenaOdD, cenaDoD;

            if (brojOd == "")
                brojOdI = 0;
            else
                Int32.TryParse(brojOd, out brojOdI);

            if (brojDo == "")
                brojDoI = 214748364;
            else
                Int32.TryParse(brojDo, out brojDoI);

            if (cenaOd == "")
                cenaOdD = 0;
            else
                double.TryParse(cenaOd, out cenaOdD);

            if (cenaDo == "")
                cenaDoD = 214748364;
            else
                double.TryParse(cenaDo, out cenaDoD);

            string aranzman = Request["aranzman"];

            List<SmestajnaJedinica> jedinice = new List<SmestajnaJedinica>();
            Aranzman mojA = new Aranzman();
            foreach (Aranzman a in aranzmaniMenadzera)
            {
                if(a.Naziv == aranzman)
                {
                    foreach(SmestajnaJedinica j in a.SmestajAranzmana.Jedinice)
                    {
                        if(j.CenaZaCeluJedinicu >= cenaOdD && j.CenaZaCeluJedinicu <= cenaDoD && j.MaxBrojGostiju >= brojOdI && j.MaxBrojGostiju <= brojDoI)
                        {
                            if(ljubimci == "/" || (ljubimci == "DA" && j.DozvoljeniKucniLjubimci) || (ljubimci == "NE" && !j.DozvoljeniKucniLjubimci))
                            {
                                jedinice.Add(j);
                            }
                        }
                    }
                    mojA = a;

                    break;
                }
            }

            Session["aranzman"] = mojA;
            Smestaj s = new Smestaj(mojA.SmestajAranzmana.Naziv, mojA.SmestajAranzmana.TipSmestaja, mojA.SmestajAranzmana.BrojZvezdica, mojA.SmestajAranzmana.Bazen,
                mojA.SmestajAranzmana.SpaCentar, mojA.SmestajAranzmana.ZaOsobeSaInvaliditetom, mojA.SmestajAranzmana.WiFi, null, mojA.Obrisan);
            s.Jedinice = jedinice;
            ViewBag.Smestaj = s;

            string sortOpcija = Request["sortOpcija"];
            string sortNacin = Request["sortNacin"];

            if(sortOpcija == "broj")
            {
                if(sortNacin == "rastuce")
                {
                    s.Jedinice = s.Jedinice.OrderBy(x => x.MaxBrojGostiju).ToList();
                }
                else
                {
                    s.Jedinice = s.Jedinice.OrderByDescending(x => x.MaxBrojGostiju).ToList();
                }
            }
            else
            {
                if (sortNacin == "rastuce")
                {
                    s.Jedinice = s.Jedinice.OrderBy(x => x.CenaZaCeluJedinicu).ToList();
                }
                else
                {
                    s.Jedinice = s.Jedinice.OrderByDescending(x => x.CenaZaCeluJedinicu).ToList();
                }
            }

            return View("DetaljiSmestaja");
        }

    }
}