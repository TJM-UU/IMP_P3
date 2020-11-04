using System;
using System.Drawing;
using System.Collections.Generic;

namespace SchetsEditor
{
    public class Schets
    {
        private Bitmap bitmap;
        // De Getekend-lijst bewaart alle getekende schetsobjecten als strings, de UndoList bewaart alle verwijderde schetsobjecten. Als een nieuw schetsobject wordt gemaakt, wordt de UndoList geleegd. 
        public List<Compact> Getekend;
        public List<Compact> UndoList;
        //
        public Schets()
        {
            bitmap = new Bitmap(1, 1);
            Getekend = new List<Compact>();
            UndoList = new List<Compact>();
        }
        public Graphics BitmapGraphics
        {
            get { return Graphics.FromImage(bitmap); }
        }
        public void VeranderAfmeting(Size sz)
        {
            if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
            {
                Bitmap nieuw = new Bitmap( Math.Max(sz.Width,  bitmap.Size.Width)
                                         , Math.Max(sz.Height, bitmap.Size.Height)
                                         );
                Graphics gr = Graphics.FromImage(nieuw);
                gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
                gr.DrawImage(bitmap, 0, 0);
                bitmap = nieuw;
            }
        }
        // Kopieer het een-na-laatste item van de ls-lijst (het laatste item is een lege lijn) naar de ul-lijst. 'if (ls.Count > 0)' vermijdt dat er een System.ArgumentOutOfRangeException-exception optreed.
        public void Undo()
        {
            List<Compact> ls = Getekend;
            List<Compact> ul = UndoList;
            if (ls.Count > 0)
            {
                ul.Add(ls[ls.Count - 1]);
                ls.RemoveAt(ls.Count - 1);
            }
        }
        // Kopieer het een-na-laatste item van de ul-lijst (het laatste item is een lege lijn) naar de ls-lijst. 'if (ul.Count > 0)' vermijdt dat er een System.ArgumentOutOfRangeException-exception optreed.
        public void Redo()
        {
            List<Compact> ls = Getekend;
            List<Compact> ul = UndoList;
            if (ul.Count > 0)
            {
                ls.Add(ul[ul.Count - 1]);
                ul.RemoveAt(ul.Count - 1);
            }
            
        }
        public void Teken(Graphics gr)
        {
            gr.DrawImage(bitmap, 0, 0);
        }
        public void Schoon()
        {
            Getekend.Clear();
            Graphics gr = Graphics.FromImage(bitmap);
            gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
        }
        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        public void LijstNaarGraphics(SchetsControl sc)
        {
            Graphics gr = sc.MaakBitmapGraphics();
            gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
            List<Compact> ls = sc.Schets.Getekend;
            for(int i = 0; i < ls.Count; i++)
                KiesMethode(ls[i],sc,gr);
            sc.Invalidate();

        }
        public void KiesMethode(Compact c, SchetsControl sc, Graphics gr)
        {
            if (c.soort.ToString() == "tekst")
                ((TekstTool)c.soort).Woord(sc, c.begin, c.tekst, c.kleur, sc.Schets.Getekend.IndexOf(c));
            else if (c.soort.ToString() == "pen")
                for (int i = 0; i < c.punten.Count-1; i++)
                    ((PenTool)c.soort).Punten(gr, c.punten[i],c.punten[i+1], c.kleur);
            else
                ((TweepuntTool)c.soort).Compleet(gr, c.begin, c.eind, c.kleur);
            sc.Invalidate();
        }
    }
}
