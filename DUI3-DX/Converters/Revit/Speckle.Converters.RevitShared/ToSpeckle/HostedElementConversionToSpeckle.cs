using System.Collections.Generic;
using Autodesk.Revit.DB;
using Speckle.Converters.Common;
using Speckle.Converters.RevitShared.Helpers;
using Speckle.Core.Models;

namespace Speckle.Converters.RevitShared.ToSpeckle;

public class HostedElementConversionToSpeckle
{
  private readonly ToSpeckleConvertedObjectsCache _convertedObjectsCache;
  private readonly ISpeckleConverterToSpeckle _converter;

  public HostedElementConversionToSpeckle(
    ToSpeckleConvertedObjectsCache convertedObjectsCache,
    ISpeckleConverterToSpeckle converter
  )
  {
    _convertedObjectsCache = convertedObjectsCache;
    _converter = converter;
  }

  public List<Base> GetHostedElements(Element host)
  {
    var convertedHostedElements = new List<Base>();

    var hostedElementIds = GetHostedElementIds(host);

    foreach (var elemId in hostedElementIds)
    {
      var element = host.Document.GetElement(elemId);
      if (_convertedObjectsCache.ContainsBaseConvertedFromId(element.UniqueId))
      {
        continue;
      }

      convertedHostedElements.Add(_converter.Convert(element));
    }

    return convertedHostedElements;
  }

  public IList<ElementId> GetHostedElementIds(Element host)
  {
    IList<ElementId> ids = null;
    if (host is HostObject hostObject)
    {
      ids = hostObject.FindInserts(true, false, false, false);
    }
    else
    {
      var typeFilter = new ElementIsElementTypeFilter(true);
      var categoryFilter = new ElementMulticategoryFilter(
        new List<BuiltInCategory>()
        {
          BuiltInCategory.OST_CLines,
          BuiltInCategory.OST_SketchLines,
          BuiltInCategory.OST_WeakDims
        },
        true
      );
      ids = host.GetDependentElements(new LogicalAndFilter(typeFilter, categoryFilter));
    }

    // dont include host elementId
    ids.Remove(host.Id);

    return ids;
  }
}
