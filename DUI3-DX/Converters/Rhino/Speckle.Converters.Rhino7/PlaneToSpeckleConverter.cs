﻿using Rhino;
using Rhino.Geometry;
using Speckle.Converters.Common;
using Speckle.Converters.Common.Objects;
using Speckle.Core.Models;
using Speckle.Objects.Geometry;
using Plane = Rhino.Geometry.Plane;
using Point = Speckle.Objects.Geometry.Point;

namespace Speckle.Converters.Rhino7;

// POC: not sure I like the place of the default rank
[NameAndRankValue(nameof(Rhino.Geometry.Plane), NameAndRankValueAttribute.SPECKLE_DEFAULT_RANK)]
public class PlaneToSpeckleConverter : IHostObjectToSpeckleConversion, IRawConversion<Plane, Objects.Geometry.Plane>
{
  private readonly IRawConversion<Vector3d, Vector> _vectorConverter;
  private readonly IRawConversion<Point3d, Point> _pointConverter;

  public PlaneToSpeckleConverter(
    IHostToSpeckleUnitConverter<UnitSystem> unitConverter,
    IRawConversion<Vector3d, Vector> vectorConverter,
    IRawConversion<Point3d, Point> pointConverter
  )
  {
    _vectorConverter = vectorConverter;
    _pointConverter = pointConverter;
  }

  public Base Convert(object target) => RawConvert((Plane)target);

  public Objects.Geometry.Plane RawConvert(Plane target) =>
    new(
      _pointConverter.RawConvert(target.Origin),
      _vectorConverter.RawConvert(target.ZAxis),
      _vectorConverter.RawConvert(target.XAxis),
      _vectorConverter.RawConvert(target.YAxis)
    );
}
