using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SchetsEditor
{
    public class Schets
    {
        private Bitmap bitmap;
        public List<Geometry> buffer;


        public Schets()
        {
            bitmap = new Bitmap(1, 1);
            buffer = new List<Geometry>(); 
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
            buffer.Clear();
            Herteken();
        }
        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }

        public void OpslaanOpFile(string bestandsnaam, int formaat = 1)
        {
            ImageFormat opslagformaat;
            switch (formaat)
            {
                case 1:
                    opslagformaat = ImageFormat.Jpeg;
                    bitmap.Save(bestandsnaam, opslagformaat);
                    break;
                case 2:
                    opslagformaat = ImageFormat.Png;
                    bitmap.Save(bestandsnaam, opslagformaat);
                    break;
                case 3:
                    opslagformaat = ImageFormat.Bmp;
                    bitmap.Save(bestandsnaam, opslagformaat);
                    break;
                case 4:
                    StreamWriter sw = new StreamWriter(bestandsnaam);
                    foreach (var geometry in buffer)
                    {
                        string[] data = geometry.Opslaan();
                        string regel = string.Join("|", data);
                        sw.WriteLine(regel);
                    }
                    sw.Close();
                    break;
                default:
                    opslagformaat = ImageFormat.Jpeg;
                    bitmap.Save(bestandsnaam, opslagformaat);
                    break;
            }
        }

        public void LadenVanFile(string bestandsnaam)
        {
            if (Path.GetExtension(bestandsnaam) == ".schets")
            {
                buffer.Clear();

                StreamReader sr = new StreamReader(bestandsnaam);
                string regel;
                while ((regel = sr.ReadLine()) != null)
                {
                    string[] data = regel.Split('|');
                    switch (data[0])
                    {
                        case "rechthoek":
                            Rechthoek rechthoek = new Rechthoek(data);
                            Toevoegen(rechthoek);
                            break;
                    }
                }
                sr.Close();

                Herteken();
            } else
            {
                bitmap = new Bitmap(bestandsnaam);
            }            
        }

        public void Herteken()
        {
            BitmapGraphics.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);

            var g = BitmapGraphics;
            foreach (var geometry in buffer)
            {
                geometry.Teken(g);
            }

        }

        public void Toevoegen(Geometry geometry)
        {
            buffer.Add(geometry);
            geometry.Teken(BitmapGraphics);
        }

    }
}
