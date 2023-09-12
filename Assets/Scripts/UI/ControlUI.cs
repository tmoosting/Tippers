using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlUI : MonoBehaviour
{

    public Color textFieldColor;

    public bool showWindow = true;
    private UIDocument uiDocument;
    private VisualElement window;
    private VisualElement content;
     
    private BlueTextField trafficRateTextField;
    public Label trafficLabel;
    private Slider speedSlider;
    public Toggle shiftPatternToggle;
    public Toggle shiftSideToggle;
    private Button createPatternButton;
    public Button tipButton;

    private List<Button> loadButtons;

    
    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        window = uiDocument.rootVisualElement.Q("WindowBase");

        BuildWindow();
        RefreshWindow();
    }

    public void ShowWindow(bool show)
    {
        showWindow = show;
        if (showWindow)
            uiDocument.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        else
            uiDocument.rootVisualElement.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

    }
    private void BuildWindow()
    {
        content = new VisualElement();
        content.style.width = Length.Percent(98);
        content.style.height = Length.Percent(98);
        content.style.marginTop = 2f;
        content.style.marginLeft = 3f;
        content.style.marginRight = 5f;

        VisualElement tipRow = new VisualElement();
        tipRow.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        tipRow.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
        
        trafficRateTextField = new BlueTextField(this);
        trafficRateTextField.label = "Traffic per Minute"; 
        trafficRateTextField.style.marginTop = 4f;
        trafficRateTextField.style.minWidth = 220f;
        trafficRateTextField.RegisterValueChangedCallback(TrafficChange);
        tipRow.Add(trafficRateTextField);

        trafficLabel = new Label();
        trafficLabel.text = "0/" + PixelController.Instance.trafficPerTip;
        trafficLabel.style.marginLeft = 6f;
        tipRow.Add(trafficLabel);

        tipButton = new Button();
        tipButton.text = "TIP";
        tipButton.clicked += ClickTipButton;
        tipButton.style.marginLeft = 6f;
        tipButton.style.width = 80f;
        tipRow.Add(tipButton);
        content.Add(tipRow);

        
        VisualElement shiftRow = new VisualElement();
        shiftRow.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        shiftRow.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
        shiftRow.style.marginTop = 4f;

        shiftPatternToggle = new Toggle();
        shiftPatternToggle.label = "Shift Pattern";
        shiftPatternToggle.RegisterValueChangedCallback(evt => ToggleShiftPattern(evt));
        shiftRow.Add(shiftPatternToggle); 
        
        shiftSideToggle = new Toggle();
        shiftSideToggle.label = "Shift Sideways";
        shiftSideToggle.RegisterValueChangedCallback(evt => ToggleShiftSideways(evt));
        shiftRow.Add(shiftSideToggle);
        
        
        content.Add(shiftRow);

        speedSlider = new Slider();
        speedSlider.label = "1 Tick per Second";
        speedSlider.style.marginTop = 4f; 
        speedSlider.value = 1;
        speedSlider.lowValue = 0.1f;
        speedSlider.highValue = 5f;
        speedSlider.RegisterValueChangedCallback(evt => ChangeSliderValue(evt));
        content.Add(speedSlider);
        
          createPatternButton = new Button();
        createPatternButton.text = "Create Pattern";
        createPatternButton.clicked += ClickStartDiffusion;
        createPatternButton.style.width = 120f;
        createPatternButton.style.marginTop = 10f;
        createPatternButton.style.marginBottom = 10f;
        content.Add(createPatternButton);

        VisualElement saveRow = new VisualElement();
        saveRow.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        saveRow.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
        saveRow.style.marginBottom = 6f;
        VisualElement loadRow = new VisualElement();
        loadRow.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        loadRow.style.justifyContent = new StyleEnum<Justify>(Justify.SpaceBetween);
        loadRow.style.marginBottom = 6f;


        loadButtons = new List<Button>();
        for (int i = 0; i < 5; i++)
        {
           int saveIndex = i;

            Button saveButton = new Button();
            saveButton.text = "Save " + i;
            saveButton.clicked += () => { ClickSaveButton(saveIndex); };
            saveRow.Add(saveButton);
        }

        for (int i = 0; i < 5; i++)
        {
           int   saveIndex = i;
            Button loadButton = new Button();
            loadButton.text = "Load " + i;
            loadButton.clicked += () => { ClickLoadButton(saveIndex); };
            loadRow.Add(loadButton);
            loadButtons.Add(loadButton);
        }
        
        content.Add(saveRow);
        content.Add(loadRow);

        window.Add(content);
    }
 
    public void RefreshWindow()
    {
        foreach (var loadButton in loadButtons)
        {
            loadButton.SetEnabled(PixelController.Instance.savePresence[loadButtons.IndexOf(loadButton)]);
        }
    }
    
    
    private void TrafficChange(ChangeEvent<string> evt)
    {
        if (evt.newValue == "")
        {
            PixelController.Instance.trafficRate = 0;
            return;
        }
        int outValue = 0;
        if (int.TryParse(evt.newValue, out outValue))
            PixelController.Instance.trafficRate = outValue;
        else
            PixelController.Instance.trafficRate = 0; 
    }

    private void ClickTipButton()
    { 
        PixelController.Instance.Tip(); 
    }

    private void ClickLoadButton(int i)
    {
        PixelController.Instance.LoadFile(i); 
    }

    private void ClickSaveButton(int i)
    {
        PixelController.Instance.SaveToFile(i);
    }

    private void ToggleShiftPattern(ChangeEvent<bool> evt)
    {
        if (evt.newValue == true)
            if (PixelController.Instance.currentPattern == 99)
                 PixelController.Instance.MakePatternChange();
        PixelController.Instance.ShiftPattern = evt.newValue; 
        
        createPatternButton.SetEnabled(!evt.newValue);
    }  
    
    private void ToggleShiftSideways(ChangeEvent<bool> evt)
    {
    
        PixelController.Instance.ShiftSideways = evt.newValue;
        
        createPatternButton.SetEnabled(!evt.newValue);
    }

    private void ChangeSliderValue(ChangeEvent<float> evt)
    {
        speedSlider.label = evt.newValue +" Ticks per Second";
        PixelController.Instance.UpdateTicksPerSecond (evt.newValue);

    }

    private void ClickStartDiffusion()
    {
        PatternGenerator.Instance.CreatePattern();
    }
}
