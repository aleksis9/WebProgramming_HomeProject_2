using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TuristickaAgencija.Models;

namespace TuristickaAgencija.Controllers
{
    public class TuristaController : Controller
    {
        private static Korisnik login;
        public static List<Smestaj> smestaji;

        // GET: Turista
        public ActionResult Index()
        {
            smestaji = new List<Smestaj>();
            foreach(Korisnik k in Controllers.HomeController.menadzeri)
            {
                foreach(Aranzman a in k.ListaAranzmana)
                {
                    smestaji.Add(a.SmestajAranzmana);
                }
            }

            login = Controllers.HomeController.login;

            return View();
        }

        public ActionResult Rezervacije()
        {
            smestaji = new List<Smestaj>();
            foreach (Korisnik k in Controllers.HomeController.menadzeri)
            {
                foreach (Aranzman a in k.ListaAranzmana)
                {
                    smestaji.Add(a.SmestajAranzmana);
                }
            }

            login = Controllers.HomeController.login;

            return View();
        }

        public ActionResult Rezervisi()
        {
            smestaji = new List<Smestaj>();
            foreach (Korisnik k in Controllers.HomeController.menadzeri)
            {
                foreach (Aranzman a in k.ListaAranzmana)
                {
                    smestaji.Add(a.SmestajAranzmana);
                }
            }

            login = Controllers.HomeController.login;


            string smestaj = Request["smestaj"];
            Smestaj moj = new Smestaj();
            string brojGostiju = Request["broj"];
            string ljubimci = Request["ljubimci"];
            string cena = Request["cena"];
            
            foreach(Smestaj s in smestaji)
            {
                if (s.Naziv == smestaj)
                {
                    moj = s;
                    break;
                }
            }
            foreach(SmestajnaJedinica j in moj.Jedinice)
            {
                if(j.CenaZaCeluJedinicu == double.Parse(cena) && j.DozvoljeniKucniLjubimci.ToString() == ljubimci && j.MaxBrojGostiju == Int32.Parse(brojGostiju))
                {
                    j.Rezervisana = true;
                    //dodati novu rezervaciju ulogovanom korisniku
                    Rezervacija nova = new Rezervacija();
                    nova.Id = DateTime.Now.ToString("yyyyMMddHHmmss") + j.MaxBrojGostiju.ToString();
                    string aranzmanNaziv = Request["aranzman"];
                    foreach(Aranzman a in HomeController.aranzmaniMenadzera)
                    {
                        if(a.Naziv == aranzmanNaziv)
                        {
                            nova.IzabraniAranzman = a;
                            break;
                        }
                    }
                    nova.StatusAktivna = true;
                    nova.Jedinica = j;
                    nova.StatusAktivna = true;
                    nova.Turista = (Korisnik)Session["login"];
                    login.ListaRezervacija.Add(nova);
                    HomeController.login = login;
                    Session["login"] = HomeController.login;

                    //upisati novu rezervaciju u fajl
                    string path = login.KorisnickoIme.ToLower() + ".tsv";
                    string fullPath = Server.MapPath("~/App_Data/Rezervacije/" + path);
                    string tempFile = Path.GetTempFileName();

                    StreamReader sr = new StreamReader(fullPath);
                    StreamWriter sw = new StreamWriter(tempFile);

                    string line = "";

                    while ((line = sr.ReadLine()) != null)
                    {
                        sw.WriteLine(line);
                    }

                    string newLine = nova.Id.ToString() + "\t" + nova.Turista.KorisnickoIme + "\t" + nova.StatusAktivna.ToString() + "\t" + nova.IzabraniAranzman.Naziv;

                    sw.WriteLine(newLine);

                    sr.Close();
                    sw.Close();

                    System.IO.File.Delete(fullPath);
                    System.IO.File.Move(tempFile, fullPath);

                    break;
                }
            }

            return RedirectToAction("Rezervacije","Turista");
        }

        public ActionResult Komentarisi()
        {
            string nazivAranzmana = Request["aranzman"];
            foreach(Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if(a.Naziv == nazivAranzmana)
                {
                    TempData["aranzman"] = a;
                    break;
                }
            }
            return View();
        }

        public ActionResult KomentarisiClick()
        {
            if (!ValidateKomentar(Request))
            {
                return View("Komentarisi");
            }

            Komentar novi = new Komentar();
            novi.AranzmanKomentarisan = (Aranzman)TempData["aranzman"];
            novi.Turista = (Korisnik)Session["login"];
            novi.Tekst = Request["komentar"];
            novi.Ocena = Int32.Parse(Request["zvezdice"]);
            HomeController.komentari.Add(novi);
            TempData["message"] = "Komentar prosledjen menadzeru aranzmana! Ceka se odobrenje.";

            //sada se upisuje u fajl sa stanjem Ceka
            string registrovaniFullPath = Server.MapPath("~/App_Data/komentari.tsv");
            string tempFile = Path.GetTempFileName();

            StreamReader sr = new StreamReader(registrovaniFullPath);
            StreamWriter sw = new StreamWriter(tempFile);

            string line;
            string userPath = login.KorisnickoIme.ToLower() + ".tsv";
            //korisnicko ime, lozinka, ime, prezime, pol, email, datum, uloga, naziv fajla sa listom aranzmana/rezervacija, obrisan
            string newLine = login.KorisnickoIme + "\t" + novi.AranzmanKomentarisan.Naziv + "\t" + novi.Tekst + "\t" + novi.Ocena + "\t" + "Ceka";

            while ((line = sr.ReadLine()) != null)
            {
                sw.WriteLine(line);
            }
            sw.WriteLine(newLine);

            sr.Close();
            sw.Close();

            System.IO.File.Delete(registrovaniFullPath);
            System.IO.File.Move(tempFile, registrovaniFullPath);

            return RedirectToAction("Rezervacije");
        }

        private bool ValidateKomentar(HttpRequestBase request)
        {
            string tekst = request["komentar"];
            int zvezdice;
            try
            {
                zvezdice = Int32.Parse(request["zvezdice"]);
            }
            catch (Exception e)
            {
                TempData["message"] = "Broj zvezdica mora biti ceo broj!";
                return false;
            }
            if(zvezdice < 1 || zvezdice > 5)
            {
                TempData["message"] = "Broj zvezdica mora biti izmedju 1 i 5!";
                return false;
            }
            if(tekst == null || tekst == "")
            {
                TempData["message"] = "Morate uneti tekst komentara!";
                return false;
            }

            return true;
        }

        public ActionResult Otkazi()
        {
            string nazivAranzmana = Request["aranzman"];
            string jedBroj = Request["jedBroj"];
            string jedLjub = Request["jedLjub"];
            string jedCena = Request["jedCena"];

            string fullPath;
            string newLine;

            string[] parts;
            string line;
            string tempFile;

            Rezervacija moja = new Rezervacija();
            foreach(Rezervacija r in login.ListaRezervacija)
            {
                if(r.IzabraniAranzman.Naziv == nazivAranzmana)
                {
                    foreach (SmestajnaJedinica jed in r.IzabraniAranzman.SmestajAranzmana.Jedinice)
                    {
                        if(jed.MaxBrojGostiju == Int32.Parse(jedBroj) && jed.DozvoljeniKucniLjubimci.ToString() == jedLjub && jed.CenaZaCeluJedinicu == double.Parse(jedCena))
                        {
                            bool flag = false;
                            foreach(Smestaj s in smestaji)
                            {
                                foreach(SmestajnaJedinica j in s.Jedinice)
                                {
                                    if(j.MaxBrojGostiju == Int32.Parse(jedBroj) && j.DozvoljeniKucniLjubimci.ToString() == jedBroj && j.CenaZaCeluJedinicu == double.Parse(jedCena))
                                    {
                                        j.Rezervisana = false;
                                        flag = true;
                                        break;
                                    }
                                }
                                if (flag)
                                    break;
                            }

                            flag = false;
                            foreach (Aranzman a in HomeController.aranzmaniMenadzera)
                            {
                                foreach (SmestajnaJedinica j in a.SmestajAranzmana.Jedinice)
                                {                                   
                                    if (j.MaxBrojGostiju == Int32.Parse(jedBroj) && j.DozvoljeniKucniLjubimci.ToString() == jedBroj && j.CenaZaCeluJedinicu == double.Parse(jedCena))
                                    {
                                        j.Rezervisana = false;
                                        flag = true;
                                        break;
                                    }
                                   
                                    if (flag)
                                        break;
                                }
                            }

                            jed.Rezervisana = false;
                            //upisi u fajl da je otkazana smestajna jedinica
                            fullPath = Server.MapPath("~/App_Data/Smestaji/" + r.IzabraniAranzman.SmestajAranzmana.Naziv.Replace(" ", "").ToLower() + ".tsv");
                            newLine = jed.MaxBrojGostiju + "\t" + jed.DozvoljeniKucniLjubimci.ToString() + "\t" + jed.CenaZaCeluJedinicu.ToString() + "\t";
                            if (jed.Rezervisana)
                            {
                                newLine += "1";
                            }
                            else
                            {
                                newLine += "0";
                            }
                            newLine += "\t" + "0";

                            tempFile = Path.GetTempFileName();

                            StreamReader sr2 = new StreamReader(fullPath);
                            StreamWriter sw2 = new StreamWriter(tempFile);

                            //prva linija detalji smestaja
                            line = sr2.ReadLine();
                            sw2.WriteLine(line);

                            //detalji jedinica
                            while ((line = sr2.ReadLine()) != null)
                            {
                                parts = line.Split('\t');
                                if (jed.MaxBrojGostiju.ToString() == parts[0] && jed.DozvoljeniKucniLjubimci.ToString() == parts[1] && 
                                    jed.CenaZaCeluJedinicu.ToString() == parts[2] && "1" == parts[3] && jed.Obrisana.ToString() == parts[4])
                                {
                                    //umesto ovog reda prepisati nove podatke
                                    sw2.WriteLine(newLine);
                                }
                                else
                                    sw2.WriteLine(line);
                            }

                            sr2.Close();
                            sw2.Close();

                            System.IO.File.Delete(fullPath);
                            System.IO.File.Move(tempFile, fullPath);

                            r.StatusAktivna = false;
                            moja = r;
                            
                            break;
                        }
                    }
                }
            }

            login.BrojOtkazivanja++;

            HomeController.login = login;
            Session["login"] = login;

            //promeni u fajlu stanje rezervacije 
            fullPath = Server.MapPath("~/App_Data/Rezervacije/" + login.KorisnickoIme.ToLower() + ".tsv");
            newLine = moja.Id + "\t" + login.KorisnickoIme + "\t" + moja.StatusAktivna + "\t" + nazivAranzmana;

            tempFile = Path.GetTempFileName();

            StreamReader sr = new StreamReader(fullPath);
            StreamWriter sw = new StreamWriter(tempFile);

            while ((line = sr.ReadLine()) != null)
            {
                parts = line.Split('\t');
                if (moja.Id == parts[0])
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

            //povecaj broj otkazivanja u fajlu kod korisnika
            fullPath = Server.MapPath("~/App_Data/turisti.tsv");
            newLine = login.KorisnickoIme + "\t" + login.Lozinka + "\t" + login.Ime + "\t" + login.Prezime + "\t" + login.Pol + "\t" + login.Email + "\t" +
                login.DatumRodjenja.ToString("dd'/'MM'/'yyyy") + "\t" + "Turista" + "\t" + login.KorisnickoIme.ToLower() + ".tsv" + "\t" + "0" + "\t" + login.BrojOtkazivanja.ToString();

            tempFile = Path.GetTempFileName();

            StreamReader sr1 = new StreamReader(fullPath);
            StreamWriter sw1 = new StreamWriter(tempFile);

            while ((line = sr1.ReadLine()) != null)
            {
                parts = line.Split('\t');
                if (login.KorisnickoIme == parts[0])
                {
                    //umesto ovog reda prepisati nove podatke
                    sw1.WriteLine(newLine);
                }
                else
                    sw1.WriteLine(line);
            }

            sr1.Close();
            sw1.Close();

            System.IO.File.Delete(fullPath);
            System.IO.File.Move(tempFile, fullPath);

            return RedirectToAction("Rezervacije");
        }

        public ActionResult Komentar()
        {
            string nazAranzmana = Request["aranzman"];

            Aranzman moj = new Aranzman();
            List<Komentar> komentari = new List<Komentar>();

            foreach(Aranzman a in HomeController.aranzmaniMenadzera)
            {
                if(a.Naziv == nazAranzmana)
                {
                    moj = a;
                    break;
                }
            }

            foreach(Komentar k in HomeController.komentari)
            {
                if(k.Turista.KorisnickoIme == login.KorisnickoIme && k.AranzmanKomentarisan.Naziv == moj.Naziv)
                {
                    komentari.Add(k);
                }
            }

            TempData["aranzman"] = moj;
            ViewBag.Komentari = komentari;

            return View();
        }
    }
}