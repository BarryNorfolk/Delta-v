using System.Diagnostics.CodeAnalysis;

namespace Content.Server._DV.Machines.Components;

[DataDefinition]
[Serializable]
public sealed partial class MachinePart
{
    [DataField]
    public string Name = "";

    [DataField(required: true)]
    public string Component = "";

    // Filled out during ComponentStartup
    [DataField]
    public Type? ComponentType = null;

    [DataField(required: true)]
    public Vector2i Offset;

    [DataField]
    public bool Assembled = false;

    [DataField]
    public EntityUid? Entity = null;
}

[RegisterComponent]
public sealed partial class MultipartMachineComponent : Component
{
    //Ideally this would be a dictionary but vOv, can't get it to serialize
    [DataField]
    public List<MachinePart> Parts = [];

    public EntityUid? GetEnt(string name)
    {
        foreach (var item in Parts)
        {
            if (item.Name == name)
            {
                return item.Entity;
            }
        }

        return null;
    }

    public bool GetEnt(string name, out EntityUid? entity)
    {
        entity = null;
        foreach (var item in Parts)
        {
            if (item.Name == name)
            {
                entity = item.Entity;
                return true;
            }
        }

        return false;
    }
}
