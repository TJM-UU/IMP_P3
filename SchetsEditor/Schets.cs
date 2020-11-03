using System;
using System.Drawing;
using System.Collections.Generic;

namespace SchetsEditor
{
    public class Schets
    {
        private Bitmap bitmap;
        //
        public List<Compact> Getekend;
        //
        public Schets()
        {
            bitmap = new Bitmap(1, 1);
            Getekend = new List<Compact>();
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
