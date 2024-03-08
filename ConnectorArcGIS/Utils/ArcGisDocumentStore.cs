using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using DUI3.Models;

namespace ConnectorArcGIS.Utils;

public class ArcGisDocumentStore : DocumentModelStore
{
  public ArcGisDocumentStore()
  {
    // Subscribe here document related events like OnSave, OnClose, OnOpen etc...
  }

  public override async void WriteToFile()
  {
    // Implement the logic to save it to file
    await QueuedTask
      .Run(
        () =>
          Project.Current.SaveMetadataAsHTML(
            @"C:\Users\katri\Documents\ArcGIS\Projects\OutputHTML.htm",
            MDSaveAsHTMLOption.esriCurrentMetadataStyle
          )
      )
      .ConfigureAwait(false);
  }

  public override void ReadFromFile()
  {
    // Implement the logic to read it from file
  }
}
