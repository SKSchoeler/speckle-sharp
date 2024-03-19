/*

   Copyright 2022 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/

using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace Speckle.Connectors.ArcGIS.HostApp;

internal class SpeckleDUI3ViewModel : DockPane
{
  private const string DockPaneId = "SpeckleDUI3_SpeckleDUI3";

  internal static void Create()
  {
    SQLitePCL.Batteries.Init();
    var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId);
    pane?.Activate();
  }

  /// <summary>
  /// Called when the pane is initialized.
  /// </summary>
  protected override async Task InitializeAsync()
  {
    await base.InitializeAsync();
  }

  /// <summary>
  /// Called when the pane is uninitialized.
  /// </summary>
  protected override async Task UninitializeAsync()
  {
    await base.UninitializeAsync();
  }
}

/// <summary>
/// Button implementation to create a new instance of the pane and activate it.
/// </summary>
internal class SpeckleDUI3OpenButton : Button
{
  protected override void OnClick()
  {
    SpeckleDUI3ViewModel.Create();
  }
}
