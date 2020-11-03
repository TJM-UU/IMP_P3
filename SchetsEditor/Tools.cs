using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor
{
    public interface ISchetsTool
    {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p);
        void Letter(SchetsControl s, char c, Color k);
    }

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
        public abstract void Letter(SchetsControl s,  char c, Color k);
        public virtual void Compleet(Graphics g, Point p1, Point p2, Color c) { }
    }

    public class TekstTool : StartpuntTool
    {
        protected int Index;
        public override string ToString() { return "tekst"; }
        public override void MuisDrag(SchetsControl s, Point p) { }
        public override void Letter(SchetsControl s, char c, Color k)
        {
            if (c >= 32)
            {   if (kwast == null)
                    kwast = new SolidBrush(k);
                Graphics gr = s.MaakBitmapGraphics();
                Font font = new Font("Tahoma", 40);
                string tekst = c.ToString();
                SizeF sz = gr.MeasureString(tekst, font, startpunt, StringFormat.GenericTypographic);
                gr.DrawString(tekst, font, kwast, startpunt, StringFormat.GenericTypographic);
                startpunt.X += (int)sz.Width;
                Compact com = s.Schets.Getekend[this.Index];
                if(com.soort.ToString() == "tekst")
                {
                    com.eind.X += (int)sz.Width;
                    com.eind.Y = com.begin.Y + (int)sz.Height;
                }
                s.Invalidate();
            }
        }
        public virtual void Woord(SchetsControl sc, Point p1, string s, Color k, int index)
        {   if(s != null)
            {
                this.Index = index;
                startpunt = p1;
                foreach (char c in s)
                    Letter(sc, c, k);
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
            this.Bezig(s.CreateGraphics(), this.startpunt, p, s.PenKleur);
        }
        public override void MuisLos(SchetsControl s, Point p)
        {   base.MuisLos(s, p);
            Compleet(s.MaakBitmapGraphics(), startpunt, p, s.PenKleur);
            s.Invalidate();
        }
        public override void Letter(SchetsControl s, char c, Color k)
        {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2, Color c);
        
        public override void Compleet(Graphics g, Point p1, Point p2, Color c)
        {   this.Bezig(g, p1, p2, c);
        }
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2, Color c)
        {   g.DrawRectangle(MaakPen(new SolidBrush(c),3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }
    
    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void Compleet(Graphics g, Point p1, Point p2, Color c)
        {   g.FillRectangle(new SolidBrush(c), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }
    //
    public class EllipsTool : TweepuntTool
    {
        public override string ToString() { return "ovaal"; }

        public override void Bezig(Graphics g, Point p1, Point p2, Color c)
        {
            g.DrawEllipse(MaakPen(new SolidBrush(c), 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    public class VolEllipsTool : EllipsTool
    {
        public override string ToString() { return "schijf"; }

        public override void Compleet(Graphics g, Point p1, Point p2, Color c)
    {
            g.FillEllipse(new SolidBrush(c), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }
    //
    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2, Color c)
        {   g.DrawLine(MaakPen(new SolidBrush(c),3), p1, p2);
        }
    }

    public class PenTool : LijnTool
    {
        public override string ToString() { return "pen"; }

        public override void MuisDrag(SchetsControl s, Point p)
        {   this.MuisLos(s, p);
            this.MuisVast(s, p);
        }
        public void Punten(Graphics g, Point p1, Point p2, Color c)
        {
            base.Compleet(g, p1, p2,c);
        }
    }
    //
    public class GumTool : StartpuntTool
    {
        public override string ToString() { return "gum"; }

        public override void MuisVast(SchetsControl s, Point p)
        {
            List<Compact> ls = s.Schets.Getekend;
            int index = ls.Count-1;
            for (int i = index; i >= 0; i--)
                if (ls[i].Raak(p))
                {
                    ls.RemoveAt(i);
                    i = -1;
                }
        }
        public override void MuisLos(SchetsControl s, Point p)
        {
            s.Schets.LijstNaarGraphics(s);
            s.Invalidate();
        }
        public override void MuisDrag(SchetsControl s, Point p)
        {   
        }
        public override void Letter(SchetsControl s, char c, Color k)
        {
        }
    }//
}
