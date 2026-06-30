using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectEclipse.Input
{
    public enum GameInputKey
    {
        None = 0,
        Return = 13,
        Space = 32,
        A = 97,
        D = 100,
        E = 101,
        F = 102,
        J = 106,
        Q = 113,
        R = 114,
        S = 115,
        W = 119,
        UpArrow = 273,
        RightArrow = 275,
        LeftArrow = 276,
        DownArrow = 274,
        RightShift = 303,
        LeftShift = 304,
        LeftControl = 306,
        Tab = 9
    }

    public static class GameInput
    {
        private static readonly Dictionary<GameInputKey, InputAction> KeyActions = new Dictionary<GameInputKey, InputAction>();
        private static InputAction leftMouseAction;
        private static InputAction rightMouseAction;
        private static InputAction pointerPositionAction;

        public static bool IsHeld(GameInputKey key)
        {
            InputAction action = GetOrCreateKeyAction(key);
            return action != null && action.IsPressed();
        }

        public static bool WasPressedThisFrame(GameInputKey key)
        {
            InputAction action = GetOrCreateKeyAction(key);
            return action != null && action.WasPressedThisFrame();
        }

        public static bool IsLeftMouseHeld()
        {
            leftMouseAction = GetOrCreateAction(leftMouseAction, "ProjectEclipse.LeftMouse", "<Mouse>/leftButton");
            return leftMouseAction != null && leftMouseAction.IsPressed();
        }

        public static bool IsRightMouseHeld()
        {
            rightMouseAction = GetOrCreateAction(rightMouseAction, "ProjectEclipse.RightMouse", "<Mouse>/rightButton");
            return rightMouseAction != null && rightMouseAction.IsPressed();
        }

        public static Vector2 PointerScreenPosition
        {
            get
            {
                pointerPositionAction = GetOrCreateAction(pointerPositionAction, "ProjectEclipse.PointerPosition", "<Pointer>/position", InputActionType.Value);
                return pointerPositionAction != null ? pointerPositionAction.ReadValue<Vector2>() : Vector2.zero;
            }
        }

        private static InputAction GetOrCreateKeyAction(GameInputKey key)
        {
            if (key == GameInputKey.None)
            {
                return null;
            }

            InputAction action;
            if (KeyActions.TryGetValue(key, out action))
            {
                return action;
            }

            string binding = ToBindingPath(key);
            if (string.IsNullOrEmpty(binding))
            {
                return null;
            }

            action = new InputAction("ProjectEclipse." + key, InputActionType.Button, binding);
            action.Enable();
            KeyActions[key] = action;
            return action;
        }

        private static InputAction GetOrCreateAction(InputAction action, string actionName, string binding, InputActionType type = InputActionType.Button)
        {
            if (action != null)
            {
                return action;
            }

            InputAction created = new InputAction(actionName, type, binding);
            created.Enable();
            return created;
        }

        private static string ToBindingPath(GameInputKey key)
        {
            switch (key)
            {
                case GameInputKey.Return:
                    return "<Keyboard>/enter";
                case GameInputKey.Space:
                    return "<Keyboard>/space";
                case GameInputKey.A:
                    return "<Keyboard>/a";
                case GameInputKey.D:
                    return "<Keyboard>/d";
                case GameInputKey.E:
                    return "<Keyboard>/e";
                case GameInputKey.F:
                    return "<Keyboard>/f";
                case GameInputKey.J:
                    return "<Keyboard>/j";
                case GameInputKey.Q:
                    return "<Keyboard>/q";
                case GameInputKey.R:
                    return "<Keyboard>/r";
                case GameInputKey.S:
                    return "<Keyboard>/s";
                case GameInputKey.W:
                    return "<Keyboard>/w";
                case GameInputKey.UpArrow:
                    return "<Keyboard>/upArrow";
                case GameInputKey.RightArrow:
                    return "<Keyboard>/rightArrow";
                case GameInputKey.LeftArrow:
                    return "<Keyboard>/leftArrow";
                case GameInputKey.DownArrow:
                    return "<Keyboard>/downArrow";
                case GameInputKey.RightShift:
                    return "<Keyboard>/rightShift";
                case GameInputKey.LeftShift:
                    return "<Keyboard>/leftShift";
                case GameInputKey.LeftControl:
                    return "<Keyboard>/leftCtrl";
                case GameInputKey.Tab:
                    return "<Keyboard>/tab";
                default:
                    return string.Empty;
            }
        }
    }
}
