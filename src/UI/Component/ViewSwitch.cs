using System;
using Godot;

namespace Nasara.UI.Component;

/// <summary>
/// A simple pack for `PageSwitch`, use in AppLayout
/// </summary>
public partial class ViewSwitch : PageSwitch
{
    /// <summary>
    /// Add a packed scene to switch
    /// </summary>
    /// <param name="packedScene">Packed to add</param>
    /// <returns>Index of added scene</returns>
    public int AddView(PackedScene packedScene)
    {
        var node = packedScene.Instantiate<Control>();
        AddChild(node);
        Pages = [.. Pages, node];

        return Pages.Length - 1;
    }
    
    /// <summary>
    /// Add a scene to switch
    /// </summary>
    /// <param name="page">Node to add (not in tree)</param>
    /// <returns>Index of added scene</returns>
    public int AddView(Control page)
    {
        if (page.IsInsideTree()) throw new Exception("page is inside tree");
        
        AddChild(page);
        Pages = [.. Pages, page];

        return Pages.Length - 1;
    }
}