#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ProjectEclipse.EditorTools
{
    [InitializeOnLoad]
    public static class AnimationAssetGenerator
    {
        private const string AnimationRoot = "Assets/ProjectEclipse/Animations";

        static AnimationAssetGenerator()
        {
            EditorApplication.delayCall += EnsurePlaceholderAnimationAssets;
        }

        [MenuItem("Project Eclipse/Generate Placeholder Animation Assets")]
        public static void EnsurePlaceholderAnimationAssets()
        {
            if (!Directory.Exists(AnimationRoot))
            {
                Directory.CreateDirectory(AnimationRoot);
            }

            CreateController("Player", true);
            CreateController("Enemy", false);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateController(string prefix, bool playerController)
        {
            string controllerPath = AnimationRoot + "/" + prefix + ".controller";
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath) != null)
            {
                return;
            }

            AnimationClip idle = CreateClip(prefix + "_Idle");
            AnimationClip move = CreateClip(prefix + "_Move");
            AnimationClip attack = CreateClip(prefix + "_Attack");
            AnimationClip hurt = CreateClip(prefix + "_Hurt");
            AnimationClip die = CreateClip(prefix + "_Die");

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState idleState = machine.AddState("Idle");
            AnimatorState moveState = machine.AddState("Move");
            AnimatorState attackState = machine.AddState("Attack");
            AnimatorState hurtState = machine.AddState("Hurt");
            AnimatorState dieState = machine.AddState("Die");

            idleState.motion = idle;
            moveState.motion = move;
            attackState.motion = attack;
            hurtState.motion = hurt;
            dieState.motion = die;
            machine.defaultState = idleState;

            AnimatorStateTransition idleToMove = idleState.AddTransition(moveState);
            idleToMove.hasExitTime = false;
            idleToMove.duration = 0.05f;
            idleToMove.AddCondition(AnimatorConditionMode.If, 0f, "IsMoving");

            AnimatorStateTransition moveToIdle = moveState.AddTransition(idleState);
            moveToIdle.hasExitTime = false;
            moveToIdle.duration = 0.05f;
            moveToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsMoving");

            AddTriggeredState(machine, attackState, idleState, "Attack", 0.12f);
            AddTriggeredState(machine, hurtState, idleState, "Hurt", 0.1f);
            AddTriggeredState(machine, dieState, dieState, "Die", 0f);

            if (playerController)
            {
                AddGroundedReturn(moveState, idleState);
            }

            EditorUtility.SetDirty(controller);
        }

        private static AnimationClip CreateClip(string name)
        {
            string path = AnimationRoot + "/" + name + ".anim";
            AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
            {
                return existing;
            }

            AnimationClip clip = new AnimationClip();
            clip.name = name;
            clip.frameRate = 12f;
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void AddTriggeredState(AnimatorStateMachine machine, AnimatorState state, AnimatorState returnState, string triggerName, float duration)
        {
            AnimatorStateTransition any = machine.AddAnyStateTransition(state);
            any.hasExitTime = false;
            any.duration = 0f;
            any.AddCondition(AnimatorConditionMode.If, 0f, triggerName);

            if (state == returnState)
            {
                return;
            }

            AnimatorStateTransition exit = state.AddTransition(returnState);
            exit.hasExitTime = true;
            exit.exitTime = 1f;
            exit.duration = duration;
        }

        private static void AddGroundedReturn(AnimatorState moveState, AnimatorState idleState)
        {
            AnimatorStateTransition groundedReturn = moveState.AddTransition(idleState);
            groundedReturn.hasExitTime = false;
            groundedReturn.duration = 0.05f;
            groundedReturn.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
        }
    }
}
#endif

