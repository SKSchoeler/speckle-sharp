using System;
using System.Linq;
using System.Windows.Threading;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Speckle.Autofac.DependencyInjection;
using Speckle.Autofac.Files;
using Speckle.Connectors.DUI.Bridge;
using Speckle.Connectors.Revit.DependencyInjection;
using CefSharp;
using CefSharp.DevTools;
using System.Reflection;
using System.IO;

namespace Speckle.Connectors.Revit.Plugin;

internal class RevitExternalApplication : IExternalApplication
{
  private IRevitPlugin? _revitPlugin = null;
  private AutofacContainer? _container = null;

  // POC: this is getting hard coded - need a way of injecting it
  //      I am beginning to think the shared project is not the way
  //      and an assembly which is invoked with some specialisation is the right way to go
  //      maybe subclassing, or some hook to inject som configuration
  private readonly RevitSettings _revitSettings;

  public static readonly DockablePaneId DoackablePanelId = new(new Guid("{f7b5da7c-366c-4b13-8455-b56f433f461e}"));

  public RevitExternalApplication()
  {
    // POC: load from JSON file?
    _revitSettings = new RevitSettings
    {
      RevitPanelName = "Speckle DUI3 (DI)",
      RevitTabName = "Speckle",
      RevitTabTitle = "Speckle DUI3 (DI)",
      RevitVersionName = "2023",
      RevitButtonName = "Speckle DUI3 (DI)",
      RevitButtonText = "Revit Connector"
    };
  }

  public Result OnStartup(UIControlledApplication application)
  {
    try
    {
      // POC: not sure what this is doing...  could be messing up our Aliasing????
      AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

      _container = new AutofacContainer(new StorageInfo());

      // POC: re-instate, can this be done with some injected class?
#if REVIT2020
              // Panel.Browser.JavascriptObjectRepository.NameConverter = null; // not available in cef65, we need the below
              BindingOptions bindingOptions = new () { CamelCaseJavascriptNames = false };
#endif

      //#if REVIT2023

      // init DI
      _container
        //.LoadAutofacModules(new string[] { "<paths>" }) // TODO, it's coming
        .AddModule(new AutofacUIModule())
        .AddSingletonInstance<BindingOptions>(BindingOptions.DefaultBinder) // this is for 2023, POC: re-instate above for 2020
        .AddSingletonInstance<RevitSettings>(_revitSettings) // apply revit settings into DI
        .AddSingletonInstance<UIControlledApplication>(application) // inject UIControlledApplication application
        .Build();

      // resolve root object
      _revitPlugin = _container.Resolve<IRevitPlugin>();
      _revitPlugin.Initialise();
    }
    catch (Exception ex)
    {
      // POC: feedback?
      return Result.Failed;
    }

    return Result.Succeeded;
  }

  public Result OnShutdown(UIControlledApplication application)
  {
    try
    {
      // POC: could this be more a generic Connector Init() Shutdown()
      // possibly with injected pieces or with some abstract methods?
      // need to look for commonality
      _revitPlugin.Shutdown();
    }
    catch (Exception ex)
    {
      // POC: feedback?
      return Result.Failed;
    }

    return Result.Succeeded;
  }

  private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
  {
    // POC: tight binding to files
    Assembly assembly = null;
    string name = args.Name.Split(',')[0];
    string path = Path.GetDirectoryName(typeof(RevitPlugin).Assembly.Location);

    if (path != null)
    {
      string assemblyFile = Path.Combine(path, name + ".dll");

      if (File.Exists(assemblyFile))
      {
        assembly = Assembly.LoadFrom(assemblyFile);
      }
    }

    return assembly;
  }
}
