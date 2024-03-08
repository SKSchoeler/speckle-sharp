using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DUI3;
using DUI3.Bindings;
using DUI3.Models;

namespace ConnectorArcGIS.Bindings;

public class SendBinding : ISendBinding
{
  public string Name { get; set; } = "sendBinding";
  public IBridge Parent { get; set; }

  private readonly DocumentModelStore _store;

  public SendBinding(DocumentModelStore store)
  {
    _store = store;
  }

  public void CancelSend(string modelCardId) => throw new NotImplementedException();

  public List<ISendFilter> GetSendFilters() => throw new NotImplementedException();

  public void Send(string modelCardId)
  {
    Debug.WriteLine(modelCardId);
  }

  public string TestString(string test)
  {
    return test;
  }
}
