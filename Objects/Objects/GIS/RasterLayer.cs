using System.Collections.Generic;
using Objects.BuiltElements;
using Objects.Structural.Geometry;
using Speckle.Core.Kits;
using Speckle.Core.Models;

namespace Objects.GIS;

public class RasterLayer : Collection
{
  public CRS? crs { get; set; }
  public string? units { get; set; }
  public CRS? rasterCrs { get; set; }
  public string? geomType { get; set; }
  public Dictionary<string, object>? renderer { get; set; }

  public RasterLayer()
  {
    collectionType = "RasterLayer";
  }
}
