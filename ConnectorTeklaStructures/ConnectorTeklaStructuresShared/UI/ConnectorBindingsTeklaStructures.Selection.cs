﻿using System;
using System.Collections.Generic;
using DesktopUI2;
using DesktopUI2.Models;
using DesktopUI2.Models.Filters;
using Speckle.ConnectorTeklaStructures.Util;
using System.Linq;
namespace Speckle.ConnectorTeklaStructures.UI
{
  public partial class ConnectorBindingsTeklaStructures : ConnectorBindings

  {
    public override List<string> GetSelectedObjects()
    {
      var names = new List<string>();
      var util = new ConnectorTeklaStructuresUtils();
      var typeNameTupleList = ConnectorTeklaStructuresUtils.SelectedObjects(Model);
      if (typeNameTupleList == null) return new List<string>() { };
      foreach (var item in typeNameTupleList)
      {
        (string typeName, string name) = item;
        if (ConnectorTeklaStructuresUtils.IsTypeTeklaStructuresAPIUsable(typeName))
        {
          names.Add(string.Concat(typeName, ": ", name));
        }
      }
      if (names.Count == 0)
      {
        return new List<string>() { };
      }
      return names;
    }

    public override List<ISelectionFilter> GetSelectionFilters()
    {
      var objectTypes = new List<string>();
      //var objectIds = new List<string>();
      string[] groupNames = null;
      var groups = new List<string>();
      int numNames = 0;
      if (Model != null)
      {
        ConnectorTeklaStructuresUtils.GetObjectIDsTypesAndNames(Model);
        objectTypes = ConnectorTeklaStructuresUtils.ObjectIDsTypesAndNames
            .Select(pair => pair.Value.Item1).Distinct().ToList();
        //objectIds = ConnectorTeklaStructuresUtils.ObjectIDsTypesAndNames.Select(pair => pair.Key).ToList();
        Model.GroupDef.GetNameList(ref numNames, ref groupNames);
        groups = groupNames.ToList();
      }

      return new List<ISelectionFilter>()
            {
            new ManualSelectionFilter(),
            new ListSelectionFilter {Slug="type", Name = "Categories",
                Icon = "Category", Values = objectTypes,
                Description="Adds all objects belonging to the selected types"},
        //new PropertySelectionFilter{
        //  Slug="param",
        //  Name = "Param",
        //  Description="Adds  all objects satisfying the selected parameter",
        //  Icon = "FilterList",
        //  HasCustomProperty = false,
        //  Values = objectNames,
        //  Operators = new List<string> {"equals", "contains", "is greater than", "is less than"}
        //},
            new AllSelectionFilter {Slug="all",  Name = "All",
                Icon = "CubeScan", Description = "Selects all document objects." },


            new ListSelectionFilter { Slug = "group", Name = "Group",
            Icon = "SelectGroup", Values = groups, Description = "Add all objects belonging to TeklaStructures Group" }
            };
    }

    public override void SelectClientObjects(string args)
    {
      throw new NotImplementedException();
    }

    private List<string> GetSelectionFilterObjects(ISelectionFilter filter)
    {
      var doc = Model;

      var selection = new List<string>();

      switch (filter.Slug)
      {
        case "manual":
          return GetSelectedObjects();
        case "all":
          if (ConnectorTeklaStructuresUtils.ObjectIDsTypesAndNames == null)
          {
            ConnectorTeklaStructuresUtils.GetObjectIDsTypesAndNames(Model);
          }
          selection.AddRange(ConnectorTeklaStructuresUtils.ObjectIDsTypesAndNames
                      .Select(pair => pair.Key).ToList());
          return selection;


        case "type":
          var typeFilter = filter as ListSelectionFilter;
          if (ConnectorTeklaStructuresUtils.ObjectIDsTypesAndNames == null)
          {
            ConnectorTeklaStructuresUtils.GetObjectIDsTypesAndNames(Model);
          }
          foreach (var type in typeFilter.Selection)
          {
            selection.AddRange(ConnectorTeklaStructuresUtils.ObjectIDsTypesAndNames
                .Where(pair => pair.Value.Item1 == type)
                .Select(pair => pair.Key)
                .ToList());
          }
          return selection;
        case "group":
          //Clear objects first
          Model.SelectObj.ClearSelection();
          var groupFilter = filter as ListSelectionFilter;
          foreach (var group in groupFilter.Selection)
          {
            Model.SelectObj.Group(group);
          }
          return GetSelectedObjects();



          /// TeklaStructures doesn't list fields of different objects. 
          /// For "param" search, maybe search over the name of
          /// methods of each type?

          //case "param":
          //    try
          //    {
          //        if (ConnectorTeklaStructuresUtils.ObjectTypes.Count == 0)
          //        {
          //            var _ = ConnectorTeklaStructuresUtils.GetObjectTypesAndNames(Model);
          //        }

          //        var propFilter = filter as PropertySelectionFilter;
          //        var query = new FilteredElementCollector(doc)
          //          .WhereElementIsNotElementType()
          //          .WhereElementIsNotElementType()
          //          .WhereElementIsViewIndependent()
          //          .Where(x => x.IsPhysicalElement())
          //          .Where(fi => fi.LookupParameter(propFilter.PropertyName) != null);

          //        propFilter.PropertyValue = propFilter.PropertyValue.ToLowerInvariant();

          //        switch (propFilter.PropertyOperator)
          //        {
          //            case "equals":
          //                query = query.Where(fi =>
          //                  GetStringValue(fi.LookupParameter(propFilter.PropertyName)) == propFilter.PropertyValue);
          //                break;
          //            case "contains":
          //                query = query.Where(fi =>
          //                  GetStringValue(fi.LookupParameter(propFilter.PropertyName)).Contains(propFilter.PropertyValue));
          //                break;
          //            case "is greater than":
          //                query = query.Where(fi => RevitVersionHelper.ConvertFromInternalUnits(
          //                                            fi.LookupParameter(propFilter.PropertyName).AsDouble(),
          //                                            fi.LookupParameter(propFilter.PropertyName)) >
          //                                          double.Parse(propFilter.PropertyValue));
          //                break;
          //            case "is less than":
          //                query = query.Where(fi => RevitVersionHelper.ConvertFromInternalUnits(
          //                                            fi.LookupParameter(propFilter.PropertyName).AsDouble(),
          //                                            fi.LookupParameter(propFilter.PropertyName)) <
          //                                          double.Parse(propFilter.PropertyValue));
          //                break;
          //        }

          //        selection = query.ToList();
          //    }
          //    catch (Exception e)
          //    {
          //        Log.CaptureException(e);
          //    }
          //    return selection;
      }

      return selection;
    }
  }
}
