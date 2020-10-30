using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Schema;

namespace SchetsEditor
{
    public interface ISchetsTool
    {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p);
        void Letter(SchetsControl s, char c);
    }
    //
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
        {   string x = this.soort.ToString();
            bool res = false;
            switch (x)
            {
                case "tekst": res = RaakTekst(p,begin,eind); break;
                case "kader": res = RaakKader(p,begin,eind); break;
                case "vlak": res = RaakVlak(p,begin,eind); break;
                case "ovaal": res = RaakOvaal(p); break;
                case "schijf": res = RaakSchijf(p); break;
                case "lijn": res = RaakLijn(p); break;
                case "pen": res = RaakPen(p); break;
            }
            return res;
        }
        private static bool RaakTekst(Point p, Point b, Point e)
        {
            return RaakVlak(p,b,e);
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
            bool res = false;
            if (p.X > b.X && p.X < e.X && p.Y > b.X && p.Y < e.Y)
                res = true;
            return res;
        }
        private static bool RaakOvaal(Point p)
        {
            return true;
        }
        private static bool RaakSchijf(Point p)
        {
            return true;
        }
        private bool RaakLijn(Point p)
        {
            return true;
        }
        private bool RaakPen(Point p)
        {
            return true;
        }
    }//

    public abstract class StartpuntTool : ISchetsTool
    {
        protected Point startpunt;
        protected Brush kwast;
        public virtual void MuisVast(SchetsControl s, Point p)
        {   startpunt = p;
        }
        public virtual void MuisLos(SchetsControl s, Point p)
        {   kwast = new SolidBrush(s.PenKleur);
        }
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);
    }

    public class TekstTool : StartpuntTool
    {
        public override string ToString() { return "tekst"; }
        public override void MuisDrag(SchetsControl s, Point p) { }
        public override void Letter(SchetsControl s, char c)
        {
            if (c >= 32)
            {
                Graphics gr = s.MaakBitmapGraphics();
                Font font = new Font("Tahoma", 40);
                string tekst = c.ToString();
                SizeF sz = 
                gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
                gr.DrawString   (tekst, font, kwast, 
                                              this.startpunt, StringFormat.GenericTypographic);
                startpunt.X += (int)sz.Width;
                s.Invalidate();
            }
        }
    }

    public abstract class TweepuntTool : StartpuntTool
    {
        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {   return new Rectangle( new Point(Math.Min(p1.X,p2.X), Math.Min(p1.Y,p2.Y))
                                , new Size (Math.Abs(p1.X-p2.X), Math.Abs(p1.Y-p2.Y))
                                );
        }
        public static Pen MaakPen(Brush b, int dikte)
        {   Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }
        public override void MuisVast(SchetsControl s, Point p)
        {   base.MuisVast(s, p);
            kwast = Brushes.Gray;
        }
        public override void MuisDrag(SchetsControl s, Point p)
        {   s.Refresh();
            this.Bezig(s.CreateGraphics(), this.startpunt, p);
        }
        public override void MuisLos(SchetsControl s, Point p)
        {   base.MuisLos(s, p);
            this.Compleet(s.MaakBitmapGraphics(), this.startpunt, p);
            s.Invalidate();
        }
        public override void Letter(SchetsControl s, char c)
        {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2);
        
        public virtual void Compleet(Graphics g, Point p1, Point p2)
        {   this.Bezig(g, p1, p2);
        }
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {   g.DrawRectangle(MaakPen(kwast,3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }
    
    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void Compleet(Graphics g, Point p1, Point p2)
        {   g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }
    //
    public class EllipsTool : TweepuntTool
    {
        public override string ToString() { return "ovaal"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawEllipse(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    public class VolEllipsTool : EllipsTool
    {
        public override string ToString() { return "schijf"; }

        public override void Compleet(Graphics g, Point p1, Point p2)
        {
            g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }
    //
    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {   g.DrawLine(MaakPen(this.kwast,3), p1, p2);
        }
    }

    public class PenTool : LijnTool
    {
        public override string ToString() { return "pen"; }

        public override void MuisDrag(SchetsControl s, Point p)
        {   this.MuisLos(s, p);
            this.MuisVast(s, p);
        }
    }
    //
    public class GumTool : StartpuntTool
    {
        public override string ToString() { return "gum"; }

        public override void MuisVast(SchetsControl s, Point p)
        {  
        }
        public override void MuisLos(SchetsControl s, Point p)
        {
            List<Compact> ls = s.Schets.Getekend;
            Compact MagWeg = null;
            foreach (Compact c in ls)
                if (c.Raak(p) && ls.IndexOf(c) > ls.IndexOf(MagWeg))
                    MagWeg = c;
            ls.Remove(MagWeg);
        }
        public override void MuisDrag(SchetsControl s, Point p)
        {   
        }
        public override void Letter(SchetsControl s, char c)
        {
        }
        
    }//
}
