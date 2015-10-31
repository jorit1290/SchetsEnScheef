using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SchetsEditor
{
    public class Schets
    {
        public Bitmap bitmap {get; protected set; }
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
                Bitmap nieuw = new Bitmap(Math.Max(sz.Width, bitmap.Size.Width)
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
                    var stream = new FileStream(bestandsnaam, FileMode.Create);
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, buffer);
                    stream.Close();
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
                var stream = new FileStream(bestandsnaam, FileMode.Open);
                var formatter = new BinaryFormatter();
                buffer = (List<Geometry>)formatter.Deserialize(stream);

                Herteken();
            }
            else
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
