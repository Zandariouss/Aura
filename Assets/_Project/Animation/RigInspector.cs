using System.Text;
using UnityEngine;

// Prints a humanoid rig's bones and body measurements to the Console.
// Diagnostic only: it never modifies the character.
public class RigInspector : MonoBehaviour
{
    [ContextMenu("Inspect Rig")]
    private void Inspect()
    {
        Animator animator = GetComponentInChildren<Animator>();
        if (animator == null || !animator.isHuman)
        {
            Debug.LogWarning("No humanoid Animator found on this object or its children.");
            return;
        }

        StringBuilder report = new StringBuilder();
        int found = 0;

        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            HumanBodyBones id = (HumanBodyBones)i;
            Transform bone = animator.GetBoneTransform(id);
            if (bone == null) continue;

            found++;
            string parent = bone.parent != null ? bone.parent.name : "none";
            report.AppendLine($"{id}: {bone.name} (parent: {parent})");
        }

        Transform upper = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        Transform lower = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        Transform foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);

        if (upper != null && lower != null && foot != null && hips != null)
        {
            float legLength = Vector3.Distance(upper.position, lower.position)
                            + Vector3.Distance(lower.position, foot.position);
            float hipHeight = hips.position.y - animator.transform.position.y;
            report.AppendLine($"Leg length: {legLength:F3}");
            report.AppendLine($"Hip height above root: {hipHeight:F3}");
        }

        Debug.Log($"Rig report: {found} bones mapped\n{report}");
    }
}