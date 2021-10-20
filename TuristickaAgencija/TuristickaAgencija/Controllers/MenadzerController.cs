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
    public class MenadzerController : Controller
    {
        public static Aranzman trenutnoKreirani;

        // GET: Menadzer
        public ActionResult Index()
        {
            Session["filter"] = null;

            return View();
        }

        public static void OdobriKomentare()
        {
            //izmeniti polje odobren u komentaru

            //HomeController.komentari.Add(novi);
            //da li dodati novi ili je on vec dodat proveri

            //upisi u fajl komentar


        }

        public ActionResult Aranzmani()
        {
            return RedirectToAction("Index");
        }

        public ActionResult DetaljiAranzmana()
        {
            string naziv = Request["name"];
            foreach(Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if(a.Naziv == naziv)
                {
                    ViewBag.Aranzman = a;
                    Session["aranzman"] = a;
                }
            }
            return View();
        }

        public ActionResult DodajAranzman()
        {
            trenutnoKreirani = new Aranzman();
            return View();
        }

        [HttpPost]
        public ActionResult DodajAranzmanClick(HttpPostedFileBase poster)
        {
            trenutnoKreirani = new Aranzman();
            if (!ValidateAranzman(Request))
            {
                return RedirectToAction("DodajAranzman");
            }

            string naziv = Request["naziv"];
            string tipAranzmana = Request["tipAranzmana"];
            string tipPrevoza = Request["tipPrevoza"];
            string lokacija = Request["lokacija"];
            string datumPocetka = Request["datumPocetka"];
            string datumKraja = Request["datumKraja"];
            string mesto = Request["mesto"];
            string putnici = Request["putnici"];
            string vreme = Request["vreme"];
            string opis = Request["opis"];
            string program = Request["program"];
            try
            {
                string posterPath = Path.Combine(Server.MapPath("~/Images/"), poster.FileName);
                poster.SaveAs(posterPath);
            }
            catch(Exception e)
            {
                TempData["message"] = "Morate izabrati poster aranzmana!";
                return RedirectToAction("DodajAranzman");
            }

            string[] deloviDP = datumPocetka.Split('/');
            string[] deloviDK = datumKraja.Split('/');

            DateTime datumPocetkaDT = new DateTime(int.Parse(deloviDP[2]), int.Parse(deloviDP[1]), int.Parse(deloviDP[0]));
            DateTime datumKrajaDT = new DateTime(int.Parse(deloviDK[2]), int.Parse(deloviDK[1]), int.Parse(deloviDK[0]));

            string[] delovi = mesto.Split('-');
            string adresa = delovi[0];
            double geoDuz = double.Parse(delovi[1]);
            double geoSir = double.Parse(delovi[2]);

            trenutnoKreirani.Naziv = naziv;
            trenutnoKreirani.TipAranzmana = tipAranzmana;
            trenutnoKreirani.TipPrevoza = tipPrevoza;
            trenutnoKreirani.Lokacija = lokacija;
            trenutnoKreirani.VremePolaska = vreme;
            trenutnoKreirani.DatumPocetka = datumPocetkaDT;
            trenutnoKreirani.DatumZavrsetka = datumKrajaDT;
            trenutnoKreirani.MestoPolaska = new MestoNalazenja(adresa, geoDuz, geoSir);
            trenutnoKreirani.OpisAranzmana = opis;
            trenutnoKreirani.ProgramPutovanja = program;
            trenutnoKreirani.PosterAranzmana = poster.FileName;
            trenutnoKreirani.MaxBrojPutnika = Int32.Parse(putnici);
            trenutnoKreirani.Obrisan = false;
            trenutnoKreirani.SmestajAranzmana = new Smestaj();

            return View();
        }

        [HttpPost]
        public ActionResult DodajAranzmanSmestajClick()
        {
            if (!ValidateSmestaj(Request))
            {
                return View("DodajAranzmanClick");
            }

            //dodat je smestaj i sad njegove podatke ubaciti
            trenutnoKreirani.SmestajAranzmana.Naziv = Request["naziv"];
            trenutnoKreirani.SmestajAranzmana.TipSmestaja = Request["tipSmestaja"];
            trenutnoKreirani.SmestajAranzmana.BrojZvezdica = Int32.Parse(Request["zvezdice"]);
            trenutnoKreirani.SmestajAranzmana.Obrisan = false;
            bool bazen, spa, invalidi, wifi;

            if (Request["bazen"].ToLower() == "da")
                bazen = true;
            else
                bazen = false;
            if (Request["spa"].ToLower() == "da")
                spa = true;
            else
                spa = false;
            if (Request["invalidi"].ToLower() == "da")
                invalidi = true;
            else
                invalidi = false;
            if (Request["wifi"].ToLower() == "da")
                wifi = true;
            else
                wifi = false;

            trenutnoKreirani.SmestajAranzmana.Bazen = bazen;
            trenutnoKreirani.SmestajAranzmana.SpaCentar = spa;
            trenutnoKreirani.SmestajAranzmana.ZaOsobeSaInvaliditetom = invalidi;
            trenutnoKreirani.SmestajAranzmana.WiFi = wifi;
            trenutnoKreirani.SmestajAranzmana.Jedinice = new List<SmestajnaJedinica>();

            return View();
        }

        [HttpPost]
        public ActionResult DodajJedinicuClick()
        {
            string nazivStari = Request["stari"];
            foreach (Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if (a.Naziv == nazivStari)
                {
                    ViewBag.Aranzman = a;
                    break;
                }
            }

            if (Request["zavrsi"] == "true" && Request["akcija"] == null)
            {
                SacuvajAranzman();
                return RedirectToAction("Index", "Menadzer");
            }

            if (Request["akcija"] != null && Request["zavrsi"] == "true")
            {
                SacuvajIzmeneAranzmana(ViewBag.Aranzman);
                ViewBag.Aranzman = trenutnoKreirani;
                trenutnoKreirani = null;
                return View("Index");
            }

            if (!ValidateJedinica(Request))
            {
                return View();
            }

            SmestajnaJedinica nova = new SmestajnaJedinica();
            bool ljubimci;

            nova.CenaZaCeluJedinicu = double.Parse(Request["cena"]);
            if (Request["ljubimci"].ToLower() == "da")
                ljubimci = true;
            else
                ljubimci = false;
            nova.DozvoljeniKucniLjubimci = ljubimci;
            nova.MaxBrojGostiju = Int32.Parse(Request["broj"]);
            nova.Obrisana = false;
            nova.Rezervisana = false;

            trenutnoKreirani.SmestajAranzmana.Jedinice.Add(nova);

            TempData["message"] = "Nova jedinica dodata!";

            if(Request["akcija"] != null)
            {
                //TEK KAD SE ZAVRSE SVE IZMENE SACUVATI VREDNOSTI I UPISATI IH U FAJL
                //ViewBag.Aranzman = ???

                return View("IzmeniAranzmanSmestajClick");
            }

            return View("DodajAranzmanSmestajClick");
        }

        public void SacuvajAranzman()
        {
            //cuvanje svih podataka u fajlovima

            //upis novog aranzmana u fajl od menadzera
            Korisnik login = HomeController.login;
            string pathAranzman = Server.MapPath("~/App_Data/Aranzmani/" + login.KorisnickoIme.ToLower() + ".tsv");
            string tempFile = Path.GetTempFileName();

            StreamReader sr1 = new StreamReader(pathAranzman);
            StreamWriter sw1 = new StreamWriter(tempFile);

            string line = "";
            string aranzmanFile = trenutnoKreirani.Naziv.Replace(" ", "").Replace(",", "").Replace(".", "").Replace("/", "").Replace("-", "").ToLower();
            string mestoNalazenjaFile = trenutnoKreirani.MestoPolaska.Adresa.Replace(",", "").Replace(".", "").Replace("/", "").Replace("-", "").Replace(" ", "_").ToLower() + ".tsv";
            string opisFile = aranzmanFile + "Opis.tsv";
            string programFile = aranzmanFile + "Program.tsv";
            string smestajFile = trenutnoKreirani.SmestajAranzmana.Naziv.Replace(" ", "").Replace(",", "").Replace(".", "").Replace("/", "").Replace("-", "").ToLower() + ".tsv";

            string newLine = trenutnoKreirani.Naziv + "\t" + trenutnoKreirani.TipAranzmana + "\t" + trenutnoKreirani.TipPrevoza + "\t" + trenutnoKreirani.Lokacija + "\t" +
                trenutnoKreirani.DatumPocetka.ToString("dd'/'MM'/'yyyy") + "\t" + trenutnoKreirani.DatumZavrsetka.ToString("dd'/'MM'/'yyyy") + "\t" + mestoNalazenjaFile + "\t" +
                trenutnoKreirani.VremePolaska + "\t" + trenutnoKreirani.MaxBrojPutnika.ToString() + "\t" + opisFile + "\t" + programFile + "\t" + trenutnoKreirani.PosterAranzmana +
                "\t" + smestajFile + "\t" + trenutnoKreirani.Obrisan.ToString();

            while ((line = sr1.ReadLine()) != null)
            {
                sw1.WriteLine(line);
            }

            sw1.WriteLine(newLine);

            sr1.Close();
            sw1.Close();

            System.IO.File.Delete(pathAranzman);
            System.IO.File.Move(tempFile, pathAranzman);

            //upis mesta nalazenja u novi fajl
            string pathMestoNalazenja = Server.MapPath("~/App_Data/MestaNalazenja/" + mestoNalazenjaFile);

            newLine = trenutnoKreirani.MestoPolaska.Adresa + "\t" + trenutnoKreirani.MestoPolaska.GeografskaDuzina.ToString() + "\t" + trenutnoKreirani.MestoPolaska.GeografskaSirina.ToString();
            System.IO.File.WriteAllText(pathMestoNalazenja, newLine);

            //StreamWriter sw2 = new StreamWriter(pathMestoNalazenja);
            //sw2.WriteLine(newLine);
            //sw2.Close();


            //upis opisa aranzmana u novi fajl
            string pathOpis = Server.MapPath("~/App_Data/Opisi/" + opisFile);

            newLine = trenutnoKreirani.OpisAranzmana;
            System.IO.File.WriteAllText(pathOpis, newLine);


            //upis programa aranzmana u novi fajl
            string pathProgram = Server.MapPath("~/App_Data/Programi/" + programFile);

            newLine = trenutnoKreirani.ProgramPutovanja;
            System.IO.File.WriteAllText(pathProgram, newLine);


            //upis smestaja i njegovih jedinica u novi fajl
            string smestajFullPath = Server.MapPath("~/App_Data/Smestaji/" + smestajFile);
            StreamWriter sw2 = new StreamWriter(smestajFullPath);
            newLine = trenutnoKreirani.SmestajAranzmana.Naziv + "\t" + trenutnoKreirani.SmestajAranzmana.TipSmestaja + "\t" + trenutnoKreirani.SmestajAranzmana.BrojZvezdica + "\t" +
                trenutnoKreirani.SmestajAranzmana.Bazen.ToString() + "\t" + trenutnoKreirani.SmestajAranzmana.SpaCentar.ToString() + "\t" + trenutnoKreirani.SmestajAranzmana.ZaOsobeSaInvaliditetom.ToString()
                 + "\t" + trenutnoKreirani.SmestajAranzmana.WiFi.ToString() + "\t" + trenutnoKreirani.SmestajAranzmana.Obrisan.ToString();

            sw2.WriteLine(newLine);
            foreach(SmestajnaJedinica j in trenutnoKreirani.SmestajAranzmana.Jedinice)
            {
                newLine = j.MaxBrojGostiju + "\t" + j.DozvoljeniKucniLjubimci.ToString() + "\t" + j.CenaZaCeluJedinicu + "\t" + "0" + "\t" + "0";
                sw2.WriteLine(newLine);
            }

            sw2.Close();

            foreach(Korisnik m in HomeController.menadzeri)
            {
                if (m.KorisnickoIme == login.KorisnickoIme)
                {
                    m.ListaAranzmana.Add(trenutnoKreirani);
                    login = m;
                    break;
                }
            }
            HomeController.sviKorisnici = HomeController.admini.Concat(HomeController.menadzeri).ToList().Concat(HomeController.turisti).ToList();
            HomeController.aranzmaniMenadzera.Add(trenutnoKreirani);
            HomeController.login = login;
            HttpContext.Application["aranzmani"] = HomeController.aranzmaniMenadzera;
            Session["login"] = login;

            trenutnoKreirani = null;
        }

        public bool ValidateAranzman(HttpRequestBase request)
        {
            //proveriti da ne postoji sa istim nazivom vec
            string naziv = Request["naziv"];
            string tipAranzmana = Request["tipAranzmana"];
            string tipPrevoza = Request["tipPrevoza"];
            string lokacija = Request["lokacija"];
            string datumPocetka = Request["datumPocetka"];
            string datumKraja = Request["datumKraja"];
            string mesto = Request["mesto"];
            string putnici = Request["putnici"];
            string vreme = Request["vreme"];
            string opis = Request["opis"];
            string program = Request["program"];

            string stari = Request["stari"];

            if(naziv == null || naziv == "")
            {
                TempData["message"] = "Morate uneti naziv!";
                return false;
            }
            foreach(Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if(a.Naziv.ToLower() == naziv.ToLower())
                {
                    TempData["message"] = "Vec postoji aranzman sa tim imenom!";
                    return false;
                }
            }
            Regex rgx = new Regex("[A-Za-z0-9]");
            if (!rgx.IsMatch(naziv))
            {
                TempData["message"] = "Naziv aranzmana moze da sadrzi samo slova i brojeve!";
                return false;
            }

            if (lokacija == null || lokacija == "")
            {
                TempData["message"] = "Morate uneti lokaciju!";
                return false;
            }
            if (!rgx.IsMatch(lokacija))
            {
                TempData["message"] = "Lokacija moze da sadrzi samo slova i brojeve!";
                return false;
            }

            //datum pocetka
            int dan = int.Parse(datumPocetka.Substring(0, 2));
            int mesec = int.Parse(datumPocetka.Substring(3, 2));
            int godina = int.Parse(datumPocetka.Substring(6, 4));

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
            //datum kraja
            dan = int.Parse(datumKraja.Substring(0, 2));
            mesec = int.Parse(datumKraja.Substring(3, 2));
            godina = int.Parse(datumKraja.Substring(6, 4));

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

            //mesto
            if (mesto == null || mesto == "")
            {
                TempData["message"] = "Morate uneti mesto nalazenja!";
                return false;
            }
            try
            {
                string[] parts = mesto.Split('-');
                if (!rgx.IsMatch(parts[0]))
                {
                    TempData["message"] = "Adresa nalazenja moze da sadrzi samo slova i brojeve!";
                    return false;
                }
                rgx = new Regex("[0-9]");
                string[] p1 = parts[1].Split('.');
                string[] p2 = parts[2].Split('.');
                if (!rgx.IsMatch(p1[0]))
                {
                    TempData["message"] = "Pogresan format mesta nalazenja!";
                    return false;
                }
                if (!rgx.IsMatch(p1[1]))
                {
                    TempData["message"] = "Pogresan format mesta nalazenja!";
                    return false;
                }
                if (!rgx.IsMatch(p2[0]))
                {
                    TempData["message"] = "Pogresan format mesta nalazenja!";
                    return false;
                }
                if (!rgx.IsMatch(p2[1]))
                {
                    TempData["message"] = "Pogresan format mesta nalazenja!";
                    return false;
                }

            }
            catch
            {
                TempData["message"] = "Pogresan format mesta nalazenja!";
                return false;
            }

            //putnici
            if(putnici == null || putnici == "")
            {
                TempData["message"] = "Morate uneti broj putnika!";
                return false;
            }
            try
            {
                int p = Int32.Parse(putnici);
                if(p <= 0)
                {
                    TempData["message"] = "Broj putnika mora biti veci od 0!";
                    return false;
                }
            }
            catch
            {
                TempData["message"] = "Broj putnika mora biti ceo pozitivan broj!";
                return false;
            }

            //vreme
            if (vreme == null || vreme == "")
            {
                TempData["message"] = "Morate uneti vreme!";
                return false;
            }
            try
            {
                string[] parts = vreme.Split(':');
                if(Int32.Parse(parts[0]) > 23 || Int32.Parse(parts[1]) > 59)
                {
                    TempData["message"] = "Vreme mora biti u formatu hh:mm!";
                    return false;
                }
            }
            catch
            {
                TempData["message"] = "Vreme mora biti u formatu hh:mm!";
                return false;
            }

            //opis
            if (opis == null || opis == "")
            {
                TempData["message"] = "Morate uneti opis!";
                return false;
            }

            //program
            if (program == null || program == "")
            {
                TempData["message"] = "Morate uneti program!";
                return false;
            }

            //stari
            if (stari == null || stari == "")
            {
                TempData["message"] = "Nepoznat naziv starog aranzmana!";
                return false;
            }
            rgx = new Regex("[A-Za-z0-9]");
            if (!rgx.IsMatch(stari))
            {
                TempData["message"] = "KStari naziv aranzmana moze da sadrzi samo slova i brojeve!";
                return false;
            }

            return true;
        }

        public bool ValidateSmestaj(HttpRequestBase request)
        {
            string naziv = Request["naziv"];
            string tipSmestaja = Request["tipSmestaja"];
            string brojZvezdica = Request["zvezdice"];
            string bazen = request["bazen"];
            string spa = request["spa"];
            string invalidi = request["invalidi"];
            string wifi = request["invalidi"];

            //naziv
            if (naziv == null || naziv == "")
            {
                TempData["message"] = "Morate uneti naziv!";
                return false;
            }
            foreach (Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if (a.SmestajAranzmana.Naziv.ToLower() == naziv.ToLower())
                {
                    TempData["message"] = "Vec postoji smestaj sa tim imenom!";
                    return false;
                }
            }
            Regex rgx = new Regex("[A-Za-z0-9]");
            if (!rgx.IsMatch(naziv))
            {
                TempData["message"] = "Naziv smestaja moze da sadrzi samo slova i brojeve!";
                return false;
            }

            //broj zvezdica
            if (brojZvezdica == null || brojZvezdica == "")
            {
                TempData["message"] = "Morate uneti broj zvezdica!";
                return false;
            }
            try
            {
                int p = Int32.Parse(brojZvezdica);
                if (p <= 0 || p > 5)
                {
                    TempData["message"] = "Broj zvezdica mora biti izmedju 1 i 5!";
                    return false;
                }
            }
            catch
            {
                TempData["message"] = "Broj zvezdica mora biti ceo pozitivan broj!";
                return false;
            }

            //bazen
            if(bazen == null || bazen == "")
            {
                TempData["message"] = "Bazen (DA/NE)?";
                return false;
            }
            if(bazen.ToLower() != "da" && bazen.ToLower() != "ne")
            {
                TempData["message"] = "Bazen (DA/NE)?";
                return false;
            }

            //spa
            if (spa == null || spa == "")
            {
                TempData["message"] = "Spa centar (DA/NE)?";
                return false;
            }
            if (spa.ToLower() != "da" && spa.ToLower() != "ne")
            {
                TempData["message"] = "Spa centar (DA/NE)?";
                return false;
            }

            //invalidi
            if (invalidi == null || invalidi == "")
            {
                TempData["message"] = "PRilagodjeno za invalide (DA/NE)?";
                return false;
            }
            if (invalidi.ToLower() != "da" && invalidi.ToLower() != "ne")
            {
                TempData["message"] = "Prilagodjeno za invalide (DA/NE)?";
                return false;
            }

            //wifi
            if (wifi == null || wifi == "")
            {
                TempData["message"] = "WiFi (DA/NE)?";
                return false;
            }
            if (wifi.ToLower() != "da" && wifi.ToLower() != "ne")
            {
                TempData["message"] = "WiFi (DA/NE)?";
                return false;
            }

            return true;
        }

        public bool ValidateJedinica(HttpRequestBase request)
        {
            string broj = Request["broj"];
            string ljubimci = Request["ljubimci"];
            string cena = Request["cena"];

            //broj
            if (broj == null || broj == "")
            {
                TempData["message"] = "Morate uneti maksimalan broj kreveta!";
                return false;
            }
            try
            {
                int p = Int32.Parse(broj);
                if (p <= 0)
                {
                    TempData["message"] = "Broj kreveta mora biti veci od 0!";
                    return false;
                }
            }
            catch
            {
                TempData["message"] = "Broj kreveta mora biti ceo pozitivan broj!";
                return false;
            }

            //ljubimci
            if (ljubimci == null || ljubimci == "")
            {
                TempData["message"] = "Dozvoljeni kucni ljubimci (DA/NE)?";
                return false;
            }
            if (ljubimci.ToLower() != "da" && ljubimci.ToLower() != "ne")
            {
                TempData["message"] = "Dozvoljeni kucni ljubimci (DA/NE)?";
                return false;
            }

            //cena
            if (cena == null || cena == "")
            {
                TempData["message"] = "Morate uneti cenu!";
                return false;
            }
            try
            {
                double p = double.Parse(cena);
                if (p <= 0)
                {
                    TempData["message"] = "Cena mora biti pozitivna!";
                    return false;
                }
            }
            catch
            {
                TempData["message"] = "Cena mora biti pozitivan broj!";
                return false;
            }

            return true;
        }

        [HttpPost]
        public ActionResult DodajAranzmanSmestajKrajClick()
        {
            return View();
        }

        public ActionResult IzmeniAranzman()
        {
            string naziv = Request["naziv"];
            foreach(Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if(a.Naziv == naziv)
                {
                    ViewBag.Aranzman = a;
                    break;
                }
            }
            trenutnoKreirani = new Aranzman();

            return View();
        }

        public ActionResult IzmeniAranzmanClick(HttpPostedFileBase poster)
        {
            string nazivStari = Request["stari"];
            string naziv = Request["naziv"];
            string tipAranzmana = Request["tipAranzmana"];
            string tipPrevoza = Request["tipPrevoza"];
            string lokacija = Request["lokacija"];
            string datumPocetka = Request["datumPocetka"];
            string datumKraja = Request["datumKraja"];
            string mesto = Request["mesto"];
            string vreme = Request["vreme"];
            string putnici = Request["putnici"];
            string opis = Request["opis"];
            string program = Request["program"];
            Aranzman stari = new Aranzman();
            foreach (Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if (a.Naziv == nazivStari)
                {
                    stari = a;
                    break;
                }
            }

            if (!ValidateAranzman(Request))
            {
                return RedirectToAction("IzmeniAranzman");
            }

            if(poster != null) {
                string posterPath = Path.Combine(Server.MapPath("~/Images/"), poster.FileName);
                poster.SaveAs(posterPath);
                trenutnoKreirani.PosterAranzmana = poster.FileName;
            }
            else
            {
                trenutnoKreirani.PosterAranzmana = stari.PosterAranzmana;
            }

            string[] deloviDP = datumPocetka.Split('/');
            string[] deloviDK = datumKraja.Split('/');

            DateTime datumPocetkaDT = new DateTime(int.Parse(deloviDP[2]), int.Parse(deloviDP[1]), int.Parse(deloviDP[0]));
            DateTime datumKrajaDT = new DateTime(int.Parse(deloviDK[2]), int.Parse(deloviDK[1]), int.Parse(deloviDK[0]));

            string[] delovi = mesto.Split('-');
            string adresa = delovi[0];
            double geoDuz = double.Parse(delovi[1]);
            double geoSir = double.Parse(delovi[2]);

            trenutnoKreirani.Naziv = naziv;
            trenutnoKreirani.TipAranzmana = tipAranzmana;
            trenutnoKreirani.TipPrevoza = tipPrevoza;
            trenutnoKreirani.Lokacija = lokacija;
            trenutnoKreirani.VremePolaska = vreme;
            trenutnoKreirani.DatumPocetka = datumPocetkaDT;
            trenutnoKreirani.DatumZavrsetka = datumKrajaDT;
            trenutnoKreirani.MestoPolaska = new MestoNalazenja(adresa, geoDuz, geoSir);
            trenutnoKreirani.OpisAranzmana = opis;
            trenutnoKreirani.ProgramPutovanja = program;
            trenutnoKreirani.MaxBrojPutnika = Int32.Parse(putnici);
            trenutnoKreirani.SmestajAranzmana = new Smestaj();

            ViewBag.Aranzman = stari;
            return View();
        }

        public ActionResult IzmeniAranzmanSmestajClick()
        {
            string nazivStari = Request["stari"];
            Aranzman stari = new Aranzman();
            foreach (Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if (a.Naziv == nazivStari)
                {
                    stari = a;
                    break;
                }
            }

            ViewBag.Aranzman = stari;

            if (!ValidateSmestaj(Request))
            {
                return View("IzmeniAranzmanClick");
            }

            //dodat je smestaj i sad njegove podatke ubaciti
            trenutnoKreirani.SmestajAranzmana.Naziv = Request["naziv"];
            trenutnoKreirani.SmestajAranzmana.TipSmestaja = Request["tipSmestaja"];
            trenutnoKreirani.SmestajAranzmana.BrojZvezdica = Int32.Parse(Request["zvezdice"]);
            bool bazen, spa, invalidi, wifi;

            if (Request["bazen"].ToLower() == "da")
                bazen = true;
            else
                bazen = false;
            if (Request["spa"].ToLower() == "da")
                spa = true;
            else
                spa = false;
            if (Request["invalidi"].ToLower() == "da")
                invalidi = true;
            else
                invalidi = false;
            if (Request["wifi"].ToLower() == "da")
                wifi = true;
            else
                wifi = false;

            trenutnoKreirani.SmestajAranzmana.Bazen = bazen;
            trenutnoKreirani.SmestajAranzmana.SpaCentar = spa;
            trenutnoKreirani.SmestajAranzmana.ZaOsobeSaInvaliditetom = invalidi;
            trenutnoKreirani.SmestajAranzmana.WiFi = wifi;
            //dodeli mu iste u slucaju da se nista ne izmeni
            trenutnoKreirani.SmestajAranzmana.Jedinice = stari.SmestajAranzmana.Jedinice;

            return View();
        }

        public ActionResult IzmeniJedinicuClick()
        {
            //upisi nove podatke u trenutniAranzman
            string naziv = Request["stari"];

            string broj = Request["broj"];
            string ljubimci = Request["ljubimci"];
            string cena = Request["cena"];

            string sBroj = Request["sBroj"];
            string sLjub = Request["sLjub"];
            bool slj = false;
            if (sLjub.ToLower() == "da")
                slj = true;
            string sCena = Request["sCena"];

            //podaci za povratak nastranicu
            Aranzman stari = new Aranzman();
            foreach (Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if (a.Naziv == naziv)
                {
                    ViewBag.Aranzman = a;
                    stari = a;
                    break;
                }
            }

            if (!ValidateJedinica(Request))
            {
                ViewBag.Aranzman = stari;
                return View("IzmeniJedinicu");
            }

            foreach(SmestajnaJedinica j in trenutnoKreirani.SmestajAranzmana.Jedinice)
            {
                if(j.MaxBrojGostiju == Int32.Parse(sBroj) && j.CenaZaCeluJedinicu == Int32.Parse(sCena) && j.DozvoljeniKucniLjubimci == slj)
                {
                    j.MaxBrojGostiju = Int32.Parse(broj);
                    if (ljubimci.ToLower() == "da")
                        j.DozvoljeniKucniLjubimci = true;
                    else
                        j.DozvoljeniKucniLjubimci = false;
                    j.CenaZaCeluJedinicu = double.Parse(cena);
                    
                    break;
                }
            }

            TempData["message"] = "Aranzman uspesno izmenjen";

            return View("IzmeniAranzmanSmestajClick");
        }

        public void SacuvajIzmeneAranzmana(Aranzman stari)
        {
            //ne vracati trenutni na null   !!!

            //upis aranzmana
            string pathAranzman = Server.MapPath("~/App_Data/Aranzmani/" + HomeController.login.KorisnickoIme.ToLower() + ".tsv");
            string pathTemp = Path.GetTempFileName();

            StreamReader sr = new StreamReader(pathAranzman);
            StreamWriter sw = new StreamWriter(pathTemp);

            string nazivFile = trenutnoKreirani.Naziv.Replace(",", "").Replace(".", "").Replace("/", "").Replace("-", "").Replace(" ", "").ToLower();
            string mestoFile = trenutnoKreirani.MestoPolaska.Adresa.Replace(",", "").Replace(".", "").Replace("/", "").Replace("-", "").Replace(" ", "_").ToLower() + ".tsv";
            string opisFile = nazivFile + "Opis" + ".tsv";
            string programFile = nazivFile + "Program" + ".tsv";
            string posterFile = trenutnoKreirani.PosterAranzmana.Replace(",", "").Replace("/", "").Replace("-", "").Replace(" ", "").ToLower();
            string smestajFile = trenutnoKreirani.SmestajAranzmana.Naziv.Replace(",", "").Replace(".", "").Replace("/", "").Replace("-", "").Replace(" ", "").ToLower() + ".tsv";

            string newLine = trenutnoKreirani.Naziv + "\t" + trenutnoKreirani.TipAranzmana + "\t" + trenutnoKreirani.TipPrevoza + "\t" + trenutnoKreirani.Lokacija + "\t" +
                trenutnoKreirani.DatumPocetka.ToString("dd'/'MM'/'yyyy") + "\t" + trenutnoKreirani.DatumZavrsetka.ToString("dd'/'MM'/'yyyy") + "\t" + mestoFile + "\t" +
                trenutnoKreirani.VremePolaska + "\t" + trenutnoKreirani.MaxBrojPutnika + "\t" + opisFile + "\t" + programFile + "\t" + posterFile + "\t" + smestajFile + "\t" 
                + trenutnoKreirani.Obrisan.ToString();

            string line = "";
            while(!((line = sr.ReadLine()) == null))
            {
                string[] parts = line.Split('\t');
                if(parts[0] == stari.Naziv)
                {
                    sw.WriteLine(newLine);
                }
                else
                {
                    sw.WriteLine(line);
                }
            }

            sr.Close();
            sw.Close();

            System.IO.File.Delete(pathAranzman);
            System.IO.File.Move(pathTemp, pathAranzman);

            //upis mesta nalazenja
            string mestoPath = Server.MapPath("~/App_Data/MestaNalazenja/" + mestoFile);
            pathTemp = Path.GetTempFileName();

            StreamWriter sw1 = new StreamWriter(pathTemp);

            newLine = trenutnoKreirani.MestoPolaska.Adresa + "\t" + trenutnoKreirani.MestoPolaska.GeografskaDuzina + "\t" + trenutnoKreirani.MestoPolaska.GeografskaSirina;

            sw1.WriteLine(newLine);

            sw1.Close();

            System.IO.File.Delete(mestoPath);
            System.IO.File.Move(pathTemp, mestoPath);


            //upis opisa
            string opisPath = Server.MapPath("~/App_Data/Opisi/" + opisFile);
            pathTemp = Path.GetTempFileName();

            StreamWriter sw2 = new StreamWriter(pathTemp);

            newLine = trenutnoKreirani.OpisAranzmana;

            sw2.WriteLine(newLine);

            sw2.Close();

            System.IO.File.Delete(opisPath);
            System.IO.File.Move(pathTemp, opisPath);


            //upis programa
            string programPath = Server.MapPath("~/App_Data/Programi/" + programFile);
            pathTemp = Path.GetTempFileName();

            StreamWriter sw3 = new StreamWriter(pathTemp);

            newLine = trenutnoKreirani.ProgramPutovanja;

            sw3.WriteLine(newLine);

            sw3.Close();

            System.IO.File.Delete(programPath);
            System.IO.File.Move(pathTemp, programPath);


            //upis rezervacija ako se promenio naziv aranzmana !!!
            foreach (Korisnik t in HomeController.turisti) {
                string rezervacijaPath = Server.MapPath("~/App_Data/Rezervacije/" + t.KorisnickoIme.ToLower() + ".tsv");
                pathTemp = Path.GetTempFileName();

                StreamReader sr4 = new StreamReader(rezervacijaPath);
                StreamWriter sw4 = new StreamWriter(pathTemp);

                while (!((line = sr4.ReadLine()) == null))
                {
                    string[] parts = line.Split('\t');
                    if (parts[3] == stari.Naziv)
                    {
                        newLine = parts[0] + "\t" + parts[1] + "\t" + parts[2] + "\t" + trenutnoKreirani.Naziv;
                        sw4.WriteLine(newLine);
                    }
                    else
                    {
                        sw4.WriteLine(line);
                    }
                }

                sr4.Close();
                sw4.Close();

                System.IO.File.Delete(rezervacijaPath);
                System.IO.File.Move(pathTemp, rezervacijaPath);

            }
            //upis smestaja
            string smestajPath = Server.MapPath("~/App_Data/Smestaji/" + smestajFile);
            pathTemp = Path.GetTempFileName();

            StreamWriter sw5 = new StreamWriter(pathTemp);

            sw5.WriteLine(trenutnoKreirani.SmestajAranzmana.Naziv + "\t" + trenutnoKreirani.SmestajAranzmana.TipSmestaja + "\t" + trenutnoKreirani.SmestajAranzmana.BrojZvezdica + "\t"
                + trenutnoKreirani.SmestajAranzmana.Bazen.ToString() + "\t" + trenutnoKreirani.SmestajAranzmana.SpaCentar.ToString() + "\t" +
                trenutnoKreirani.SmestajAranzmana.ZaOsobeSaInvaliditetom.ToString() + "\t" + trenutnoKreirani.SmestajAranzmana.WiFi.ToString());

            foreach (SmestajnaJedinica j in trenutnoKreirani.SmestajAranzmana.Jedinice) {
                if (!j.Rezervisana)
                {
                    if(j.Obrisana)
                        sw5.WriteLine(j.MaxBrojGostiju + "\t" + j.DozvoljeniKucniLjubimci.ToString() + "\t" + j.CenaZaCeluJedinicu + "\t" + "0" + "\t" + "1");
                    else
                        sw5.WriteLine(j.MaxBrojGostiju + "\t" + j.DozvoljeniKucniLjubimci.ToString() + "\t" + j.CenaZaCeluJedinicu + "\t" + "0" + "\t" + "0");
                }
                else
                {
                    if(j.Obrisana)
                        sw5.WriteLine(j.MaxBrojGostiju + "\t" + j.DozvoljeniKucniLjubimci.ToString() + "\t" + j.CenaZaCeluJedinicu + "\t" + "1" + "\t" + "0");
                    else
                        sw5.WriteLine(j.MaxBrojGostiju + "\t" + j.DozvoljeniKucniLjubimci.ToString() + "\t" + j.CenaZaCeluJedinicu + "\t" + "1" + "\t" + "1");
                }
            }

            sw5.Close();

            //promeni naziv aranzmana u fajlu za komentare

            string komPath = Server.MapPath("~/App_Data/komentari.tsv");
            pathTemp = Path.GetTempFileName();

            StreamReader sr6 = new StreamReader(komPath);
            StreamWriter sw6 = new StreamWriter(pathTemp);

            while ((line = sr6.ReadLine()) != null)
            {
                string[] parts = line.Split('\t');
                if(parts[1] == stari.Naziv)
                {
                    newLine = parts[0] + "\t" + trenutnoKreirani.Naziv + "\t" + parts[2] + "\t" + parts[3] + "\t" + parts[4];
                    sw6.WriteLine(newLine);
                }
                else
                {
                    sw6.WriteLine(line);
                }
            }

            sr6.Close();
            sw6.Close();

            System.IO.File.Delete(komPath);
            System.IO.File.Move(pathTemp, komPath);

            //dodeliti nove vrednosti

            //promeni aranzman kod menadzera koji ga je kreirao
            List<Aranzman> novi = new List<Aranzman>();
            foreach(Aranzman a in HomeController.login.ListaAranzmana)
            {
                if(a.Naziv == stari.Naziv)
                {
                    novi.Add(trenutnoKreirani);
                }
                else
                {
                    novi.Add(a);
                }
            }
            HomeController.login.ListaAranzmana = novi;

            //promeni aranzman kod turista koji su ga rezervisali
            List<Rezervacija> nove = new List<Rezervacija>();
            foreach(Korisnik t in HomeController.turisti)
            {
                foreach(Rezervacija r in t.ListaRezervacija)
                {
                    if(r.IzabraniAranzman.Naziv == stari.Naziv)
                    {
                        //jedinica se takodje izmenila  !!!!!!!!!!!!
                        foreach(SmestajnaJedinica j in trenutnoKreirani.SmestajAranzmana.Jedinice)
                        {
                            if (r.Jedinica.MaxBrojGostiju == j.MaxBrojGostiju)
                            {
                                nove.Add(new Rezervacija(r.Id, r.Turista, r.StatusAktivna, trenutnoKreirani, j));
                                break;
                            }
                        }
                    }
                    else
                    {
                        nove.Add(r);
                    }
                }
                t.ListaRezervacija = nove;
                nove = new List<Rezervacija>();
            }

            //promeni aranzman u spisku komentara za taj aranzman
            List<Komentar> novik = new List<Komentar>();
            foreach(Komentar k in HomeController.komentari)
            {
                if(k.AranzmanKomentarisan.Naziv == stari.Naziv)
                {
                    novik.Add(new Komentar(k.Turista, trenutnoKreirani, k.Tekst, k.Ocena));
                }
                else
                {
                    novik.Add(k);
                }
            }
            HomeController.komentari = novik;


            //promeni aranzman u spisku svih aranzmana menadzera
            foreach(Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if(a.Naziv == stari.Naziv)
                {
                    HomeController.aranzmaniMenadzera.Remove(a);
                    HomeController.aranzmaniMenadzera.Add(trenutnoKreirani);
                    break;
                }
            }

            //promeni aranzman u listi komentara
            foreach(Komentar k in HomeController.komentari)
            {
                if(k.AranzmanKomentarisan.Naziv == stari.Naziv)
                {
                    HomeController.komentari.Remove(k);
                    HomeController.komentari.Add(new Komentar(k.Turista, trenutnoKreirani, k.Tekst, k.Ocena));
                    break;
                }
            }

            //promeni aranzman u spisku rezervacija
            List<Rezervacija> rez = new List<Rezervacija>();
            foreach(Rezervacija r in HomeController.rezervacije)
            {
                if(r.IzabraniAranzman.Naziv == stari.Naziv)
                {
                    foreach(SmestajnaJedinica j in trenutnoKreirani.SmestajAranzmana.Jedinice)
                    {
                        if(j.MaxBrojGostiju == r.Jedinica.MaxBrojGostiju)
                        {
                            rez.Add(new Rezervacija(r.Id, r.Turista, r.StatusAktivna, trenutnoKreirani, j));
                            break;
                        }
                    }
                }
                else
                {
                    rez.Add(r);
                }
            }
            HomeController.rezervacije = rez;

            //promeni aranzman u login korisniku
            List<Aranzman> ara = new List<Aranzman>();
            foreach (Aranzman a in HomeController.login.ListaAranzmana)
            {
                if(a.Naziv == stari.Naziv)
                {
                    ara.Add(trenutnoKreirani);
                }
                else
                {
                    ara.Add(a);
                }
            }
            HomeController.login.ListaAranzmana = ara;


            //promeni aranzman u spisku svih korisnika zaj
            HomeController.sviKorisnici = HomeController.admini.Concat(HomeController.menadzeri).ToList().Concat(HomeController.turisti).ToList();


            //promeni sve vrednosti u HttpContext.App i Session
            Session["login"] = HomeController.login;
            HttpContext.Application["menadzeri"] = HomeController.menadzeri;
            HttpContext.Application["admini"] = HomeController.admini;
            HttpContext.Application["turisti"] = HomeController.turisti;
            HttpContext.Application["aranzmaniMenadzera"] = HomeController.aranzmaniMenadzera;
            HttpContext.Application["rezervacije"] = HomeController.rezervacije;
            HttpContext.Application["komentari"] = HomeController.komentari;
            HttpContext.Application["rezervacije"] = HomeController.rezervacije;

        }

        public ActionResult IzmeniJedinicu()
        {
            string nazivStari = Request["stari"];
            string cena = Request["cena"];
            string ljubimci = Request["ljubimci"];
            string broj = Request["broj"];

            foreach (Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if (a.Naziv == nazivStari)
                {
                    ViewBag.Aranzman = a;
                    foreach(SmestajnaJedinica j in a.SmestajAranzmana.Jedinice)
                    {
                        if(j.CenaZaCeluJedinicu.ToString() == cena && j.DozvoljeniKucniLjubimci.ToString() == ljubimci && j.MaxBrojGostiju.ToString() == broj)
                        {
                            ViewBag.Jedinica = j;
                            break;
                        }
                    }
                    break;
                }
            }

            return View();
        }

        public ActionResult Komentari()
        {
            List<Komentar> moji = new List<Komentar>();
            foreach(Komentar k in HomeController.komentari)
            {
                foreach(Aranzman a in HomeController.login.ListaAranzmana)
                {
                    if(k.AranzmanKomentarisan.Naziv == a.Naziv)
                    {
                        moji.Add(k);
                        break;
                    }
                }
            }
            ViewBag.Komentari = moji;

            return View();
        }

        public ActionResult Odbij()
        {
            string turistaKI = Request["turista"];
            string aranzmanN = Request["aranzman"];
            Komentar kom = new Komentar();

            foreach(Komentar k in HomeController.komentari)
            {
                if (k.Odobren == "Ceka" && k.Turista.KorisnickoIme == turistaKI && k.AranzmanKomentarisan.Naziv == aranzmanN)
                {
                    HomeController.komentari.Remove(k);
                    k.Odobren = "Odbijen";
                    HomeController.komentari.Add(k);
                    kom = k;
                    break;
                }
            }

            //upis u fajl
            string path = Server.MapPath("~/komentari.tsv");
            string temp = Path.GetTempFileName();

            StreamReader sr = new StreamReader(path);
            StreamWriter sw = new StreamWriter(temp);

            string line = "";
            while((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split('\t');
                if(parts[0] == turistaKI && parts[1] == aranzmanN)
                {
                    string newline = turistaKI + "\t" + aranzmanN + "\t" + kom.Tekst + "\t" + kom.Ocena + "\t" + "Odbijen";
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

            return RedirectToAction("Komentari");
        }

        public ActionResult Odobri()
        {
            string turistaKI = Request["turista"];
            string aranzmanN = Request["aranzman"];
            Komentar kom = new Komentar();

            foreach (Komentar k in HomeController.komentari)
            {
                if (k.Odobren == "Ceka" && k.Turista.KorisnickoIme == turistaKI)
                {
                    HomeController.komentari.Remove(k);
                    k.Odobren = "Odobren";
                    HomeController.komentari.Add(k);
                    kom = k;
                    break;
                }
            }

            //upis u fajl
            string path = Server.MapPath("~/App_Data/komentari.tsv");
            string temp = Path.GetTempFileName();

            StreamReader sr = new StreamReader(path);
            StreamWriter sw = new StreamWriter(temp);

            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split('\t');
                if (parts[0] == turistaKI && parts[1] == aranzmanN)
                {
                    string newline = turistaKI + "\t" + aranzmanN + "\t" + kom.Tekst + "\t" + kom.Ocena + "\t" + "Odobren";
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

            return RedirectToAction("Komentari");
        }

        public ActionResult ObrisiAranzman()
        {
            string aranzman = Request["naziv"];

            //provera da li ga ima u rezervacijama u buducnosti
            foreach(Rezervacija r in HomeController.rezervacije)
            {
                if(r.IzabraniAranzman.Naziv == aranzman && r.IzabraniAranzman.DatumPocetka >= DateTime.Today)
                {
                    TempData["message"] = "Ne mozete obrisati rezervisane aranzmane!";
                    return RedirectToAction("Index");
                }
            }

            Aranzman tr = new Aranzman();
            foreach(Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if(a.Naziv == aranzman)
                {
                    a.Obrisan = true;
                    tr = a;
                    break;
                }
            }

            //podesi vrednosti u svim poljima
            SacuvajBrisanje(tr);

            //upisi u fajl novi aranzman
            string path = Server.MapPath("~/App_Data/Aranzmani/" + HomeController.login.KorisnickoIme.ToLower() + ".tsv");
            string temp = Path.GetTempFileName();

            StreamReader sr = new StreamReader(path);
            StreamWriter sw = new StreamWriter(temp);

            string line;
            while((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split('\t');
                if(parts[0] == tr.Naziv)
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

            return RedirectToAction("Index");
        }

        public ActionResult ObrisiSmestaj()
        {
            string aranzman = Request["aranzman"];

            //provera da li ga ima u rezervacijama u buducnosti
            foreach (Rezervacija r in HomeController.rezervacije)
            {
                if (r.IzabraniAranzman.Naziv == aranzman && r.IzabraniAranzman.DatumPocetka >= DateTime.Today)
                {
                    TempData["message"] = "Ne mozete obrisati rezervisane smestaje!";
                    return RedirectToAction("Index");
                }
            }

            Aranzman tr = new Aranzman();
            foreach (Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if (a.Naziv == aranzman)
                {
                    a.SmestajAranzmana.Obrisan = true;
                    tr = a;
                    break;
                }
            }

            //podesi vrednosti u svim poljima
            SacuvajBrisanje(tr);

            //upisi u fajl novi aranzman
            string path = Server.MapPath("~/App_Data/Smestaji/" + tr.SmestajAranzmana.Naziv.ToLower() + ".tsv");
            string temp = Path.GetTempFileName();

            StreamReader sr = new StreamReader(path);
            StreamWriter sw = new StreamWriter(temp);

            string line = sr.ReadLine();
            string newline = line.Substring(0, line.Length - 5) + "True";
            sw.WriteLine(newline);

            while ((line = sr.ReadLine()) != null)
            {
                sw.WriteLine(line);
            }

            sr.Close();
            sw.Close();

            System.IO.File.Delete(path);
            System.IO.File.Move(temp, path);


            return RedirectToAction("Index");
        }

        public ActionResult ObrisiJedinicu()
        {
            string aranzman = Request["aranzman"];
            string broj = Request["broj"];
            string ljub = Request["ljub"];
            string cena = Request["cena"];
            Aranzman mojA = new Aranzman();
            SmestajnaJedinica mojaJ = new SmestajnaJedinica();

            foreach(Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if(a.Naziv == aranzman)
                {
                    mojA = a;
                    foreach(SmestajnaJedinica j in a.SmestajAranzmana.Jedinice)
                    {
                        if(broj == j.MaxBrojGostiju.ToString() && ljub == j.DozvoljeniKucniLjubimci.ToString() && cena == j.CenaZaCeluJedinicu.ToString() && !j.Obrisana)
                        {
                            j.Obrisana = true;
                            mojaJ = j;
                            break;
                        }
                    }
                    break;
                }
            }

            SacuvajBrisanje(mojA);

            //upisi u fajl novi aranzman
            string path = Server.MapPath("~/App_Data/Smestaji/" + mojA.SmestajAranzmana.Naziv.ToLower() + ".tsv");
            string temp = Path.GetTempFileName();

            StreamReader sr = new StreamReader(path);
            StreamWriter sw = new StreamWriter(temp);

            string line = sr.ReadLine();
            sw.WriteLine(line);

            string newline;

            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split('\t');

                //ako nije obrisana i nije rezervisana
                if(parts[0] == broj && parts[1] == ljub && parts[2] == cena && parts[3] == "0" && parts[4] == "0")
                {
                    newline = line.Substring(0, line.Length - 1) + "1";
                    sw.WriteLine(newline);
                }
                else
                    sw.WriteLine(line);
            }

            sr.Close();
            sw.Close();

            System.IO.File.Delete(path);
            System.IO.File.Move(temp, path);

            return RedirectToAction("Index");
        }

        public void SacuvajBrisanje(Aranzman trenutni)
        {
            //promeni aranzman kod menadzera koji ga je kreirao
            List<Aranzman> novi = new List<Aranzman>();
            foreach (Aranzman a in HomeController.login.ListaAranzmana)
            {
                if (a.Naziv == trenutni.Naziv)
                {
                    novi.Add(trenutni);
                }
                else
                {
                    novi.Add(a);
                }
            }
            HomeController.login.ListaAranzmana = novi;

            //promeni aranzman kod turista koji su ga rezervisali
            List<Rezervacija> nove = new List<Rezervacija>();
            foreach (Korisnik t in HomeController.turisti)
            {
                foreach (Rezervacija r in t.ListaRezervacija)
                {
                    if (r.IzabraniAranzman.Naziv == trenutni.Naziv)
                    {
                        //jedinica se takodje izmenila  !!!!!!!!!!!!
                        foreach (SmestajnaJedinica j in trenutni.SmestajAranzmana.Jedinice)
                        {
                            if (r.Jedinica.MaxBrojGostiju == j.MaxBrojGostiju)
                            {
                                nove.Add(new Rezervacija(r.Id, r.Turista, r.StatusAktivna, trenutni, j));
                                break;
                            }
                        }
                    }
                    else
                    {
                        nove.Add(r);
                    }
                }
                t.ListaRezervacija = nove;
                nove = new List<Rezervacija>();
            }

            //promeni aranzman u spisku komentara za taj aranzman
            List<Komentar> novik = new List<Komentar>();
            foreach (Komentar k in HomeController.komentari)
            {
                if (k.AranzmanKomentarisan.Naziv == trenutni.Naziv)
                {
                    novik.Add(new Komentar(k.Turista, trenutni, k.Tekst, k.Ocena));
                }
                else
                {
                    novik.Add(k);
                }
            }
            HomeController.komentari = novik;


            //promeni aranzman u spisku svih aranzmana menadzera
            foreach (Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if (a.Naziv == trenutni.Naziv)
                {
                    HomeController.aranzmaniMenadzera.Remove(a);
                    HomeController.aranzmaniMenadzera.Add(trenutni);
                    break;
                }
            }

            //promeni aranzman u listi komentara
            foreach (Komentar k in HomeController.komentari)
            {
                if (k.AranzmanKomentarisan.Naziv == trenutni.Naziv)
                {
                    HomeController.komentari.Remove(k);
                    HomeController.komentari.Add(new Komentar(k.Turista, trenutni, k.Tekst, k.Ocena));
                    break;
                }
            }

            //promeni aranzman u spisku rezervacija
            List<Rezervacija> rez = new List<Rezervacija>();
            foreach (Rezervacija r in HomeController.rezervacije)
            {
                if (r.IzabraniAranzman.Naziv == trenutni.Naziv)
                {
                    foreach (SmestajnaJedinica j in trenutni.SmestajAranzmana.Jedinice)
                    {
                        if (j.MaxBrojGostiju == r.Jedinica.MaxBrojGostiju)
                        {
                            rez.Add(new Rezervacija(r.Id, r.Turista, r.StatusAktivna, trenutni, j));
                            break;
                        }
                    }
                }
                else
                {
                    rez.Add(r);
                }
            }
            HomeController.rezervacije = rez;

            //promeni aranzman u login korisniku
            List<Aranzman> ara = new List<Aranzman>();
            foreach (Aranzman a in HomeController.login.ListaAranzmana)
            {
                if (a.Naziv == trenutni.Naziv)
                {
                    ara.Add(trenutni);
                }
                else
                {
                    ara.Add(a);
                }
            }
            HomeController.login.ListaAranzmana = ara;

            //promeni aranzman u spisku svih korisnika zaj
            HomeController.sviKorisnici = HomeController.admini.Concat(HomeController.menadzeri).ToList().Concat(HomeController.turisti).ToList();

            //promeni sve vrednosti u HttpContext.App i Session
            Session["login"] = HomeController.login;
            HttpContext.Application["menadzeri"] = HomeController.menadzeri;
            HttpContext.Application["admini"] = HomeController.admini;
            HttpContext.Application["turisti"] = HomeController.turisti;
            HttpContext.Application["aranzmaniMenadzera"] = HomeController.aranzmaniMenadzera;
            HttpContext.Application["rezervacije"] = HomeController.rezervacije;
            HttpContext.Application["komentari"] = HomeController.komentari;
            HttpContext.Application["rezervacije"] = HomeController.rezervacije;

        }

        public ActionResult Rezervacije()
        {
            List<Rezervacija> list = new List<Rezervacija>();
            foreach(Aranzman a in HomeController.login.ListaAranzmana)
            {
                foreach(Rezervacija r in HomeController.rezervacije)
                {
                    if(a.Naziv == r.IzabraniAranzman.Naziv)
                    {
                        list.Add(r);
                    }
                }
            }

            ViewBag.Rezervacije = list;
            return View();
        }

        public ActionResult FilterR()
        {
            string id = Request["id"];
            string turista = Request["turista"];
            string aktivna = Request["aktivna"];
            string aranzman = Request["aranzman"];
            string gosti = Request["gosti"];

            string sortOpcija = Request["sortOpcija"];
            string sortNacin = Request["sortNacin"];

            int gostiI;
            Int32.TryParse(gosti, out gostiI);

            List<Rezervacija> list = new List<Rezervacija>();
            foreach (Aranzman a in HomeController.login.ListaAranzmana)
            {
                foreach (Rezervacija r in HomeController.rezervacije)
                {
                    if (a.Naziv == r.IzabraniAranzman.Naziv)
                    {
                        if(r.Id.Contains(id) && r.Turista.KorisnickoIme.ToLower().Contains(turista.ToLower()) && r.IzabraniAranzman.Naziv.ToLower().Contains(aranzman.ToLower()) 
                            && ((r.Jedinica.MaxBrojGostiju.ToString() == gosti && gosti != "") || (gosti == "")))
                        {
                            //aktivna
                            if(aktivna == "/")
                                list.Add(r);
                            else if(aktivna == "DA" && r.StatusAktivna)
                                list.Add(r);
                            else if(aktivna == "NE" && !r.StatusAktivna)
                                list.Add(r);
                        }
                    }
                }
            }

            if(sortOpcija == "id")
            {
                if(sortNacin == "rastuce")
                {
                    list = list.OrderBy(x => x.Id).ToList();
                }
                else
                {
                    list = list.OrderByDescending(x => x.Id).ToList();
                }
            }
            else if(sortOpcija == "turista")
            {
                if (sortNacin == "rastuce")
                {
                    list = list.OrderBy(x => x.Turista.KorisnickoIme).ToList();
                }
                else
                {
                    list = list.OrderByDescending(x => x.Turista.KorisnickoIme).ToList();
                }
            }
            else if (sortOpcija == "status")
            {
                if (sortNacin == "rastuce")
                {
                    list = list.OrderBy(x => x.StatusAktivna).ToList();
                }
                else
                {
                    list = list.OrderByDescending(x => x.StatusAktivna).ToList();
                }
            }
            else if (sortOpcija == "aranzman")
            {
                if (sortNacin == "rastuce")
                {
                    list = list.OrderBy(x => x.IzabraniAranzman.Naziv).ToList();
                }
                else
                {
                    list = list.OrderByDescending(x => x.IzabraniAranzman.Naziv).ToList();
                }
            }
            else
            {
                if (sortNacin == "rastuce")
                {
                    list = list.OrderBy(x => x.Jedinica.MaxBrojGostiju).ToList();
                }
                else
                {
                    list = list.OrderByDescending(x => x.Jedinica.MaxBrojGostiju).ToList();
                }
            }


            ViewBag.Rezervacije = list;

            return View("Rezervacije");
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

                if (tipPrevoza == "/")
                {
                    tipPrevoza = "";
                }
                if (tipAranzmana == "/")
                {
                    tipAranzmana = "";
                }

                List<Aranzman> filter = new List<Aranzman>();
                foreach (Aranzman a in HomeController.aranzmaniMenadzera)
                {
                    if (a.Naziv.ToLower().Contains(naziv.ToLower()) && a.DatumPocetka >= datumPocetkaOdDT && a.DatumPocetka <= datumPocetkaDoDT && a.DatumZavrsetka >= datumKrajaOdDT &&
                        a.DatumZavrsetka <= datumKrajaDoDT)
                    {
                        if (tipPrevoza == "")
                        {
                            if (tipAranzmana == "")
                            {
                                filter.Add(a);
                            }
                            else
                            {
                                if (a.TipAranzmana == tipAranzmana)
                                {
                                    filter.Add(a);
                                }
                            }
                        }
                        else
                        {
                            if (tipAranzmana == "")
                            {
                                if (a.TipPrevoza == tipPrevoza)
                                {
                                    filter.Add(a);
                                }
                            }
                            else
                            {
                                if (a.TipAranzmana == tipAranzmana && a.TipPrevoza == tipPrevoza)
                                {
                                    filter.Add(a);
                                }
                            }
                        }
                    }
                }

                string sortOpcija = Request["sortOpcija"];
                string sortNacin = Request["sortNacin"];

                if (sortOpcija == "naziv")
                {
                    if (sortNacin == "rastuce")
                    {
                        filter = filter.OrderBy(o => o.Naziv).ToList();
                    }
                    else
                    {
                        filter = filter.OrderByDescending(o => o.Naziv).ToList();
                    }
                }
                else if (sortOpcija == "datumPocetka")
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
                if (page == "index")
                {
                    return View("Index");
                }
                if (page == "protekli")
                {
                    return View("Protekli");
                }
            }

            return View("Index");

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
                foreach (Aranzman a in filter)
                {
                    int slobodne = 0;
                    foreach (SmestajnaJedinica j in a.SmestajAranzmana.Jedinice)
                    {
                        if(!a.SmestajAranzmana.Obrisan && !a.Obrisan)
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

            return View("Index");
        }

    }
}