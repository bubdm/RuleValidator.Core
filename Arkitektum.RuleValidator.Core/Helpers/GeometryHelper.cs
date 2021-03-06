using Arkitektum.RuleValidator.Core.Exceptions;
using Arkitektum.RuleValidator.Core.Extensions;
using Arkitektum.RuleValidator.Core.Models;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SpatialReference = OSGeo.OSR.SpatialReference;

namespace Arkitektum.RuleValidator.Core.Helpers
{
    public class GeometryHelper
    {
        private static readonly Regex _exceptionRegex = new(@"^\w+Exception: ", RegexOptions.Compiled);

        public static Geometry CreatePoint(double x, double y, double z = 0, int? epsg = null)
        {
            var point = new Geometry(wkbGeometryType.wkbPoint);

            if (epsg.HasValue)
            {
                var spatialReference = new SpatialReference(null);
                spatialReference.ImportFromEPSG(epsg.Value);

                point.AssignSpatialReference(spatialReference);
            }

            point.AddPoint(x, y, z);

            return point;
        }

        public static Geometry CreateLine(double[][] points, int? epsg = null)
        {
            var line = new Geometry(wkbGeometryType.wkbLineString);

            if (epsg.HasValue)
            {
                var spatialReference = new SpatialReference(null);
                spatialReference.ImportFromEPSG(epsg.Value);

                line.AssignSpatialReference(spatialReference);
            }

            for (int i = 0; i < points.Length; i++)
                line.AddPoint(points[i][0], points[i][1], 0);

            return line;
        }

        public static Geometry GetFootprintOfSolid(XElement geoElement)
        {
            if (geoElement == null)
                return null;

            var posLists = GetPosLists(geoElement);

            if (!posLists.Any())
                return null;

            using var multiPolygon = new Geometry(wkbGeometryType.wkbMultiPolygon);

            foreach (var posList in posLists)
            {
                var coords = PosListToCoordinates(posList, 3);

                using var polygon = new Geometry(wkbGeometryType.wkbPolygon);
                using var ring = new Geometry(wkbGeometryType.wkbLinearRing);

                ring.AddPoint(coords[0][0], coords[0][1], 0);
                ring.AddPoint(coords[1][0], coords[1][1], 0);
                ring.AddPoint(coords[2][0], coords[2][1], 0);
                ring.AddPoint(coords[3][0], coords[3][1], 0);

                polygon.AddGeometry(ring);
                multiPolygon.AddGeometry(polygon);
            }

            return multiPolygon.UnionCascaded();
        }

        public static List<string> GetPosLists(XElement geoElement)
        {
            return geoElement.GetValues<string>("//*:posList | //*:pos").ToList();
        }

        public static List<double[]> GetCoordinates(XElement geoElement, int dimensions = 2)
        {
            return GetPosLists(geoElement)
                .SelectMany(posList => PosListToCoordinates(posList, dimensions))
                .ToList();
        }

        public static List<double[]> PosListsToCoordinates(List<string> posLists, int dimensions = 2)
        {
            return posLists
                .SelectMany(posList => PosListToCoordinates(posList, dimensions))
                .ToList();
        }

        public static List<double[]> PosListToCoordinates(string posList, int dimensions = 2)
        {
            if (posList == null)
                throw new Exception("Element gml:posList eksisterer ikke");

            var posStrings = posList.Split(" ");

            if (posStrings.Length == 0 || posStrings.Length % dimensions != 0)
                throw new Exception($"Element gml:posList har ugyldig antall koordinater: '{posStrings.Length}'");

            static double posStringToDouble(string posString)
            {
                try
                {
                    return double.Parse(posString, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    throw new Exception($"Element gml:posList har ugyldig koordinat: '{posString}'");
                }
            }

            var positions = new List<double[]>();

            for (var i = dimensions - 1; i < posStrings.Length; i += dimensions)
            {
                var x = dimensions == 2 ? posStringToDouble(posStrings[i - 1]) : posStringToDouble(posStrings[i - 2]);
                var y = dimensions == 2 ? posStringToDouble(posStrings[i]) : posStringToDouble(posStrings[i - 1]);
                var z = dimensions == 2 ? 0 : posStringToDouble(posStrings[i]);

                positions.Add(new[] { x, y, z });
            }

            return positions;
        }

        public static DisposableList<Geometry> PosListsToPoints(List<string> posLists, int dimensions = 2)
        {
            var coordinates = PosListsToCoordinates(posLists, dimensions);
            var points = new DisposableList<Geometry>();

            foreach (var coordinate in coordinates)
            {
                points.Add(CreatePoint(coordinate[0], coordinate[1], coordinate[2]));
            }

            return points;
        }

        public static bool Within(XElement geoElement, Geometry geometry, double threshold, out List<double[][]> outsidePoints)
        {
            outsidePoints = new List<double[][]>();
            
            var posLists = GetPosLists(geoElement);
            using var points = PosListsToPoints(posLists);

            foreach (var point in points)
                if (point.Distance(geometry) > threshold)
                    outsidePoints.Add(point.GetPoints());

            return !outsidePoints.Any();
        }

        public static List<List<Geometry>> GetLineSegmentsOfPolygon(Geometry polygon)
        {
            var geometryCount = polygon.GetGeometryCount();
            var lineSegmentsList = new List<List<Geometry>>();

            for (var i = 0; i < geometryCount; i++)
            {
                using var ring = polygon.GetGeometryRef(i);
                var points = ring.GetPoints();

                if (!points.Any())
                    continue;

                var lineSegments = new List<Geometry>();

                for (var j = 1; j < points.Length; j++)
                {
                    var x1 = points[j - 1][0];
                    var y1 = points[j - 1][1];
                    var x2 = points[j][0];
                    var y2 = points[j][1];

                    var lineString = new Geometry(wkbGeometryType.wkbLineString);
                    lineString.AddPoint(x1, y1, 0);
                    lineString.AddPoint(x2, y2, 0);

                    lineSegments.Add(lineString);
                }

                lineSegmentsList.Add(lineSegments);
            }

            return lineSegmentsList;
        }

        public static bool PointsAreClockWise(List<double[]> points)
        {
            double sum = 0;

            for (int i = 1; i < points.Count; i++)
                sum += (points[i][0] - points[i - 1][0]) * (points[i][1] + points[i - 1][1]);

            return sum >= 0;
        }

        public static bool LineSegmentsAreConnected(Geometry lineA, Geometry lineB)
        {
            var pointsA = lineA.GetPoints();
            var pointsB = lineB.GetPoints();

            return pointsA[1][0] == pointsB[0][0] && pointsA[1][1] == pointsB[0][1] ||
                pointsA[0][0] == pointsB[1][0] && pointsA[0][1] == pointsB[1][1];
        }

        public static Circle PointsToCircle(double[] p1, double[] p2, double[] p3)
        {
            var ax = (p1[0] + p2[0]) / 2;
            var ay = (p1[1] + p2[1]) / 2;
            var ux = p1[1] - p2[1];
            var uy = p2[0] - p1[0];
            var bx = (p2[0] + p3[0]) / 2;
            var by = (p2[1] + p3[1]) / 2;
            var vx = p2[1] - p3[1];
            var vy = p3[0] - p2[0];
            var dx = ax - bx;
            var dy = ay - by;
            var vu = vx * uy - vy * ux;

            if (vu == 0)
                return null;

            var g = (dx * uy - dy * ux) / vu;
            var cx = bx + g * vx;
            var cy = by + g * vy;
            var radius = Math.Sqrt(Math.Pow(p2[0] - cx, 2) + Math.Pow(p2[1] - cy, 2));

            return new Circle
            {
                Center = new double[2] { cx, cy },
                Radius = radius
            };
        }

        public static Geometry GeometryFromGML(XElement geoElement)
        {
            try
            {
                var geometry = Geometry.CreateFromGML(geoElement.ToString());
                var _ = geometry.IsValid();

                return geometry;
            }
            catch (Exception exception)
            {
                throw new GeometryFromGMLException($"Ugyldig geometri: {geoElement.GetName()} '{geoElement.GetAttribute("gml:id")}'. Detaljert feil: {_exceptionRegex.Replace(exception.Message, "")}");
            }
        }

        public static Geometry GetOrCreateGeometry(List<IndexedGeometry> geometryIndex, XElement geoElement, out string errorMessage)
        {
            var gmlId = geoElement?.GetAttribute("gml:id");

            if (gmlId == null)
            {
                errorMessage = "Ugyldig GML-element";
                return null;
            }

            var indexed = geometryIndex.SingleOrDefault(index => index.GmlId == gmlId);

            if (indexed != null)
            {
                errorMessage = indexed.ErrorMessage;
                return indexed.Geometry?.Clone();
            }

            Geometry geometry = null;
            errorMessage = null;

            try
            {
                geometry = GeometryFromGML(geoElement);
            }
            catch (GeometryFromGMLException exception)
            {
                errorMessage = exception.Message;
            }

            geometryIndex.Add(new IndexedGeometry(gmlId, geometry, errorMessage));

            return geometry?.Clone();
        }
    }
}
