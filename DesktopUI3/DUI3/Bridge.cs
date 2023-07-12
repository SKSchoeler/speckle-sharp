using System.Reflection;
using System.Text.Json;
using Speckle.Core.Logging;

namespace DUI3
{

  /// <summary>
  /// Wraps a binding class, and manages its calls from the Frontend to .NET, and sending events from .NET to the the Frontend. 
  /// <para>See also: https://github.com/johot/WebView2-better-bridge</para>
  /// </summary>
  /// <typeparam name="TBrowser">The browser type (CefSharp or WebView2 currently supported.)</typeparam>
  public class BrowserBridge<TBrowser> : IBridge
  {
    /// <summary>
    /// The name under which we expect the frontend to hoist this bindings class to the global scope.
    /// e.g., `receiveBindings` should be available as `window.receiveBindings`. 
    /// </summary>
    public string FrontendBoundName { get; }

    public TBrowser Browser { get; }

    public IBinding Binding { get; }

    private MethodInfo ExecuteScriptAsyncMethod { get; set; }

    private Type BindingType { get; set; }
    private Dictionary<string, MethodInfo> BindingMethodCache { get; set; }

    public BrowserBridge(TBrowser browser, IBinding binding)
    {
      FrontendBoundName = binding.Name;
      Browser = browser;
      Binding = binding;
      
      BindingType = Binding.GetType(); 
      BindingMethodCache = new Dictionary<string, MethodInfo>();
      foreach(var m in BindingType.GetMethods())
      {
        BindingMethodCache[m.Name] = m;
      }

      Binding.Parent = this;

      // NOTE: For later, for older browsers, this can be replaced by something that does url hacks. 
      ExecuteScriptAsyncMethod = Browser.GetType().GetMethod("ExecuteScriptAsync");

      if (ExecuteScriptAsyncMethod == null)
      {
        throw new SpeckleException($"Unsupported browser type {Browser.GetType().AssemblyQualifiedName}.");
      }
    }

    /// <summary>
    /// Used by the Frontend bridge logic to understand which methods are available.
    /// </summary>
    /// <returns></returns>
    public string[] GetMethodNames() => BindingMethodCache.Keys.ToArray();

    public async Task<string> RunMethod(string methodName, string args)
    {
      if (!BindingMethodCache.ContainsKey(methodName))
        throw new SpeckleException($"Cannot find method {methodName} in bindings class {BindingType.AssemblyQualifiedName}.");

      var method = BindingMethodCache[methodName];
      var parameters = method.GetParameters();
      var jsonArgsArray = JsonSerializer.Deserialize<string[]>(args);

      if (parameters.Length != jsonArgsArray.Length)
        throw new SpeckleException($"Wrong number of arguments when invoking binding function {methodName}, expected {parameters.Length}, but got {jsonArgsArray.Length}.");

      var typedArgs = new object[jsonArgsArray.Length];

      for (int i = 0; i < typedArgs.Length; i++)
      {
        var typedObj = JsonSerializer.Deserialize(jsonArgsArray[i], parameters[i].ParameterType);
        typedArgs[i] = typedObj;
      }
      var resultTyped = method.Invoke(Binding, typedArgs);

      // Was it an async method (in bridgeClass?)
      var resultTypedTask = resultTyped as Task;

      string resultJson;

      // Was the method called async?
      if (resultTypedTask == null)
      {
        // Regular method: no need to await things
        resultJson = JsonSerializer.Serialize(resultTyped);
      }
      else // It's an async call
      {
        await resultTypedTask;

        // If has a "Result" property return the value otherwise null (Task<void> etc)
        var resultProperty = resultTypedTask.GetType().GetProperty("Result");
        var taskResult = resultProperty != null ? resultProperty.GetValue(resultTypedTask) : null;
        resultJson = JsonSerializer.Serialize(taskResult);
      }

      return resultJson;
    }

    /// <summary>
    /// Notifies the Frontend about something by doing the browser specific way for `browser.ExecuteScriptAsync("window.FrontendBoundName.on(eventName, etc.)")`. 
    /// </summary>
    /// <param name="eventData"></param>
    public void SendToBrowser(IHostAppEvent eventData)
    {
      var payload = JsonSerializer.Serialize(eventData);
      var script = $"{FrontendBoundName}.emit('{eventData.EventName}', '{payload}')";
      ExecuteScriptAsyncMethod.Invoke(Browser, new object[] { script });
    }

    /// <summary>
    /// Notifies the Frontend about something by doing the browser specific way for `browser.ExecuteScriptAsync("window.FrontendBoundName.on(eventName, etc.)")`. 
    /// </summary>
    /// <param name="eventData"></param>
    public void SendToBrowser(string eventName)
    {
      var script = $"bindings.emit('{eventName}')";
      ExecuteScriptAsyncMethod.Invoke(Browser, new object[] { script });
    }

  }

}
