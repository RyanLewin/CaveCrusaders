using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapMenuItemScript : MonoBehaviour
{
    [SerializeField]
    Text _nameText;
    [SerializeField]
    Button DeleteButton;
    MapSaveController.MapRefrence Map;
    public void SetInfo(MapSaveController.MapRefrence level)
    {
        _nameText.text = level.name;
        Map = level;
        if (DeleteButton)
        {
            DeleteButton.interactable = !level.IsResource;
        }
    }

    public void LoadLevel()
    {
        LevelEditorUIScript.instance.LoadMap(_nameText.text,Map.IsResource);
    }

    public void DeleteLevel()
    {
        LevelEditorUIScript.instance.DeleteMap(_nameText.text);
    }
}
