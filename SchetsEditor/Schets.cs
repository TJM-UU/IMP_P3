using System;
using System.Drawing;
using System.Windows.Forms;
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
            Graphics gr = Graphics.FromImage(bitmap);
            gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
        }
        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        public void LijstNaarGraphics(SchetsControl sc)
        {
            List<Compact> ls = sc.Schets.Getekend;
            foreach(Compact c in ls)
                KiesMethode(c,sc);

        }
        public void KiesMethode(Compact c, SchetsControl sc)
        {
            Schoon();
            Graphics gr = Graphics.FromImage(bitmap);
            if (c.soort.ToString() == "tekst")
                ((TekstTool) c.soort).Woord(sc, c.tekst);
            else if (c.soort.ToString() == "pen")
                foreach(Point p in c.punten)
                    ((PenTool)c.soort).Punten(sc.MaakBitmapGraphics(), p, c.kleur);
            else
                ((TweepuntTool) c.soort).Compleet(sc.MaakBitmapGraphics(), c.begin,c.eind,c.kleur);
            sc.Invalidate();
        }
    }
}
