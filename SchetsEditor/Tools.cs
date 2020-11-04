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
        // Voeg kleur toe als parameter om ook uit een SchetsElement te kunnen tekenen.
        void Letter(SchetsControl s, char c, Color k);
    }

    public abstract class StartpuntTool : ISchetsTool
    {
        protected Point startpunt;
        protected Brush kwast;
        public virtual void MuisVast(SchetsControl s, Point p)
        {   startpunt = p;
        }
        public abstract void MuisLos(SchetsControl s, Point p);
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s,  char c, Color k);
    }

    public class TekstTool : StartpuntTool
    {   // Extra member variabele
        protected int Index;
        public override string ToString() { return "tekst"; }
        public override void MuisDrag(SchetsControl s, Point p) { }
        public override void MuisLos(SchetsControl s, Point p) { }
        public override void Letter(SchetsControl s, char c, Color k)
        {
            if (c >= 32)
            {   
                kwast = new SolidBrush(k);
                Graphics gr = s.MaakBitmapGraphics();
                Font font = new Font("Tahoma", 40);
                string tekst = c.ToString();
                SizeF sz = gr.MeasureString(tekst, font, startpunt, StringFormat.GenericTypographic);
                gr.DrawString(tekst, font, kwast, startpunt, StringFormat.GenericTypographic);
                startpunt.X += (int)sz.Width;
                // Kies het juiste element uit de lijst met behulp van de index.
                // behoud deze index hetzelfde zodat je bij dat element het eindpunt kan aanpassen.
                SchetsElement elem;
                if (Index < s.Schets.Getekend.Count)
                {
                    elem = s.Schets.Getekend[this.Index];
                    if (elem.soort.ToString() == "tekst")
                    {   // Dit verschoven eindpunt is nodig voor het methode Raak in SchetsElement (Rechthoek)
                        elem.eind.X += (int)sz.Width;
                        elem.eind.Y = elem.begin.Y + (int)sz.Height;
                    }
                }
                s.Invalidate();
            }
        }
        public virtual void Woord(SchetsControl sc, Point p1, string s, Color k, int index)
        {   if(s != null)
            {   // als het woord gegeven staat in de SchetsElement, leg de index vast en teken elke letter 1 voor 1.
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
        {   Compleet(s.MaakBitmapGraphics(), startpunt, p, s.PenKleur);
            s.Invalidate();
        }
        public override void Letter(SchetsControl s, char c, Color k) { }
        // Voeg kleur toe als parameter om ook uit een SchetsElement te kunnen tekenen.
        public abstract void Bezig(Graphics g, Point p1, Point p2, Color c);
        // Voeg hier ook kleur toe als parameter om ook uit een SchetsElement te kunnen tekenen.
        public virtual void Compleet(Graphics g, Point p1, Point p2, Color c)
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
    public class EllipsTool : TweepuntTool
    {   // Opbouw van een ovaal is vergelijkbaar met een rechthoek.
        public override string ToString() { return "ovaal"; }

        public override void Bezig(Graphics g, Point p1, Point p2, Color c)
        {
            g.DrawEllipse(MaakPen(new SolidBrush(c), 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    public class VolEllipsTool : EllipsTool
    {   // Opbouw van een schijf is vergelijkbaar met een vlak.
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
    }

    public class GumTool : StartpuntTool
    {
        public override string ToString() { return "gum"; }

        public override void MuisVast(SchetsControl s, Point p)
        {   // Als je drukt op de schets, ga je de lijst van SchetsElementen langs en check je of 1 van deze elementen raak is.
            List<SchetsElement> ls = s.Schets.Getekend;
            int index = ls.Count-1;
            // We beginnen de forloop op de hoogste index, dit correspondeerd met de bovenste laag, en we werken langzaam door de tekening van nieuw naar oud.
            for (int i = index; i >= 0; i--)
                // Als deze raak is verwijder je deze uit de lijst. Door i=-1 stopt de for loop op die positie.
                if (ls[i].Raak(p))
                {
                    ls.RemoveAt(i);
                    i = -1;
                }
        }
        public override void MuisLos(SchetsControl s, Point p)
        {   // Als de muis los wordt gelaten wordt het scherm opnieuw getekend.
            // Dit gebeurd vanuit de lijst omdat de hele bitmap opnieuw moet worden aangemaakt.
            s.Schets.LijstNaarGraphics(s);
            s.Invalidate();
        }
        public override void MuisDrag(SchetsControl s, Point p){   }
        public override void Letter(SchetsControl s, char c, Color k){}
    }
}
