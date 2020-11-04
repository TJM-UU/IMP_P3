using System;
using System.Collections.Generic;
using System.Drawing;

namespace SchetsEditor
{
    public class SchetsElement
    {   // member variabelen
        public ISchetsTool soort;
        public Point begin, eind;
        public string tekst;
        public Color kleur;
        public List<Point> punten;

        public SchetsElement(ISchetsTool s, Point b, Color k)
        {
            this.begin = b;
            this.soort = s;
            this.kleur = k;
            punten = new List<Point>();
            this.eind = b;
        }
        
        public override string ToString()
        {   // Methode voor het creeëren van een string die gemakkelijk kan worden opgeslagen in een tekstbestand
            return soort.ToString() + " " +
                    begin.X.ToString() + " " +
                    begin.Y.ToString() + " " +
                    eind.X.ToString() + " " +
                    eind.Y.ToString() + " " +
                    kleur.Name + " " +
                    tekst +
                    puntenToString(punten);
        }

        private static string puntenToString(List<Point> ls)
        {   // Gaat lijst van punten af om hier een sting van x en y coordinaten van te maken.
            string res = "";
            foreach (Point p in ls)
            {
                res += " " + p.X.ToString() + " " + p.Y.ToString();
            }
            return res;
        }
        public bool Raak(Point p)
        {   // Bepaal aan de hand van de member variabele soort de juist methode om te checken of er raak is geklikt op dat SchetsElement
            string x = this.soort.ToString();
            bool res = false;
            switch (x)
            {
                case "tekst": res = RaakRechthoek(p, begin, eind); break;
                case "kader": res = RaakKaderOfOvaal(x,p, begin, eind); break;
                case "vlak": res = RaakRechthoek(p, begin, eind); break;
                case "ovaal": res = RaakKaderOfOvaal(x,p, begin, eind); break;
                case "schijf": res = RaakSchijf(p, begin, eind); break;
                case "lijn": res = RaakLijn(p, begin, eind); break;
                default: res = RaakPen(p, punten); break;
            }
            return res;
        }

        private static bool RaakKaderOfOvaal(string s,Point p, Point b, Point e)
        {   // Methode begin met twee begin en eind punten definieren die een gebied wat groter en kleiner is.
            int gemak = 5;
            Point groteb = new Point(b.X - gemak, b.Y - gemak);
            Point kleineb = new Point(b.X + gemak, b.Y + gemak);
            Point grotee = new Point(e.X + gemak, e.Y + gemak);
            Point kleinee = new Point(e.X - gemak, e.Y - gemak);
            bool res = false;
            // Als we te maken hebben met een rechthoek: Ligt punt wel op de buitenste rechthoek en niet op de binnenste? Dan true.
            if (RaakRechthoek(p, groteb, grotee) && !(RaakRechthoek(p, kleineb, kleinee)) && s == "kader")
                res = true;
            // Hetzelfde voor Ovaal: Ligt punt wel op de buitenste Ovaal en niet op de binnenste? Dan true.
            if (RaakSchijf(p, groteb, grotee) && !(RaakSchijf(p, kleineb, kleinee)) && s == "ovaal") 
                res = true;
            return res;
        }
        private static bool RaakRechthoek(Point p, Point b, Point e)
        {   // Controleer of alle x en y coordinaten juist begrensd zijn.
            return p.X > b.X && p.X < e.X && p.Y > b.Y && p.Y < e.Y;
        }
        private static bool RaakSchijf(Point p, Point b, Point e)
        {   // https://nl.wikipedia.org/wiki/Ellips_(wiskunde) met de a en b vervangen door R1 en R2
            double R1 = (e.X - b.X) / 2;
            double R2 = (e.Y - b.Y) / 2;
            double x0 = b.X + R1;
            double y0 = b.Y + R2;
            double term1 = (p.X - x0) / R1;
            double term2 = (p.Y - y0) / R2;
            bool expressie = term1 * term1 + term2 * term2 < 1;
            return expressie;
        }
        private static bool RaakLijn(Point p, Point b, Point e)
        {   //https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line met P_1 = b, P_2 = e en p = (x_0,y_0).
            //en na problemen met alleen lijn: gebruiken we nu lijnsegmenten: https://math.stackexchange.com/questions/867721/distance-from-a-point-to-line-segment-not-it-s-perpendicular-lines-distance antwoord 3.
            int gemak = 5;
            int teller = Math.Abs(((e.Y - b.Y) * p.X) - ((e.X - b.X) * p.Y) + (e.X * b.Y) - (e.Y * b.X));
            double noemer = Math.Sqrt(((e.Y - b.Y) * (e.Y - b.Y)) + ((e.X - b.X) * (e.X - b.X)));
            // Kies van deze drie waarden de besteafhandkelijk van de relatieve positie van p.
            double afstandformule = teller / noemer;
            double afstandtotbegin = Math.Sqrt(((p.Y - b.Y) * (p.Y - b.Y)) + ((p.X - b.X) * (p.X - b.X)));
            double afstandtoteind = Math.Sqrt(((e.Y - p.Y) * (e.Y - p.Y)) + ((e.X - p.X) * (e.X - p.X)));
            // Geeft ons de kennis of p geprojecteerd wordt op het lijnsegment of op een denkbeeldig verlengde.
            double inproductPB = (p.X - b.X) * (e.X - b.X) + (p.Y - b.Y) * (e.Y - b.Y);
            double inproductPE = (p.X - e.X) * (b.X - e.X) + (p.Y - e.Y) * (b.Y - e.Y);
            // Kies de juiste minimale afstand
            double minimaleafstand;
            if (inproductPB <= 0)
                minimaleafstand = afstandtotbegin;
            else if (inproductPE <= 0)
                minimaleafstand = afstandtoteind;
            else
                minimaleafstand = afstandformule;
            // Voldoet de minimale afstand aan de maximaal toegestaande afstand (=gemak)?
            bool expressie = minimaleafstand < gemak;
            return expressie;
        }
        private static bool RaakPen(Point p, List<Point> ps)
        {   // Ga voor alle punten na of de (korte) lijn tussen twee op een volgende punten geraakt wordt.
            // Als dit ook maar 1 keer gebeurd kan er direct gestopt worden.
            for (int i = 0; i < ps.Count - 1; i++)
                if (RaakLijn(p, ps[i], ps[i + 1]))
                    return true;
            return false;
        }
    }
}
