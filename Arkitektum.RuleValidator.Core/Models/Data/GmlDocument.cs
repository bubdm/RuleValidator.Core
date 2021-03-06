using Arkitektum.RuleValidator.Core.Extensions;
using Arkitektum.RuleValidator.Core.Helpers;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Arkitektum.RuleValidator.Core.Models
{
    public class GmlDocument : ValidationDataElement<XDocument>, IDisposable
    {
        private readonly List<XElement> _features;
        private readonly List<IndexedGeometry> _geometryIndex = new();
        private readonly object geoLock = new();

        public GmlDocument(XDocument data, string fileName, Enum dataType) : base(data, fileName, dataType)
        {
            _features = data.GetElements("//*:featureMember/* | //*:featureMembers/*").ToList();
        }

        public List<XElement> GetFeatures(params string[] featureNames)
        {
            if (!featureNames.Any())
                return _features;

            return _features
                .Where(element => featureNames.Any(name => name == element.Name.LocalName))
                .ToList();
        }

        public Geometry GetOrCreateGeometry(XElement geoElement, out string errorMessage)
        {
            lock (geoLock)
            {
                return GeometryHelper.GetOrCreateGeometry(_geometryIndex, geoElement, out errorMessage);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            foreach (var index in _geometryIndex)
            {
                if (index.Geometry != null)
                    index.Geometry.Dispose();
            }
        }
    }
}
