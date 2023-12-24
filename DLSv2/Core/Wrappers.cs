namespace DLSv2.Core
{
    public class ModeInstance<T>
    {
        public T BaseMode { get; }
        // General
        public bool Enabled;
        public bool EnabledByTrigger;

        public ModeInstance(T mode)
        {
            BaseMode = mode;
            Enabled = false;
            EnabledByTrigger = false;
        }
    }

    public class ControlGroupInstance<T>
    {
        public T BaseControlGroup { get; }
        // General
        public bool Enabled;
        public int Index;
        // Audio
        public bool ManualingEnabled;
        public int ManualingIndex;

        public ControlGroupInstance(T cg)
        {
            BaseControlGroup = cg;
            Enabled = false;
            Index = 0;
            ManualingEnabled = false;
            ManualingIndex = 0;
        }

        public void Toggle(bool toggleOnly = false)
        {
            Enabled = !Enabled;

            if (toggleOnly && Enabled == false)
                MoveToNext(true);
        }

        public void Disable()
        {
            Enabled = false;
            Index = 0;
            ManualingEnabled = false;
            ManualingIndex = 0;
        }

        public void MoveToNext(bool fromToggle = false, bool cycleOnly = false)
        {
            var previousStatus = Enabled;
            var prevIndex = Index;
            var newIndex = prevIndex + 1;

            switch (this)
            {
                case ControlGroupInstance<ControlGroup> lightCG:
                {
                    if (previousStatus == false && !fromToggle)
                    {
                        if (Index == 0 || Index == lightCG.BaseControlGroup.Modes.Count - 1)
                        {
                            Enabled = true;
                            Index = 0;
                            return;
                        }
                    }
                    if (newIndex >= lightCG.BaseControlGroup.Modes.Count)
                    {
                        Enabled = fromToggle && previousStatus;
                        Index = 0;
                    }
                    else
                    {
                        Enabled = !fromToggle || previousStatus;
                        Index = newIndex;
                    }
                    break;
                }
                case ControlGroupInstance<AudioControlGroup> audioCG:
                    if (previousStatus == false && !fromToggle)
                    {
                        if (Index == 0 || Index == audioCG.BaseControlGroup.Modes.Count - 1)
                        {
                            Enabled = true;
                            Index = 0;
                            return;
                        }
                    }
                    if (newIndex >= audioCG.BaseControlGroup.Modes.Count)
                    {
                        Enabled = fromToggle ? previousStatus : !cycleOnly;
                        Index = 0;
                    }
                    else
                    {
                        Enabled = !fromToggle || previousStatus;
                        Index = newIndex;
                    }
                    break;
            }
        }

        public void MoveToPrevious(bool cycleOnly = false)
        {
            var prevIndex = Index;
            var newIndex = prevIndex - 1;

            switch (this)
            {
                case ControlGroupInstance<ControlGroup> lightCG:
                {
                    if (Enabled == false)
                    {
                        if (Index == 0)
                        {
                            Enabled = true;
                            Index = lightCG.BaseControlGroup.Modes.Count - 1;
                            return;
                        }
                    }
                    if (newIndex < 0)
                    {
                        Enabled = false;
                        Index = 0;
                    }
                    else
                    {
                        Enabled = true;
                        Index = newIndex;
                    }
                    break;
                }
                case ControlGroupInstance<AudioControlGroup> audioCG:
                    if (Enabled == false)
                    {
                        if (Index == 0)
                        {
                            Enabled = true;
                            Index = audioCG.BaseControlGroup.Modes.Count - 1;
                            return;
                        }
                    }
                    if (newIndex < 0)
                    {
                        Enabled = !cycleOnly;
                        Index = cycleOnly ? 0 : audioCG.BaseControlGroup.Modes.Count - 1;
                    }
                    else
                    {
                        Enabled = true;
                        Index = newIndex;
                    }
                    break;
            }
        }
    }
}
