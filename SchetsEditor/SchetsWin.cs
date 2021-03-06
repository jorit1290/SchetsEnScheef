﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.IO;

namespace SchetsEditor
{
    public class SchetsWin : Form
    {
        MenuStrip menuStrip;
        SchetsControl schetscontrol;
        ISchetsTool huidigeTool;
        Panel paneel;
        bool vast, veranderd;
        ResourceManager resourcemanager
            = new ResourceManager("SchetsEditor.Properties.Resources"
                                 , Assembly.GetExecutingAssembly()
                                 );

        private void veranderAfmeting(object o, EventArgs ea)
        {
            schetscontrol.Size = new Size(this.ClientSize.Width - 70
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

        private void NaarFile(object obj, EventArgs ea)
        {
            SaveFileDialog opslagKeuze = new SaveFileDialog();

            opslagKeuze.Filter = "JPG-bestand|*.jpg|PNG-bestand|*.png|BMP-bestand|*.bmp|Schets-bestand|*.schets";
            opslagKeuze.Title = "Schets opslaan";

            DialogResult resultaat = opslagKeuze.ShowDialog();
            if (resultaat == DialogResult.OK)
            {
                string bestandsnaam = opslagKeuze.FileName;
                this.schetscontrol.Schets.OpslaanOpFile(bestandsnaam, opslagKeuze.FilterIndex);
                veranderd = false;
            }

        }

        private void VanFile(object obj, EventArgs ea)
        {
            OpenFileDialog openKiezer = new OpenFileDialog();

            openKiezer.Filter = "Afbeeldingen (*.JPG;*.PNG;*.BMP)|*.JPG;*.PNG;*.BMP|" +
                                "Alle bestanden (*.*)|*.*";
            openKiezer.Title = "Schets openen";

            DialogResult resultaat = openKiezer.ShowDialog();
            if (resultaat == DialogResult.OK)
            {
                string bestandsnaam = openKiezer.FileName;
                schetscontrol.LadenVanFile(bestandsnaam);
            }
        }

        public SchetsWin()
        {
            ISchetsTool[] deTools = { new PenTool()
                                    , new LijnTool()
                                    , new RechthoekTool()
                                    , new CirkelTool()
                                    , new VolCirkelTool()
                                    , new VolRechthoekTool()
                                    , new TekstTool()
                                    , new GumTool()
                                    };
            String[] deKleuren = { "Black", "Red", "Green", "Blue"
                                 , "Yellow", "Magenta", "Cyan"
                                 };


            this.ClientSize = new Size(700, 549);
            huidigeTool = deTools[0];

            schetscontrol = new SchetsControl();
            schetscontrol.Location = new Point(64, 10);
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       { veranderd = true; };
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       {
                                           vast = true;
                                           huidigeTool.MuisVast(schetscontrol, mea.Location);
                                       };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                       {
                                           if (vast)
                                               huidigeTool.MuisDrag(schetscontrol, mea.Location);
                                       };
            schetscontrol.MouseUp += (object o, MouseEventArgs mea) =>
                                     {
                                         if (vast)
                                             huidigeTool.MuisLos(schetscontrol, mea.Location);
                                         vast = false;
                                     };
            schetscontrol.KeyPress += (object o, KeyPressEventArgs kpea) =>
                                      {
                                          huidigeTool.Letter(schetscontrol, kpea.KeyChar);
                                      };
            this.Controls.Add(schetscontrol);

            menuStrip = new MenuStrip();
            menuStrip.Visible = false;
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakToolMenu(deTools);
            this.maakActieMenu(deKleuren);
            this.maakToolButtons(deTools);
            this.maakActieButtons(deKleuren);
            this.Resize += this.veranderAfmeting;
            this.FormClosing += this.Afsluitvrager;
            this.veranderAfmeting(null, null);
        }

        private void maakFileMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("File");
            menu.MergeAction = MergeAction.MatchOnly;
            menu.DropDownItems.Add("Sluiten", null, this.afsluiten);
            menu.DropDownItems.Add("Opslaan", null, this.NaarFile);
            menu.DropDownItems.Add("Openen", null, this.VanFile);
            menuStrip.Items.Add(menu);
        }

        private void maakToolMenu(ICollection<ISchetsTool> tools)
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
            foreach (ISchetsTool tool in tools)
            {
                ToolStripItem item = new ToolStripMenuItem();
                item.Tag = tool;
                item.Text = tool.ToString();
                item.Image = (Image)resourcemanager.GetObject(tool.ToString());
                item.Click += this.klikToolMenu;
                menu.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menu);
        }

        private void maakActieMenu(String[] kleuren)
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("Actie");
            menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon);
            menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer);
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

        private void maakActieButtons(String[] kleuren)
        {
            paneel = new Panel();
            paneel.Size = new Size(600, 24);
            this.Controls.Add(paneel);

            Button b; Label l; ComboBox cbb;
            b = new Button();
            b.Text = "Clear";
            b.Location = new Point(0, 0);
            b.Click += schetscontrol.Schoon;
            paneel.Controls.Add(b);

            b = new Button();
            b.Text = "Rotate";
            b.Location = new Point(80, 0);
            b.Click += schetscontrol.Roteer;
            paneel.Controls.Add(b);

            l = new Label();
            l.Text = "Penkleur:";
            l.Location = new Point(180, 3);
            l.AutoSize = true;
            paneel.Controls.Add(l);

            cbb = new ComboBox(); cbb.Location = new Point(240, 0);
            cbb.DropDownStyle = ComboBoxStyle.DropDownList;
            cbb.SelectedValueChanged += schetscontrol.VeranderKleur;
            foreach (string k in kleuren)
                cbb.Items.Add(k);
            cbb.SelectedIndex = 0;
            paneel.Controls.Add(cbb);
        }

        private void Afsluitvrager(object o, FormClosingEventArgs fcea)
        {
            if (!veranderd) return;
            DialogResult zeker = MessageBox.Show("Er zijn onopgeslagen wijzigingen, toch afsluiten?",
            "Onopgeslagen wijzigingen", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (zeker != DialogResult.Yes)
            {
                fcea.Cancel = true;
            }
        }
    }
}
