using System;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Forms.VisualStyles;

namespace SchetsEditor
{
    public class Schets
    {
        private Bitmap bitmap;
        // Aanmaken van de lijsten die nodig zijn voor het Openen, Gummen en Undo.
        public List<SchetsElement> Getekend;
        public List<SchetsElement> UndoList;

        public Schets()
        {   
            bitmap = new Bitmap(1, 1);
            // Maak de lijsten Getekend en UndoList aan bij de aanmaak van een schets.
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
        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
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
        // De Undo en Redo is in essentie een uitwisseling tussen Getekend en UndoList.
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

        public void LijstNaarGraphics(SchetsControl sc)
        {   // Maak de huidige bitmap leeg.
            Graphics gr = sc.MaakBitmapGraphics();
            gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
            // Kies voor elk SchetsElement in de lijst de juiste teken methode.
            for(int i = 0; i < this.Getekend.Count; i++)
                KiesMethode(this.Getekend[i],sc,gr);
            sc.Invalidate();

        }
        public void KiesMethode(SchetsElement c, SchetsControl sc, Graphics gr)
        {   // Heeft het element een tekst tool als soort, teken het woord.
            if (c.soort.ToString() == "tekst")
                ((TekstTool)c.soort).Woord(sc, c.begin, c.tekst, c.kleur, sc.Schets.Getekend.IndexOf(c));
            // Is het een pen, maak voor elk twee op een volgende punten een lijn.
            else if (c.soort.ToString() == "pen")
                for (int i = 0; i < c.punten.Count-1; i++)
                    ((PenTool)c.soort).Compleet(gr, c.punten[i],c.punten[i+1], c.kleur);
            else // Kies anders de Compleet methode.
                ((TweepuntTool)c.soort).Compleet(gr, c.begin, c.eind, c.kleur);
            sc.Invalidate();
        }
    }
}
