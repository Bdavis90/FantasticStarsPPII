using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager
{
    // Start is called before the first frame update
    public GameObject characterObject;
    public CharacterSheet sheet;

    //Constructor
    public CharacterManager(GameObject _gameObject, CharacterSheet _sheet)
    {
        characterObject = _gameObject;
        sheet = _sheet;

    }

    public GameObject GetGameObject()
    {
        return characterObject;
    }

    public CharacterSheet GetEntity()
    {
        return sheet;
    }
}
