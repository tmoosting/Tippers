using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlUI : MonoBehaviour
{

    public Color textFieldColor;
    
    
    private UIDocument uiDocument;
    private VisualElement window;
    private VisualElement content;
     
    private BlueTextField trafficRateTextField;
    public Label trafficLabel;
    private Slider speedSlider;
    public Toggle shiftToggle;
    private Button createPatternButton;
    public Button tipButton;

    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        window = uiDocument.rootVisualElement.Q("WindowBase");

        BuildWindow();
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

        shiftToggle = new Toggle();
        shiftToggle.label = "Shift";
        shiftToggle.style.marginTop = 4f;
        shiftToggle.RegisterValueChangedCallback(evt => ToggleShift(evt));
        content.Add(shiftToggle);

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
        }
        
        content.Add(saveRow);
        content.Add(loadRow);

        window.Add(content);
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

    private void ToggleShift(ChangeEvent<bool> evt)
    {
        PixelController.Instance.Shift = evt.newValue;
        
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
