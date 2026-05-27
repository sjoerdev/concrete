using SharpGLTF.Runtime;

namespace Concrete;

public class Animator : Component
{
    [Show("playOnAwake")] public bool playOnAwake = false;

    private bool playing = false;
    private bool looping = false;
    private float animationTime = 0;
    private int currentAnimation = 0;

    private MeshRenderer meshRenderer;
    private ArmatureInstance armature;

    public override void Start()
    {
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        armature = meshRenderer.AnimationArmature;
        if (playOnAwake) if (armature.AnimationTracks.Count > 0) PlayAnimation(0, true);
    }

    public override void Update(float deltaTime)
    {
        if (playing)
        {
            animationTime += deltaTime;

            if (animationTime >= armature.AnimationTracks[currentAnimation].Duration)
            {
                if (looping)
                {
                    animationTime = 0;
                }
                else
                {
                    playing = false;
                    animationTime = 0;
                }
            }

            armature.SetAnimationFrame(currentAnimation, animationTime);
        }
    }

    public void PlayAnimation(int animation, bool looping)
    {
        playing = true;
        currentAnimation = animation;
        this.looping = looping;
    }
}