using System.Collections.Generic;

namespace net.jommy.RuuviCore.Bluez.Models;

public class PropertyChanges<TProperties>
{
    public PropertyChanges(TProperties properties, HashSet<string> invalidated, HashSet<string> changed)
    {
        (Properties, Invalidated, Changed) = (properties, invalidated, changed);
    }

    public TProperties Properties { get; }
    private HashSet<string> Invalidated { get; }
    private HashSet<string> Changed { get; }

    public bool HasChanged(string property)
    {
        return Changed.Contains(property);
    }

    public bool IsInvalidated(string property)
    {
        return Invalidated.Contains(property);
    }
}