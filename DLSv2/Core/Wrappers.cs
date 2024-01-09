namespace DLSv2.Core
{
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

        public bool Enabled = false;
        public int Index = 0;

        public BaseControlGroupInstance(T cg)
        {
            BaseControlGroup = cg;
        }

        public void Toggle(bool toggleOnly = false)
        {
            Enabled = !Enabled;

            if (toggleOnly && Enabled == false)
                MoveToNext(true);
        }

        public virtual void Disable()
        {
            Enabled = false;
            Index = 0;
        }

        public void MoveToNext(bool fromToggle = false, bool cycleOnly = false)
        {
            var previousStatus = Enabled;
            var prevIndex = Index;
            var newIndex = prevIndex + 1;
            
            if (previousStatus == false && !fromToggle)
            {
                if (Index == 0 || Index == BaseControlGroup.Modes.Count - 1)
                {
                    Enabled = true;
                    Index = 0;
                    return;
                }
            }
            
            if (newIndex >= BaseControlGroup.Modes.Count)
            {
                if (BaseControlGroup is AudioControlGroup)
                    Enabled = fromToggle ? previousStatus : !cycleOnly;
                else
                    Enabled = fromToggle && previousStatus;
                Index = 0;
            }
            else
            {
                Enabled = !fromToggle || previousStatus;
                Index = newIndex;
            }
        }

        public void MoveToPrevious(bool cycleOnly = false)
        {
            var prevIndex = Index;
            var newIndex = prevIndex - 1;
            
            if (Enabled == false)
            {
                if (Index == 0)
                {
                    Enabled = true;
                    Index = BaseControlGroup.Modes.Count - 1;
                    return;
                }
            }
            
            if (newIndex < 0)
            {
                if (BaseControlGroup is AudioControlGroup)
                {
                    Enabled = !cycleOnly;
                    Index = cycleOnly ? 0 : BaseControlGroup.Modes.Count - 1;
                }
                else
                {
                    Enabled = false;
                    Index = 0;
                }
            }
            else
            {
                Enabled = true;
                Index = newIndex;
            }
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
}
