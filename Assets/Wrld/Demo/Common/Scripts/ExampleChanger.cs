using Wrld;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleChanger : MonoBehaviour
{
    [System.Serializable]
    struct Example
    {
        public Example(string name, GameObject root)
        {
            Name = name;
            Root = root;
        }

        public string Name;

        public GameObject Root;
    }

    [SerializeField]
    private Dropdown m_dropdown = null;

    [SerializeField]
    private Example[] ExampleList = null;

    int m_currentActiveExample;

    void Start()
    {
        List<Dropdown.OptionData> DropData = new List<Dropdown.OptionData>();
        DropData.Add(new Dropdown.OptionData("Select an example"));
        
        foreach (Example example in ExampleList)
        {
            example.Root.SetActive(false);
            DropData.Add(new Dropdown.OptionData(example.Name));
        }

        m_dropdown.AddOptions(DropData);
        m_currentActiveExample = -1;
    }

    public void Change()
    {
        if (m_dropdown.value != 0)
        {
            int chosenExample = m_dropdown.value - 1;

            if (m_currentActiveExample != chosenExample)
            {
                if (m_currentActiveExample != -1)
                {
                    var root = ExampleList[m_currentActiveExample].Root;
                    root.SetActive(false);

                    var wrldMap = root.GetComponentInChildren<WrldMap>();
                    
                    if (wrldMap != null)
                    {
                        wrldMap.OnDestroy();
                    }
                }

                ExampleList[chosenExample].Root.SetActive(true);
                m_currentActiveExample = chosenExample;
            }
        }
    }

    public void OnApplicationQuit()
    {
        if (m_currentActiveExample != -1)
        {
            ExampleList[m_currentActiveExample].Root.SetActive(false);
        }
    }

}
