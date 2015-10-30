using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor
{
    public static class Tools
    {
        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {
            return new Rectangle(new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y))
                                , new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y))
                                );
        }
        public static Pen MaakPen(Brush b, int dikte)
        {
            Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }
    }

    public interface ISchetsTool
    {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p);
        void Letter(SchetsControl s, char c);
    }

    public abstract class StartpuntTool : ISchetsTool
    {
        protected Point startpunt;
        protected Brush kwast;

        public virtual void MuisVast(SchetsControl s, Point p)
        {
            startpunt = p;
        }
        public virtual void MuisLos(SchetsControl s, Point p)
        {
            kwast = new SolidBrush(s.PenKleur);
        }
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);
    }

    public class TekstTool : StartpuntTool
    {
        public override string ToString() { return "tekst"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void Letter(SchetsControl s, char c)
        {
            if (!char.IsLetterOrDigit(c)) return;

            var letter = new Letter(kwast, startpunt, c);
            s.Schets.Toevoegen(letter);
            s.Invalidate();

            startpunt.X += letter.box.Width;
        }
    }

    public abstract class TweepuntTool : StartpuntTool
    {
        public override void MuisVast(SchetsControl s, Point p)
        {
            base.MuisVast(s, p);
            kwast = Brushes.Gray;
        }
        public override void MuisDrag(SchetsControl s, Point p)
        {
            s.Refresh();
            this.Bezig(s.CreateGraphics(), this.startpunt, p);
        }
        public override void MuisLos(SchetsControl s, Point p)
        {
            base.MuisLos(s, p);
            this.Compleet(s.Schets, this.startpunt, p);
            s.Invalidate();
        }
        public override void Letter(SchetsControl s, char c)
        {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2);

        public abstract void Compleet(Schets s, Point p1, Point p2);
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawRectangle(Tools.MaakPen(kwast, 3), Tools.Punten2Rechthoek(p1, p2));
        }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            Rechthoek rechthoek = new Rechthoek(kwast, Tools.Punten2Rechthoek(p1, p2));
            s.Toevoegen(rechthoek);
        }
    }


    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            VolRechthoek volrechthoek = new VolRechthoek(kwast, Tools.Punten2Rechthoek(p1, p2));
            s.Toevoegen(volrechthoek);
        }
    }

    public class CirkelTool : TweepuntTool
    {
        public override string ToString() { return "cirkel"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawEllipse(Tools.MaakPen(kwast, 3), Tools.Punten2Rechthoek(p1, p2));
        }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            Cirkel cirkel = new Cirkel(kwast, Tools.Punten2Rechthoek(p1, p2));
            s.Toevoegen(cirkel);
        }
    }

    public class VolCirkelTool : CirkelTool
    {
        public override string ToString() { return "rondje"; }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            VolCirkel volcirkel = new VolCirkel(kwast, Tools.Punten2Rechthoek(p1, p2));
            s.Toevoegen(volcirkel);
        }
    }

    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawLine(Tools.MaakPen(this.kwast, 3), p1, p2);
        }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            Lijn lijn = new Lijn(kwast, p1, p2);
            s.Toevoegen(lijn);
        }
    }

    public class PenTool : TweepuntTool
    {
        public override string ToString() { return "pen"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {

        }

        public override void Compleet(Schets s, Point p1, Point p2)
        {

        }
    }

    public class GumTool : StartpuntTool
    {
        public override void MuisLos(SchetsControl s, Point p)
        {
            base.MuisLos(s, p);

            var buffer = s.Schets.buffer;
            for (var i = buffer.Count - 1; i >= 0; i--)
            {
                var geometry = buffer[i];
                if (geometry.Bevat(p))
                {
                    buffer.RemoveAt(i);
                    s.Schets.Herteken();
                    s.Invalidate();
                    break;
                }
            }
        }



        public override void Letter(SchetsControl s, char c)
        {

        }

        public override void MuisDrag(SchetsControl s, Point p)
        {

        }

        public override string ToString() { return "gum"; }
    }
}
