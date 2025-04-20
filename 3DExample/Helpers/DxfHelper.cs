using HelixToolkit.Wpf;
using netDxf;
using netDxf.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DExample.Helpers
{
    public class DxfHelper
    {
        public static string DxfPath = AppDomain.CurrentDomain.BaseDirectory + "DxfFiles\\";

        public static void ReadDxfFile(HelixViewport3D helixViewport3D, string path, double extrudeVal =30)
        {
            DxfDocument dxfDocument = DxfDocument.Load(path);
            
            if (dxfDocument == null)
            {
                Console.WriteLine("Failed to load DXF file.");
                return;
            }
         helixViewport3D.Children.Clear();
            var block = dxfDocument.Blocks;
            List<Polyline2D> polys = new List<Polyline2D>();
            foreach (var blo in block)
            {
                foreach (var entity in blo.Entities)
                {
                    if (entity is netDxf.Entities.Polyline2D lwpoly)
                    {

                        var points2D = lwpoly.Vertexes
                            .Select(v => new System.Windows.Point(v.Position.X, v.Position.Y))
                            .ToList();

                        if (!lwpoly.IsClosed)
                            points2D.Add(points2D[0]);

                        // Extrude et (örnek)
                        var builder = new MeshBuilder();
                        var p0 = new Point3D(0, 0, 0);
                        var p1 = new Point3D(0, 0, 30);
                        var xaxis = new Vector3D(1, 0, 0);
                        builder.AddExtrudedGeometry(points2D, xaxis, p0, p1);

                        var mesh = builder.ToMesh();
                        var model = new GeometryModel3D(mesh, Materials.Red);
                        var visual = new ModelVisual3D { Content = model };

                        // viewport’a ekle
                        //helixViewport3D.Children.Add(visual);
                        DrawExtrudedPolylineWithVisual3D(helixViewport3D, lwpoly,extrudeVal);
                        polys.Add(lwpoly);
                    }
                    else if (entity is Insert insert)
                    {
                        var blocks = insert.Block;
                        var position = insert.Position;

                        foreach (var blockEntity in blocks.Entities)
                        {
                            if (blockEntity is Polyline2D poly)
                            {
                                var points2D = poly.Vertexes.Select(v => new System.Windows.Point(v.Position.X, v.Position.Y)).ToList();
                                if (!poly.IsClosed)
                                    points2D.Add(points2D[0]);

                                var builder = new MeshBuilder();
                                var p0 = new Point3D(position.X, position.Y, position.Z); // Yerleştirildiği noktaya göre extrude
                                var p1 = new Point3D(position.X, position.Y, position.Z + 30);
                                var xaxis = new Vector3D(1, 0, 0);

                                builder.AddExtrudedGeometry(points2D, xaxis, p0, p1);

                                var mesh = builder.ToMesh();
                                var model = new GeometryModel3D(mesh, Materials.Orange);
                                var visual = new ModelVisual3D { Content = model };
                                //helixViewport3D.Children.Add(visual);

                            }
                            else
                            {
                                Console.WriteLine(blockEntity.Type);
                            }
                        }
                    }
                    else if (entity is Arc arc)
                    {
                        //DrawCleanExtrudedArc(helixViewport3D, arc);

                    }
                    
                    {
                        Console.WriteLine(entity.Type);

                    }
                }
            }


        }
        /// <summary>
        /// dogru calisan kod asagida
        /// </summary>
        /// <param name="viewport"></param>
        /// <param name="polyline"></param>
        /// <param name="extrudeHeight"></param>
        public static void DrawExtrudedPolylineWithVisual3D(HelixViewport3D viewport, Polyline2D polyline, double extrudeHeight = 30)
        {
            if (polyline == null || polyline.Vertexes.Count < 2)
                return;

            // 1. Polyline'dan 2D noktaları al
            var points2D = polyline.Vertexes
                .Select(v => new System.Windows.Point(v.Position.X, v.Position.Y))
                .ToList();

            // bunun altindaki ifstatementi kaldırabilirsin
            //if (!polyline.IsClosed)
            //points2D.Add(points2D[0]);

            // 2. PointCollection'a dönüştür
            var section = new PointCollection(points2D);

            // 3. ExtrudedVisual3D oluştur
            var render = new ExtrudedVisual3D
            {
                Section = section,
                SectionXAxis = new Vector3D(1, 0, 0),
                Fill = Brushes.GhostWhite,
                IsSectionClosed = false,
                IsPathClosed = false
            };

            // 4. Extrusion yönü (Z ekseninde extrude)
            render.Path.Add(new Point3D(0, 0, 0));
            render.Path.Add(new Point3D(0, 0, extrudeHeight));

            // 5. Sahneye ekle
            var wallModel = new ModelVisual3D();
            wallModel.Children.Add(render);
            viewport.Children.Add(wallModel);
        }
        public static void AddLightingAndEnvironment(HelixViewport3D viewport)
        {
            if (viewport == null) return;

            // Daha önceki ışıkları kaldır
            var existingLights = viewport.Children.OfType<ModelVisual3D>()
                .Where(m => m.Content is Light)
                .ToList();

            foreach (var light in existingLights)
                viewport.Children.Remove(light);

            // Yeni ışık grubu
            var lights = new Model3DGroup();

            // Key light (ana ışık - önden)
            lights.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -1, -2)));

            // Fill light (yardımcı - yandan)
            lights.Children.Add(new DirectionalLight(Color.FromRgb(180, 180, 180), new Vector3D(1, -0.5, -1)));

            // Back light (arka - kenar vurgusu)
            lights.Children.Add(new DirectionalLight(Color.FromRgb(100, 100, 100), new Vector3D(0, 1, -0.3)));

            // Ambient (yumuşak ortam ışığı)
            lights.Children.Add(new AmbientLight(Color.FromRgb(40, 40, 40)));

            // Görsel olarak ekle
            var lightVisual = new ModelVisual3D { Content = lights };
            viewport.Children.Add(lightVisual);

            // Arka plan rengi (HDR etkisi için açık renkli)
            viewport.Background = new SolidColorBrush(Color.FromRgb(220, 230, 240)); // soft blue/gray
        }

        public static void DrawExtrudedFromPathGeometry(HelixViewport3D viewport, Polyline2D polyline, double extrudeHeight = 30, Brush fill = null)
        {
            if (polyline == null || polyline.Vertexes.Count < 2)
                return;

            // 1. Polyline2D → PathGeometry
            var geometry = Polyline2DToPathGeometry(polyline);

            // 2. PathGeometry → düzleştirilmiş nokta listesi
            var points2D = FlattenPathGeometry(geometry);

            if (points2D.Count < 3)
                return;

            // 3. ExtrudedVisual3D ile extrusion
            var render = new ExtrudedVisual3D
            {
                Section = new PointCollection(points2D),
                SectionXAxis = new Vector3D(1, 0, 0),
                Fill = fill ?? Brushes.SlateGray,
                IsSectionClosed = true,
                IsPathClosed = false
            };

            render.Path.Add(new Point3D(0, 0, 0));
            render.Path.Add(new Point3D(0, 0, extrudeHeight));

            viewport.Children.Add(render);
        }

        public static PathGeometry Polyline2DToPathGeometry(Polyline2D poly)
        {
            var fig = new PathFigure
            {
                IsClosed = poly.IsClosed,
                StartPoint = new System.Windows.Point(poly.Vertexes[0].Position.X, poly.Vertexes[0].Position.Y)
            };

            for (int i = 1; i < poly.Vertexes.Count; i++)
            {
                var pt = poly.Vertexes[i].Position;
                fig.Segments.Add(new System.Windows.Media.LineSegment(new System.Windows.Point(pt.X, pt.Y), true));
            }

            var geo = new PathGeometry();
            geo.Figures.Add(fig);
            return geo;
        }
        public static List<System.Windows.Point> FlattenPathGeometry(PathGeometry geometry, double tolerance = 0.01)
        {
            var flattened = geometry.GetFlattenedPathGeometry(tolerance, ToleranceType.Absolute);
            var points = new List<System.Windows.Point>();

            foreach (var fig in flattened.Figures)
            {
                points.Add(fig.StartPoint);
                foreach (var seg in fig.Segments.OfType<System.Windows.Media.LineSegment>())
                {
                    points.Add(seg.Point);
                }
            }

            return points;
        }

        public static void DrawCappedExtrudedPolyline(HelixViewport3D viewport, Polyline2D polyline, double extrudeHeight = 30)
        {
            if (polyline == null || polyline.Vertexes.Count < 3)
                return;

            // 1. 2D noktaları al
            var points2D = polyline.Vertexes
                .Select(v => new System.Windows.Point(v.Position.X, v.Position.Y))
                .ToList();

            // Kapat (ilk-son aynı değilse)
            if (!points2D.First().Equals(points2D.Last()))
                points2D.Add(points2D.First());

            // Gerekirse yönü düzelt (saat yönü değilse)
            if (GetSignedArea(points2D) < 0)
                points2D.Reverse();

            // 2. Yan yüzey (ExtrudedVisual3D)
            var render = new ExtrudedVisual3D
            {
                Section = new PointCollection(points2D),
                SectionXAxis = new Vector3D(1, 0, 0),
                Fill = Brushes.LightGray,
                IsSectionClosed = true,
                IsPathClosed = false
            };

            render.Path.Add(new Point3D(0, 0, 0));
            render.Path.Add(new Point3D(0, 0, extrudeHeight));
            viewport.Children.Add(render);

            // 3. Üst-alt kapaklar için MeshBuilder
            var builder = new MeshBuilder();

            AddFlatCapSmart(builder, points2D, 0);
            AddFlatCapSmart(builder, points2D, extrudeHeight);

            var mesh = builder.ToMesh();
            var material = new DiffuseMaterial(new SolidColorBrush(Colors.LightGray));
            var model = new GeometryModel3D(mesh, material);
            viewport.Children.Add(new ModelVisual3D { Content = model });
        }

        private static void AddFlatCapSmart(MeshBuilder builder, List<System.Windows.Point> contour2D, double z)
        {
            if (contour2D == null || contour2D.Count < 3)
                return;

            var fanPoints = contour2D
                .Select(p => new Point3D(p.X, p.Y, z))
                .ToList();

            var center = new Point3D(
                fanPoints.Average(p => p.X),
                fanPoints.Average(p => p.Y),
                z);

            for (int i = 0; i < fanPoints.Count - 1; i++)
            {
                builder.AddTriangle(fanPoints[i], fanPoints[i + 1], center);
            }
        }


        private static double GetSignedArea(List<System.Windows.Point> pts)
        {
            double area = 0;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                area += (pts[i].X * pts[i + 1].Y) - (pts[i + 1].X * pts[i].Y);
            }
            return area / 2.0;
        }


    }
}
