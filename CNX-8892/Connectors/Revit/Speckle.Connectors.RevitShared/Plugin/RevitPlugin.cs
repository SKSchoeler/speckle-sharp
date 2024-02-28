using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Reflection;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Revit.Async;
using CefSharp;
using System.Linq;
using System.IO;
using Speckle.Connectors.DUI.Bridge;
using Speckle.Connectors.DUI.Bindings;
using Autofac;
using Speckle.Connectors.Revit.HostApp;
using System.Diagnostics;
using Autodesk.Revit.DB;
using CefSharp.Event;
using Sentry.Protocol;

namespace Speckle.Connectors.Revit.Plugin;

internal class RevitPlugin : IRevitPlugin
{
  private readonly UIControlledApplication _uIControlledApplication;
  private readonly RevitSettings _revitSettings;
  private readonly IEnumerable<Lazy<IBinding>> _bindings; // should be lazy to ensure the bindings are not created too early
  private readonly BindingOptions _bindingOptions;
  private readonly CefSharpPanel _panel;
  private readonly RevitContext _revitContext;
  private readonly IBrowserSender _browserSender;

  public RevitPlugin(
    UIControlledApplication uIControlledApplication,
    RevitSettings revitSettings,
    IEnumerable<Lazy<IBinding>> bindings,
    BindingOptions bindingOptions,
    RevitContext revitContext,
    IBrowserSender browserSender
  )
  {
    _uIControlledApplication = uIControlledApplication;
    _revitSettings = revitSettings;
    _bindings = bindings;
    _bindingOptions = bindingOptions;
    _revitContext = revitContext;
    _browserSender = browserSender;
  }

  public void Initialise()
  {
    _uIControlledApplication.ControlledApplication.ApplicationInitialized += OnApplicationInitialized;

    CreateTabAndRibbonPanel(_uIControlledApplication);
  }

  public void Shutdown()
  {
    // POC: should we be cleaning up the RibbonPanel etc...
    // Should we be indicating to any active in-flight functions that we are being closed?
  }

  // POC: Could be injected but maybe not worthwhile
  private void CreateTabAndRibbonPanel(UIControlledApplication application)
  {
    // POC: some top-level handling and feedback here
    try
    {
      application.CreateRibbonTab(_revitSettings.RevitTabName);
    }
    catch (ArgumentException)
    {
      throw;
    }

    RibbonPanel specklePanel = application.CreateRibbonPanel(_revitSettings.RevitTabName, _revitSettings.RevitTabTitle);
    PushButton _ =
      specklePanel.AddItem(
        new PushButtonData(
          _revitSettings.RevitButtonName,
          _revitSettings.RevitButtonText,
          typeof(RevitExternalApplication).Assembly.Location,
          typeof(SpeckleRevitCommand).FullName
        )
      ) as PushButton;
  }

  private void OnApplicationInitialized(object sender, Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
  {
    var uiApplication = new UIApplication(sender as Application);
    _revitContext.UIApplication = uiApplication;

    // POC: might be worth to interface this out, we shall see...
    RevitTask.Initialize(uiApplication);

    RegisterPanelAndInitializePlugin();
  }

  private void RegisterPanelAndInitializePlugin()
  {
    CefSharpSettings.ConcurrentTaskExecution = true;

    var panel = new CefSharpPanel();
    panel.Browser.JavascriptObjectRepository.NameConverter = null;
    _browserSender.SetActionScriptMethod(panel.ExecuteScriptAsync);

    // binding the bindings to each bridge
    List<IBinding> bindings = _bindings.Select(x => x.Value).ToList();
    foreach (IBinding binding in bindings)
    {
      Debug.WriteLine(binding.Name);
      binding.Parent.AssociateWithBinding(binding, panel);

      panel.Browser.JavascriptObjectRepository.Register(binding.Name, binding.Parent, true, _bindingOptions);

      // POC: something wrong here
      // _browserSender.SendRaw($"console.log('Registered: {binding.Name}')");
    }

    // connect all the events AFTER they are bound
    foreach (IBinding binding in bindings)
    {
      Debug.WriteLine(binding.Name);
      binding.ConnectEvents();

      // POC: something wrong here
      // _browserSender.SendRaw($"console.log('Events connected: {binding.Name}')");
    }

    //panel.Browser.JavascriptObjectRepository.ResolveObject += (object sender, JavascriptBindingEventArgs e) =>
    //{
    //  // POC: debugging
    //  var csharpObject = bindings.FirstOrDefault(x => x.Name.ToLower() == e.ObjectName.ToLower());
    //  if (csharpObject != null)
    //  {
    //    // POC: logging
    //    Debug.WriteLine("FOUND: " + e.ObjectName);
    //    panel.Browser.ExecuteScriptAsync($"console.log('{e.ObjectName}')");

    //    panel.Browser.JavascriptObjectRepository.Register(
    //      csharpObject.Name,
    //      csharpObject.Parent,
    //      true,
    //      _bindingOptions
    //    );
    //  }
    //  else
    //  {
    //    Debug.WriteLine("*** NOT FOUND: " + e.ObjectName);
    //  }
    //};

    _uIControlledApplication.RegisterDockablePane(
      RevitExternalApplication.DoackablePanelId,
      _revitSettings.RevitPanelName,
      panel
    );

    panel.Browser.IsBrowserInitializedChanged += (sender, e) =>
    {
      if (panel.Browser.IsBrowserInitialized)
      {
        panel.Browser.Address = "https://deploy-preview-2076--boisterous-douhua-e3cefb.netlify.app/";
      }

      // POC dev tools
      panel.ShowDevTools();

      // POC: not sure where this comes from
#if REVIT2020
              // NOTE: Cef65 does not work with DUI3 in yarn dev mode. To test things you need to do `yarn build` and serve the build
              // folder at port 3000 (or change it to something else if you want to). Guru  meditation: Je sais, pas ideal. Mais q'est que nous pouvons faire? Rien. C'est l'autodesk vie.
              // NOTE: To run the ui from a build, follow these steps:
              // - run `yarn build` in the DUI3 folder
              // - run ` PORT=3003  node .output/server/index.mjs` after the build
    
              CefSharpPanel.Browser.Load("http://localhost:3003");
              CefSharpPanel.Browser.ShowDevTools();
#endif
#if REVIT2023
              CefSharpPanel.Browser.Load("http://localhost:8082");
#endif
    };
  }

  private void JavascriptObjectRepository_ResolveObject(object sender, JavascriptBindingEventArgs e) =>
    throw new NotImplementedException();
}
