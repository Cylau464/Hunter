using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Structures;
using Enums;

public class PlayerEffectsController : MonoBehaviour
{
    [SerializeField] List<RectTransform> cellsList = null;
    [SerializeField] GameObject cellPrefab = null;
    [SerializeField] RectTransform transformParent = null;
    [SerializeField] EffectsIconsDictionary effectIcons = null;
    [SerializeField] EffectsDictionary effects = new EffectsDictionary()
    {
        { Effects.Freeze, new Effect(Effects.Freeze, 5, 1f, .1f) },
        { Effects.Burning, new Effect(Effects.Burning, 5, 2f, 2f, .5f) },
        { Effects.Bleeding, new Effect(Effects.Bleeding, 5, 3f, 2.5f, .5f) },
        { Effects.Poison, new Effect(Effects.Poison, 5, 3f, 2.5f, 1f) },
    };
    [SerializeField] EffectsGameObjectDictionary particlePrefabs = new EffectsGameObjectDictionary()
    {
        { Effects.Freeze, null },
        { Effects.Burning, null },
        { Effects.Bleeding, null },
        { Effects.Poison, null }
    };
    Dictionary<Effects, GameObject> activeParicles = new Dictionary<Effects, GameObject>(5)
    {
        { Effects.Freeze, null },
        { Effects.Burning, null },
        { Effects.Bleeding, null },
        { Effects.Poison, null },
        { Effects.Root, null }
    };
    List<Vector3> cellsCoordinate = new List<Vector3>();
    [SerializeField] List<DebuffEffect> cellsScript = new List<DebuffEffect>();
    Dictionary<Effects, int> debuffCellIndex = new Dictionary<Effects, int>(5)
    {
        { Effects.Freeze, 100 },
        { Effects.Burning, 100 },
        { Effects.Bleeding, 100 },
        { Effects.Poison, 100 },
        { Effects.Root, 100 },
    };
    PlayerAttributes playerAttributes;

    // Start is called before the first frame update
    void Start()
    {
        playerAttributes = GetComponent<PlayerAttributes>();

        cellsList.ForEach(x => {
            cellsCoordinate.Add(x.position);
            Destroy(x.gameObject);
        });
        cellsList = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Effect _effect = new Effect();
            int _rand = Random.Range(0, 4);

            switch (_rand)
            {
                case 0:
                    _effect = new Effect(Effects.Bleeding, 1);
                    break;
                case 1:
                    _effect = new Effect(Effects.Burning, 1);
                    break;
                case 2:
                    _effect = new Effect(Effects.Freeze, 1);
                    break;
                case 3:
                    _effect = new Effect(Effects.Poison, 1);
                    break;
            }

            GetEffect(_effect);
        }
    }

    public void GetEffect(Effect effect)
    {
        if(debuffCellIndex[effect.effect] == 100)
        {
            cellsScript.Add(Instantiate(cellPrefab, transformParent).GetComponent<DebuffEffect>());
            debuffCellIndex[effect.effect] = cellsScript.Count - 1;
            cellsScript[debuffCellIndex[effect.effect]].GetEffect(cellsCoordinate[debuffCellIndex[effect.effect]], this, debuffCellIndex[effect.effect], effect.stacksCount, effects[effect.effect], effectIcons[effect.effect], playerAttributes, effects[effect.effect].effectPeriod);

            //Instantiate particle prefab
            if(particlePrefabs[effect.effect] != null)
                activeParicles[effect.effect] = Instantiate(particlePrefabs[effect.effect], transform);
        }
        else
        {
            cellsScript[debuffCellIndex[effect.effect]].GetEffect(effect.stacksCount);
        }
    }

    public void RemoveCell(int cellIndex, Effects effect)
    {
        cellsScript.RemoveAt(cellIndex);
        debuffCellIndex[effect] = 100;
        Destroy(activeParicles[effect]);
        activeParicles[effect] = null;

        //Update position on next cells
        for (int i = cellIndex; i <= cellsScript.Count - 1; i++)
        {
            cellsScript[i].SetPosition(cellsCoordinate[i], i);
        }
    }
}
