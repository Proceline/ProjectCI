using System;
using UnityEngine;
using System.Collections.Generic;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Concrete;
using ProjectCI.CoreSystem.Runtime.TacticRpgTool.Unit;
using ProjectCI.Utilities.Runtime.Events;

public class PvDebugHUD : MonoBehaviour
{
    [System.Serializable]
    public struct DebugValuePair
    {
        public string name;
        public string value;
        
        public DebugValuePair(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
    
    [Header("Debug HUD Settings")]
    [SerializeField] private List<DebugValuePair> debugValues = new List<DebugValuePair>();
    [SerializeField] private bool showHUD = true;
    [SerializeField] private int fontSize = 30;
    [SerializeField] private int fieldWidth = 250;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.7f);
    
    private GUIStyle textStyle;
    private GUIStyle backgroundStyle;
    
    #region Outside Delegate Support

    [SerializeField] private PvSoUnitBattleStateEvent onStateEvent;
    [SerializeField] private PvSoUnitSelectEvent onOwnerSelectedEvent;
    [SerializeField] private FeLiteGameRules rules;
    
    private void OnStateChangedResponse(IEventOwner unit, UnitStateEventParam stateParams)
    {
        if (debugValues.Count > 1)
        {
            debugValues[1] = new DebugValuePair("State", rules.CurrentBattleState.ToString());
        }
    }

    private void OnUnitSelectedResponse(IEventOwner unit, UnitSelectEventParam stateParams)
    {
        if (debugValues.Count > 0)
        {
            if (stateParams.Behaviour == UnitSelectBehaviour.Select)
            {
                debugValues[0] = new DebugValuePair("Owner", stateParams.Unit.ID);
            }
            else if (stateParams.Behaviour == UnitSelectBehaviour.Deselect)
            {
                debugValues[0] = new DebugValuePair("Owner", "N/A");
            }
        }
    }

    #endregion
    
    private void Start()
    {
        // Initialize default values if empty
        if (debugValues.Count == 0)
        {
            debugValues.Add(new DebugValuePair("Testing", "N/A"));
        }
        
        onStateEvent.RegisterCallback(OnStateChangedResponse);
        onOwnerSelectedEvent.RegisterCallback(OnUnitSelectedResponse);
    }

    private void OnDestroy()
    {
        onStateEvent.UnregisterCallback(OnStateChangedResponse);
        onOwnerSelectedEvent.UnregisterCallback(OnUnitSelectedResponse);
    }


    private void OnGUI()
    {
        if (!showHUD) return;
        
        InitializeStyles();
        
        // Calculate HUD dimensions
        float padding = 10f;
        float lineHeight = fontSize + 10f;
        float maxWidth = 0f;
        
        // Calculate maximum width needed
        foreach (var pair in debugValues)
        {
            string displayText = $"{pair.name}: {pair.value}";
            Vector2 textSize = textStyle.CalcSize(new GUIContent(displayText));
            maxWidth = Mathf.Max(fieldWidth, textSize.x);
        }
        
        float hudWidth = maxWidth + padding * 2;
        float hudHeight = debugValues.Count * lineHeight + padding * 3;
        
        // Position HUD in top-right corner
        float x = Screen.width - hudWidth - 10f;
        float y = 10f;
        
        // Draw background
        Rect backgroundRect = new Rect(x, y, hudWidth, hudHeight);
        GUI.Box(backgroundRect, "", backgroundStyle);
        
        // Draw debug values
        for (int i = 0; i < debugValues.Count; i++)
        {
            string displayText = $"{debugValues[i].name}: {debugValues[i].value}";
            Rect textRect = new Rect(x + padding, y + padding + i * lineHeight, maxWidth, lineHeight);
            GUI.Label(textRect, displayText, textStyle);
        }
    }
    
    private void InitializeStyles()
    {
        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = fontSize;
            textStyle.normal.textColor = textColor;
            textStyle.alignment = TextAnchor.UpperLeft;
        }
        
        if (backgroundStyle == null)
        {
            backgroundStyle = new GUIStyle(GUI.skin.box);
            backgroundStyle.normal.background = CreateColorTexture(backgroundColor);
        }
    }
    
    private Texture2D CreateColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
    
    // Public methods to update values at runtime
    public void UpdateValue(string name, string value)
    {
        for (int i = 0; i < debugValues.Count; i++)
        {
            if (debugValues[i].name == name)
            {
                var pair = debugValues[i];
                pair.value = value;
                debugValues[i] = pair;
                return;
            }
        }
        
        // If name not found, add new pair
        debugValues.Add(new DebugValuePair(name, value));
    }
    
    public void UpdateValue(string name, float value)
    {
        UpdateValue(name, value.ToString("F2"));
    }
    
    public void UpdateValue(string name, int value)
    {
        UpdateValue(name, value.ToString());
    }
    
    public void UpdateValue(string name, bool value)
    {
        UpdateValue(name, value.ToString());
    }
    
    public void SetHUDVisible(bool visible)
    {
        showHUD = visible;
    }
    
    public void AddDebugValue(string name, string value)
    {
        debugValues.Add(new DebugValuePair(name, value));
    }
    
    public void RemoveDebugValue(string name)
    {
        for (int i = debugValues.Count - 1; i >= 0; i--)
        {
            if (debugValues[i].name == name)
            {
                debugValues.RemoveAt(i);
            }
        }
    }
    
    public void ClearAllValues()
    {
        debugValues.Clear();
    }
}
