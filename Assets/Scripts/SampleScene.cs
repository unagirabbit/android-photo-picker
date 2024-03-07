using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SampleScene : MonoBehaviour
{
    [SerializeField]
    private Button saveBtn;
    [SerializeField]
    private Button selectBtn;
    [SerializeField]
    private RawImage image;
    // Start is called before the first frame update
    void Start()
    {
        saveBtn.onClick.AddListener(() => {
        });
        selectBtn.onClick.AddListener(() => {
            image.texture = null;
        });
    }
}
