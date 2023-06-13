using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestButtonScript : MonoBehaviour
{
    public Unit unit;

    // This method will be called in the start of your game
    void Start()
    {
        // Get the Button component on this gameObject and add a listener to it
        Button buttonComponent = GetComponent<Button>();
        buttonComponent.onClick.AddListener(unit.StartMove);
    }
}
