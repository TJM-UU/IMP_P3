using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Linq;

namespace SchetsEditor
{
    public class SchetsWin : Form
    {   
        MenuStrip menuStrip;
        SchetsControl schetscontrol;
        ISchetsTool huidigeTool;
        Panel paneel;
        bool vast;
        ResourceManager resourcemanager
            = new ResourceManager("SchetsEditor.Properties.Resources"
                                 , Assembly.GetExecutingAssembly()
                                 );
        // Nieuwe member variabele
        private SchetsElement tijdelijk;
        private void veranderAfmeting(object o, EventArgs ea)
        {
            schetscontrol.Size = new Size ( this.ClientSize.Width  - 70
                                          , this.ClientSize.Height - 50);
            paneel.Location = new Point(64, this.ClientSize.Height - 30);
        }

        private void klikToolMenu(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((ToolStripMenuItem)obj).Tag;
        }

        private void klikToolButton(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((RadioButton)obj).Tag;
        }

        private void afsluiten(object obj, EventArgs ea)
        {
            this.Close();
        }

        public SchetsWin()
        {
            ISchetsTool[] deTools = { new PenTool()
                                    , new LijnTool()
                                    , new RechthoekTool()
                                    , new VolRechthoekTool()
                                    , new EllipsTool()
                                    , new VolEllipsTool()
                                    , new TekstTool()
                                    , new GumTool()
                                    };
            String[] deKleuren = { "Black", "Red", "Green", "Blue"
                                 , "Yellow", "Magenta", "Cyan" 
                                 };

            this.ClientSize = new Size(700, 500);
            huidigeTool = deTools[0];
            schetscontrol = new SchetsControl();
            schetscontrol.Location = new Point(64, 10);
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       {   vast=true;
                                           huidigeTool.MuisVast(schetscontrol, mea.Location);
                                           // Aanmaak van het tijdelijke SchetsElement object.
                                           tijdelijk = new SchetsElement(huidigeTool, mea.Location, schetscontrol.PenKleur);
                                           // Leegt de UndoList als je iets probeert te tekenen.
                                           schetscontrol.Schets.UndoList.Clear();
                                       };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisDrag(schetscontrol, mea.Location);
                                           // Als de huidige tool een pen is en er wordt ingedrukt. Voeg huidige locatie toe aan lijst van (bezochte) punten
                                           if(huidigeTool == deTools[0] && vast)
                                                tijdelijk.punten.Add(mea.Location);
                                       };
            schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           {   // Als je klaar bent, voeg de eind locatie toe aan tijdelijk, en plaats dit object in de lijst Getekend.
                                               tijdelijk.eind = mea.Location;
                                               TijdelijkToevoegen();
                                               huidigeTool.MuisLos(schetscontrol, mea.Location);
                                           }
                                           vast = false;
                                       };
            schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                       {   // Als er een SchetsElememt is en de laatste in de lijst Getekend is met een tekst tool aangemaakt, dan mag de letter worden toegevoegd aan de tekst.
                                           if(schetscontrol.Schets.Getekend.Count > 0)
                                               if(schetscontrol.Schets.Getekend[schetscontrol.Schets.Getekend.Count()-1].soort.ToString() == "tekst" && kpea.KeyChar != ' ')
                                               {
                                                   huidigeTool.Letter(schetscontrol, kpea.KeyChar, schetscontrol.PenKleur);
                                                   schetscontrol.Schets.Getekend[schetscontrol.Schets.Getekend.Count()-1].tekst += kpea.KeyChar;
                                               }
                                       };
            this.Controls.Add(schetscontrol);

            menuStrip = new MenuStrip();
            menuStrip.Visible = false;
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakToolMenu(deTools);
            this.maakAktieMenu(deKleuren);
            this.maakToolButtons(deTools);
            this.maakAktieButtons(deKleuren);
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
        }
        private void maakFileMenu()
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("File");
            menu.MergeAction = MergeAction.MatchOnly;
            menu.DropDownItems.Add("Opslaan als", null, this.opslaanAls);
            menu.DropDownItems.Add("Sluiten", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }

        private void maakToolMenu(ICollection<ISchetsTool> tools)
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
            foreach (ISchetsTool tool in tools)
            {   ToolStripItem item = new ToolStripMenuItem();
                item.Tag = tool;
                item.Text = tool.ToString();
                item.Image = (Image)resourcemanager.GetObject(tool.ToString());
                item.Click += this.klikToolMenu;
                menu.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menu);
        }

        private void maakAktieMenu(String[] kleuren)
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Aktie");
            menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon );
            menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer );
            // Voeg kunnen Undo en Redo toe aan het Aktie menu
            menu.DropDownItems.Add("Undo", null, schetscontrol.Undo);
            menu.DropDownItems.Add("Redo", null, schetscontrol.Redo);
            ToolStripMenuItem submenu = new ToolStripMenuItem("Kies kleur");
            foreach (string k in kleuren)
                submenu.DropDownItems.Add(k, null, schetscontrol.VeranderKleurViaMenu);
            menu.DropDownItems.Add(submenu);
            menuStrip.Items.Add(menu);
        }

        private void maakToolButtons(ICollection<ISchetsTool> tools)
        {
            int t = 0;
            foreach (ISchetsTool tool in tools)
            {
                RadioButton b = new RadioButton();
                b.Appearance = Appearance.Button;
                b.Size = new Size(45, 62);
                b.Location = new Point(10, 10 + t * 62);
                b.Tag = tool;
                b.Text = tool.ToString();
                b.Image = (Image)resourcemanager.GetObject(tool.ToString());
                b.TextAlign = ContentAlignment.TopCenter;
                b.ImageAlign = ContentAlignment.BottomCenter;
                b.Click += this.klikToolButton;
                this.Controls.Add(b);
                if (t == 0) b.Select();
                t++;
            }
        }

        private void maakAktieButtons(String[] kleuren)
        {   
            paneel = new Panel();
            paneel.Size = new Size(600, 24);
            this.Controls.Add(paneel);
            
            Button b; Label l; ComboBox cbb;
            b = new Button(); 
            b.Text = "Clear";  
            b.Location = new Point( 0, 0); 
            b.Click += schetscontrol.Schoon; 
            paneel.Controls.Add(b);
            
            b = new Button(); 
            b.Text = "Rotate"; 
            b.Location = new Point( 80, 0); 
            b.Click += schetscontrol.Roteer; 
            paneel.Controls.Add(b);
            // Voeg knop toe voor Undo
            b = new Button();
            b.Text = "Undo";
            b.Location = new Point(160, 0);
            b.Click += schetscontrol.Undo;
            paneel.Controls.Add(b);
            // Voeg knop toe voor Reddo
            b = new Button();
            b.Text = "Redo";
            b.Location = new Point(240, 0);
            b.Click += schetscontrol.Redo;
            paneel.Controls.Add(b);
            // Posities aangepast voor nieuwe knoppen
            l = new Label();  
            l.Text = "Penkleur:"; 
            l.Location = new Point(320, 3); 
            l.AutoSize = true;               
            paneel.Controls.Add(l);
            
            cbb = new ComboBox(); cbb.Location = new Point(380, 0); 
            cbb.DropDownStyle = ComboBoxStyle.DropDownList; 
            cbb.SelectedValueChanged += schetscontrol.VeranderKleur;
            foreach (string k in kleuren)
                cbb.Items.Add(k);
            cbb.SelectedIndex = 0;
            paneel.Controls.Add(cbb);
        }
        void TijdelijkToevoegen()
        {   // Check of de gum niet de huidige tool is en voeg tijdelijke element toe aan Getekend.
            if (tijdelijk.soort.ToString() != "gum")
                schetscontrol.Schets.Getekend.Add(tijdelijk);
        }
        public void opslaanAls(object o, EventArgs ea)
        {   // Opent de windows-verkenner om het de lijst Getekend die omgezet is naar strings op te slaan.
            SaveFileDialog d = new SaveFileDialog();
            if (d.ShowDialog() == DialogResult.OK)
            {
                this.schrijf(d.FileName);
            }
        }
        public void lees(string naam)
        {
            StreamReader sr = new StreamReader(naam);
            try
            {   // Als er een tekst bestand is die geschikt is, dan gaan we dit bestand converteren van strings naar een Lijst van SchetsElementen
                this.schetscontrol.Schets.Getekend = File2List(sr);
                sr.Close();
                this.Text = naam;
                // Roep vervolgens de methode op om van een lijst SchetsElementen een graphics te maken.
                schetscontrol.Schets.LijstNaarGraphics(schetscontrol);
                schetscontrol.Invalidate();
            }
            catch (Exception e) 
            {   // Als het bestand ongeschikt is vanwege een fout, dan krijgt de gebruiker de volgende melding.
                string foutmelding1 = "Het open van het bestand is mislukt.";
                string foutmelding2 = "Mogelijk is het tekst bestand buiten dit programma gewijzigd.";
                string helefoutmelding = foutmelding1 + "\n" + foutmelding2 + "\n" +"\n" + "error:"+"\n" +e.Message;
                DialogResult result = MessageBox.Show(helefoutmelding); 
            }
        }
        private List<SchetsElement> File2List(StreamReader sr)
        {   // We maken een nieuwe Lijst van SchetsElementen aan waarbij we alle waarden voor de memeber variablen uit de string lijn parsen.
            List<SchetsElement> ls = new List<SchetsElement>();
            string lijn;
            // Zolang de expressie die regels uitleest nog resultaat geeft:
            while ((lijn = sr.ReadLine()) != null)
            {   // Maak array van alle strings die doorbroken zijn door spaties in de string lijn.
                string [] l = lijn.Split(' ');
                ISchetsTool schetstool = new TekstTool();
                // Parse de strings en maak de nieuwe objecten aan die gaan dienen als member variabelen.
                schetstool = LeesSoort(l[0]);
                Point begin = new Point(int.Parse(l[1]), int.Parse(l[2]));
                Point eind = new Point(int.Parse(l[3]), int.Parse(l[4]));
                Color kleur = Color.FromName(l[5]);
                string tekst = l[6];
                List<Point> punten = MaakLijstPunten(l);
                // Maak SchetsElement object aan.
                SchetsElement elem = new SchetsElement(schetstool,begin,kleur);
                elem.tekst = tekst;
                elem.eind = eind;
                elem.punten = punten; 
                // Voeg complete SchetsElement toe aan lijst
                ls.Add(elem);
            }
            return ls;
        }
        private static List<Point> MaakLijstPunten(string [] ls)
        {   // Maak voor elk paar opeenvolgende gehele getallen een Point object aan en sla deze op in een lijst (want lengtes verschillen) aan.)
            List<Point> punten = new List<Point>();
            for(int i = 7; i < ls.Length; i += 2)
            {
                Point punt = new Point(int.Parse(ls[i]), int.Parse(ls[i + 1]));
                punten.Add(punt);
            }
            return punten;
        }
        public ISchetsTool LeesSoort(string s)
        {   // Maak afhankelijk van de string die in het tekst bestand staat een tool object aan van de juiste soort.
            switch (s)
            {
                case "tekst": return new TekstTool(); break;
                case "kader": return new RechthoekTool(); break;
                case "vlak": return new VolRechthoekTool(); break;
                case "ovaal": return new EllipsTool(); break;
                case "schijf": return new VolEllipsTool(); break;
                case "lijn": return new LijnTool(); break;
                case "pen": return new PenTool(); break;
            }
            return null;
        }
        public void schrijf (string naam)
        {   // Converteer elk SchetsElement naar string en schrijf die regel in een tekstbestant weg.
            StreamWriter sw = new StreamWriter(naam);
            foreach (SchetsElement elem in schetscontrol.Schets.Getekend)
                sw.WriteLine(elem.ToString());
            sw.Close();
            this.Text = naam;
        }
    }
}