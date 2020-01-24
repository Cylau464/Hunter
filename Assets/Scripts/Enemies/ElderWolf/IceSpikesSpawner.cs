using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class IceSpikesSpawner : MonoBehaviour
{
    public EnemySpell spell;
    public PlayerMovement player;
    [SerializeField] int spikesCount = 3;
    [SerializeField] float minDistantion = 4f;
    [SerializeField] float maxDistantion = 8f;
    
    private void Start()
    {
        StartCoroutine(SpawnIceSpikes());
    }

    IEnumerator SpawnIceSpikes()
    {
        GameObject _inst;
        GameObject _spikeRes = Resources.Load<GameObject>("Enemies/Elder Wolf/Ice Spike");
        List<GameObject> _spike = new List<GameObject>(spikesCount);
        Debug.Log(_spike.Count);
        BoxCollider2D _spikeCol = null;
        float _posX = 0f;
        int _dir = Random.Range(0, 2) * 2 - 1;

        for (int i = 0; i < spikesCount; i++)
        {
            if (_spike.Count == 0)
                _posX = player.transform.position.x;
            else
            {
                _posX = Random.Range(_spike[0].transform.position.x + _dir * (minDistantion + _spikeCol.size.x / 2), _spike[0].transform.position.x + _dir * (maxDistantion + _spikeCol.size.x / 2));
                _dir = -_dir;
            }

            _spike.Add(_inst = Instantiate(_spikeRes, new Vector3(_posX, transform.position.y, 0f), Quaternion.identity));
            _inst.GetComponent<IceSpike>().spell = spell;
            _spikeCol = _spikeCol ?? _spike[i].GetComponent<BoxCollider2D>();

            yield return new WaitForSeconds(spell.periodicityDamage);
        }

        Destroy(gameObject);
    }
}