using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;

namespace SchetsEditor
{
    public class Schets
    {
        private Bitmap bitmap;
        //
        public List<SchetsElement> Getekend;
        public List<SchetsElement> UndoList;
        //
        public Schets()
        {
            bitmap = new Bitmap(1, 1);
            Getekend = new List<SchetsElement>();
            UndoList = new List<SchetsElement>();
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

        public void Undo()
        {
            VerplaatsElementen(Getekend, UndoList);
        }
        public void Redo()
        {
            VerplaatsElementen(UndoList,Getekend);

        }
        // Kopieer het een-na-laatste item van de ls-lijst (het laatste item is een lege lijn) naar de us-lijst. 
        // 'if (ls.Count > 0)' vermijdt dat er een System.ArgumentOutOfRangeException-exception optreed.
        public static void VerplaatsElementen(List<SchetsElement> ls, List<SchetsElement> us)
        {
            if (ls.Count > 0)
            {
                us.Add(ls[ls.Count - 1]);
                ls.RemoveAt(ls.Count - 1);
            }
        }

        public void Teken(Graphics gr)
        {
            gr.DrawImage(bitmap, 0, 0);
        }
        public void Schoon()
        {   // Voor het aanmaken van een nieuwe bitmap willen we ook de lijst van getekende elementen leeg maken.
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
            for(int i = 0; i < this.Getekend.Count; i++)
                KiesMethode(this.Getekend[i],sc,gr);
            sc.Invalidate();

        }
        public void KiesMethode(SchetsElement c, SchetsControl sc, Graphics gr)
        {
            if (c.soort.ToString() == "tekst")
                ((TekstTool)c.soort).Woord(sc, c.begin, c.tekst, c.kleur, sc.Schets.Getekend.IndexOf(c));
            else if (c.soort.ToString() == "pen")
                for (int i = 0; i < c.punten.Count-1; i++)
                    ((PenTool)c.soort).Compleet(gr, c.punten[i],c.punten[i+1], c.kleur);
            else
                ((TweepuntTool)c.soort).Compleet(gr, c.begin, c.eind, c.kleur);
            sc.Invalidate();
        }
    }
}
