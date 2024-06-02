using System.Collections.Generic;

namespace DLSv2.Core;

public abstract class BaseModeInstance<T> where T: BaseMode
{
    public T BaseMode { get; }
        
    public bool Enabled = false;
    public bool EnabledByTrigger = false;

    public BaseModeInstance(T mode)
    {
        BaseMode = mode;
    }
}

public abstract class BaseControlGroupInstance<T, U>
    where T : BaseControlGroup<U>
    where U : BaseModeSelection
{
    public T BaseControlGroup { get; }

    public bool Enabled => ActiveIndexes.Count > 0;
    public List<int> ActiveIndexes = new();

    public BaseControlGroupInstance(T cg)
    {
        BaseControlGroup = cg;
    }

    public void Toggle(int newIndex = 0)
    {
        if (Enabled)
            ActiveIndexes = new();
        else
            ActiveIndexes = new() { newIndex };
    }

    public virtual void Disable()
    {
        ActiveIndexes = new();
    }

    public void MoveToNext(bool cycleOnly = false)
    {
        var newIndex = ActiveIndexes.Count > 0 ? ActiveIndexes[0] + 1: 0;

        if (newIndex >= BaseControlGroup.Modes.Count)
        {
            if (cycleOnly || BaseControlGroup is LightControlGroup)
                ActiveIndexes = [];
            else
                ActiveIndexes = [0];
            
            return;
        }

        ActiveIndexes = [newIndex];
    }

    public void MoveToPrevious(bool cycleOnly = false)
    {
        var newIndex = ActiveIndexes.Count > 0 ? ActiveIndexes[0] - 1 : BaseControlGroup.Modes.Count - 1;

        if (newIndex < 0)
        {
            if (cycleOnly || BaseControlGroup is LightControlGroup)
                ActiveIndexes = [];
            else
                ActiveIndexes = [BaseControlGroup.Modes.Count - 1];
            
            return;
        }

        ActiveIndexes = [newIndex];
    }
}

public class LightControlGroupInstance : BaseControlGroupInstance<LightControlGroup, LightModeSelection>
{
    public LightControlGroupInstance(LightControlGroup cg) : base(cg) { }
}

public class AudioControlGroupInstance : BaseControlGroupInstance<AudioControlGroup, AudioModeSelection>
{

    public bool ManualingEnabled = false;
    public int ManualingIndex = 0;

    public AudioControlGroupInstance(AudioControlGroup cg) : base(cg) { }

    public override void Disable()
    {
        base.Disable();

        ManualingEnabled = false;
        ManualingIndex = 0;
    }
}

public class LightModeInstance : BaseModeInstance<LightMode>
{
    public LightModeInstance(LightMode mode) : base(mode) { }
}

public class AudioModeInstance : BaseModeInstance<AudioMode>
{
    public AudioModeInstance(AudioMode mode) : base(mode) { }
}