using System;
using System.Collections.Generic;
using System.Drawing;

namespace SchetsEditor
{
    public class Compact
    {
        public ISchetsTool soort;
        public Point begin, eind;
        public string tekst;
        public Color kleur;
        public List<Point> punten;

        public Compact(ISchetsTool s, Point b, Color k)
        {
            this.begin = b;
            this.soort = s;
            this.kleur = k;
            punten = new List<Point>();
            this.eind = b;
        }
        public override string ToString()
        {
            return soort.ToString() + " " +
                    begin.X.ToString() + " " +
                    begin.Y.ToString() + " " +
                    eind.X.ToString() + " " +
                    eind.Y.ToString() + " " +
                    kleur.Name + " " +
                    tekst +
                    puntenToString(punten) + "\n";
        }
        private static string puntenToString(List<Point> ls)
        {
            string res = "";
            foreach (Point p in ls)
            {
                res += " " + p.X.ToString() + " " + p.Y.ToString();
            }
            return res;
        }
        public bool Raak(Point p)
        {
            string x = this.soort.ToString();
            bool res = false;
            switch (x)
            {
                case "tekst": res = RaakTekst(p, begin, eind); break;
                case "kader": res = RaakKader(p, begin, eind); break;
                case "vlak": res = RaakVlak(p, begin, eind); break;
                case "ovaal": res = RaakOvaal(p, begin, eind); break;
                case "schijf": res = RaakSchijf(p, begin, eind); break;
                case "lijn": res = RaakLijn(p, begin, eind); break;
                default: res = RaakPen(p, punten); break;
            }
            return res;
        }
        private static bool RaakTekst(Point p, Point b, Point e)
        {
            return RaakVlak(p, b, e);
        }
        private static bool RaakKader(Point p, Point b, Point e)
        {
            int gemak = 5;
            Point groteb = new Point(b.X - gemak, b.Y - gemak);
            Point kleineb = new Point(b.X + gemak, b.Y + gemak);
            Point grotee = new Point(e.X + gemak, e.Y + gemak);
            Point kleinee = new Point(e.X - gemak, e.Y - gemak);
            bool res = false;
            if (RaakVlak(p, groteb, grotee) && !(RaakVlak(p, kleineb, kleinee)))
                res = true;
            return res;
        }
        private static bool RaakVlak(Point p, Point b, Point e)
        {
            return p.X > b.X && p.X < e.X && p.Y > b.Y && p.Y < e.Y;
        }
        private static bool RaakOvaal(Point p, Point b, Point e)
        {
            int gemak = 5;
            Point groteb = new Point(b.X - gemak, b.Y - gemak);
            Point kleineb = new Point(b.X + gemak, b.Y + gemak);
            Point grotee = new Point(e.X + gemak, e.Y + gemak);
            Point kleinee = new Point(e.X - gemak, e.Y - gemak);
            bool res = false;
            if (RaakSchijf(p, groteb, grotee) && !(RaakSchijf(p, kleineb, kleinee)))
                res = true;
            return res;
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
            //en na problemen van lijn ipv lijnsegmenten ook: https://math.stackexchange.com/questions/867721/distance-from-a-point-to-line-segment-not-it-s-perpendicular-lines-distance antwoord 3.
            int gemak = 5;
            int teller = Math.Abs(((e.Y - b.Y) * p.X) - ((e.X - b.X) * p.Y) + (e.X * b.Y) - (e.Y * b.X));
            double noemer = Math.Sqrt(((e.Y - b.Y) * (e.Y - b.Y)) + ((e.X - b.X) * (e.X - b.X)));
            double afstandformule = teller / noemer;
            double afstandtotbegin = Math.Sqrt(((p.Y - b.Y) * (p.Y - b.Y)) + ((p.X - b.X) * (p.X - b.X)));
            double afstandtoteind = Math.Sqrt(((e.Y - p.Y) * (e.Y - p.Y)) + ((e.X - p.X) * (e.X - p.X)));
            double inproductPB = (p.X - b.X) * (e.X - b.X) + (p.Y - b.Y) * (e.Y - b.Y);
            double inproductPE = (p.X - e.X) * (b.X - e.X) + (p.Y - e.Y) * (b.Y - e.Y);
            double minimaleafstand;
            if (inproductPB <= 0)
                minimaleafstand = afstandtotbegin;
            else if (inproductPE <= 0)
                minimaleafstand = afstandtoteind;
            else
                minimaleafstand = afstandformule;
            bool expressie = minimaleafstand < gemak;
            return expressie;
        }
        private static bool RaakPen(Point p, List<Point> ps)
        {
            bool res = false;
            for (int i = 0; i < ps.Count - 1; i++)
                if (RaakLijn(p, ps[i], ps[i + 1]))
                    res = true;
            return res;
        }
    }
}
