using UnityEngine;

// Procedural walk for a humanoid rig. Put on Player (next to Character Controller).
// Disables the model's Animator at Start and drives the bones from code.
public class ProceduralWalk : MonoBehaviour
{
    [Header("References (auto-found if empty)")]
    public Animator animator;
    public CharacterController controller;

    [Header("Legs")]
    public float strideLength = 1.7f;    // distance per full cycle (two steps)
    public float maxLegSwing = 30f;      // degrees
    public float maxKneeBend = 65f;      // degrees, during swing phase
    public float kneeBase = 8f;          // slight bend at all times
    public float footRoll = 25f;         // toe-off angle
    public float speedForFullSwing = 8f;
    public float minWalkAmount = 0.45f;  // so even a slow walk reads clearly
    public float blendSpeed = 8f;

    [Header("Body")]
    public float bobHeight = 0.03f;      // meters
    public float swayAmount = 0.02f;     // meters, side to side over the stance foot
    public float hipTwist = 6f;          // degrees, pelvis rotation
    public float spineTwist = 8f;        // degrees, chest counter-rotation

    [Header("Arms")]
    public float armDrop = 40f;          // lowers arms from the T-pose
    public bool flipArmDrop = false;     // tick if arms go UP instead of down
    public float armSwing = 25f;         // degrees
    public float elbowBend = 25f;        // degrees

    [Header("If limbs move the wrong way")]
    public bool invertSwing = false;

    class Bone
    {
        public Transform t;
        public Quaternion rest;   // rotation relative to the model root
    }

    Transform root;
    Bone pelvis, spine1, spine2, spine3, head;
    Bone thighL, thighR, calfL, calfR, footL, footR;
    Bone armL, armR, foreL, foreR;
    Vector3 pelvisLocalPos;
    float phase, amp;

    Bone Make(HumanBodyBones id)
    {
        Transform t = animator.GetBoneTransform(id);
        if (t == null) return null;
        return new Bone { t = t, rest = Quaternion.Inverse(root.rotation) * t.rotation };
    }

    static void Apply(Bone b, Quaternion rootRot, Quaternion delta)
    {
        if (b != null) b.t.rotation = rootRot * delta * b.rest;
    }

    void Start()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (controller == null) controller = GetComponent<CharacterController>();

        if (animator == null || !animator.isHuman)
        {
            Debug.LogWarning("ProceduralWalk: no humanoid Animator found.");
            enabled = false;
            return;
        }

        root = animator.transform;

        pelvis = Make(HumanBodyBones.Hips);
        spine1 = Make(HumanBodyBones.Spine);
        spine2 = Make(HumanBodyBones.Chest);
        spine3 = Make(HumanBodyBones.UpperChest);
        head = Make(HumanBodyBones.Head);
        thighL = Make(HumanBodyBones.LeftUpperLeg);
        thighR = Make(HumanBodyBones.RightUpperLeg);
        calfL = Make(HumanBodyBones.LeftLowerLeg);
        calfR = Make(HumanBodyBones.RightLowerLeg);
        footL = Make(HumanBodyBones.LeftFoot);
        footR = Make(HumanBodyBones.RightFoot);
        armL = Make(HumanBodyBones.LeftUpperArm);
        armR = Make(HumanBodyBones.RightUpperArm);
        foreL = Make(HumanBodyBones.LeftLowerArm);
        foreR = Make(HumanBodyBones.RightLowerArm);

        pelvisLocalPos = pelvis.t.localPosition;
        animator.enabled = false;   // take over from the animation clips
    }

    void LateUpdate()
    {
        Vector3 v = controller != null ? controller.velocity : Vector3.zero;
        v.y = 0f;
        float speed = v.magnitude;
        bool moving = speed > 0.1f;

        float target = moving ? Mathf.Max(minWalkAmount, Mathf.Clamp01(speed / speedForFullSwing)) : 0f;
        amp = Mathf.Lerp(amp, target, 1f - Mathf.Exp(-blendSpeed * Time.deltaTime));
        float a = amp;

        if (moving) phase += speed / strideLength * Mathf.PI * 2f * Time.deltaTime;

        float s = invertSwing ? -1f : 1f;
        float sinL = Mathf.Sin(phase), cosL = Mathf.Cos(phase);
        float sinR = -sinL,            cosR = -cosL;

        Quaternion rr = root.rotation;
        Vector3 right = Vector3.right, up = Vector3.up, fwd = Vector3.forward;

        // ---- Pelvis and spine: hips twist one way, chest counter-twists ----
        float hipYaw = hipTwist * a * sinL;
        float chestYaw = -spineTwist * a * sinL;
        Apply(pelvis, rr, Quaternion.AngleAxis(hipYaw, up));
        Apply(spine1, rr, Quaternion.AngleAxis(Mathf.Lerp(hipYaw, chestYaw, 0.33f), up));
        Apply(spine2, rr, Quaternion.AngleAxis(Mathf.Lerp(hipYaw, chestYaw, 0.66f), up));
        Apply(spine3, rr, Quaternion.AngleAxis(chestYaw, up));
        Apply(head, rr, Quaternion.identity);   // head stays facing forward

        // Vertical bob plus a sway over the stance foot
        pelvis.t.localPosition = pelvisLocalPos;
        float bob = bobHeight * a * (1f - Mathf.Abs(sinL));
        float sway = swayAmount * a * cosL;
        pelvis.t.position += rr * (up * bob + right * sway);

        // ---- Legs (positive rotation about right axis swings a hanging leg backward) ----
        float thighL_a = -maxLegSwing * a * sinL * s;
        float thighR_a = -maxLegSwing * a * sinR * s;
        float kneeL = (kneeBase * a + maxKneeBend * a * Mathf.Max(0f, cosL)) * s;
        float kneeR = (kneeBase * a + maxKneeBend * a * Mathf.Max(0f, cosR)) * s;

        Apply(thighL, rr, Quaternion.AngleAxis(thighL_a, right));
        Apply(thighR, rr, Quaternion.AngleAxis(thighR_a, right));
        Apply(calfL, rr, Quaternion.AngleAxis(thighL_a + kneeL, right));
        Apply(calfR, rr, Quaternion.AngleAxis(thighR_a + kneeR, right));

        // Feet stay level, with toe-off at the back of the step and a slight toe-up at the front
        float footL_a = footRoll * a * (Mathf.Max(0f, -sinL) - 0.4f * Mathf.Max(0f, sinL)) * s;
        float footR_a = footRoll * a * (Mathf.Max(0f, -sinR) - 0.4f * Mathf.Max(0f, sinR)) * s;
        Apply(footL, rr, Quaternion.AngleAxis(footL_a, right));
        Apply(footR, rr, Quaternion.AngleAxis(footR_a, right));

        // ---- Arms: lower from the T-pose, then swing opposite to the same-side leg ----
        float ratio = maxLegSwing > 0.01f ? armSwing / maxLegSwing : 0f;
        float dropSign = flipArmDrop ? -1f : 1f;
        Quaternion dropL = Quaternion.AngleAxis(dropSign * armDrop, fwd);
        Quaternion dropR = Quaternion.AngleAxis(-dropSign * armDrop, fwd);

        float swingL = -thighL_a * ratio;
        float swingR = -thighR_a * ratio;

        // Elbows bend more when the arm is forward
        float elbowL = -elbowBend * Mathf.Lerp(0.4f, 1f, Mathf.Clamp01(-swingL * s / (armSwing + 0.01f))) * s;
        float elbowR = -elbowBend * Mathf.Lerp(0.4f, 1f, Mathf.Clamp01(-swingR * s / (armSwing + 0.01f))) * s;

        Apply(armL, rr, Quaternion.AngleAxis(swingL, right) * dropL);
        Apply(armR, rr, Quaternion.AngleAxis(swingR, right) * dropR);
        Apply(foreL, rr, Quaternion.AngleAxis(swingL + elbowL, right) * dropL);
        Apply(foreR, rr, Quaternion.AngleAxis(swingR + elbowR, right) * dropR);
    }
}