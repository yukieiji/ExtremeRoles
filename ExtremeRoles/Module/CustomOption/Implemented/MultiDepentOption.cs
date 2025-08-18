using System;
using System.Collections.Generic;
using System.Linq;
using ExtremeRoles.Module.CustomOption.Interfaces;

namespace ExtremeRoles.Module.CustomOption.Implemented;

public sealed class MultiDepentOption : IOldOption
{
    private readonly List<IOldOption> parents;
    private readonly Func<IReadOnlyList<IOldOption>, bool> predicate;

    public MultiDepentOption(
        IOptionInfo info,
        List<IOldOption> parents,
        Func<IReadOnlyList<IOldOption>, bool> predicate,
        IOptionRelation relation)
    {
        this.Info = info;
        this.parents = parents;
        this.predicate = predicate;
        this.Relation = relation;
    }

    public IOptionInfo Info { get; }
    public IOptionRelation Relation { get; }

    public string Title => string.Empty;
    public string ValueString => this.IsEnable.ToString();

    public int Range => 1;
    public int Selection { get; set; }

    public bool IsEnable => this.predicate(this.parents);
    public bool IsActiveAndEnable => this.IsEnable && this.Relation.Parent.IsActiveAndEnable;

    public void SwitchPreset()
    {
    }
}
