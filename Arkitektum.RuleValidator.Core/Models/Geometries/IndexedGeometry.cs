using OSGeo.OGR;
using System;

namespace Arkitektum.RuleValidator.Core.Models
{
    public class IndexedGeometry : IDisposable
    {
        public string GmlId { get; set; }
        public Geometry Geometry { get; set; }
        public string ErrorMessage { get; set; }

        public IndexedGeometry(string gmlId, Geometry geometry, string errorMessage)
        {
            GmlId = gmlId;
            Geometry = geometry;
            ErrorMessage = errorMessage;
        }

        public void Dispose()
        {
            if (Geometry != null)
                Geometry.Dispose();
        }
    }
}
