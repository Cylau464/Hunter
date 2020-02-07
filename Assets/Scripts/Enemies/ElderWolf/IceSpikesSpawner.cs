using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Structures;

public class IceSpikesSpawner : MonoBehaviour
{
    public EnemySpell spell;
    public PlayerMovement player;
    [SerializeField] int spikesCount = 4;
    [SerializeField] float minDistantion = 2f;
    [SerializeField] float maxDistantion = 12f;
    
    private void Start()
    {
        StartCoroutine(SpawnIceSpikes());
    }

    IEnumerator SpawnIceSpikes()
    {
        GameObject _inst;
        GameObject _spikeRes = Resources.Load<GameObject>("Enemies/Elder Wolf/Ice Spike");
        List<GameObject> _spikes = new List<GameObject>(spikesCount);
        BoxCollider2D _spikeCol = null;
        Collider2D[] _neighbours;
        float _xPos;
        

        for (int i = 0; i < spikesCount; i++)
        {
            if (_spikes.Count == 0)
                _xPos = player.transform.position.x;
            else
            {
                do
                {
                    _xPos = Random.Range(_spikes[0].transform.position.x - maxDistantion - _spikeCol.size.x / 2f, _spikes[0].transform.position.x + maxDistantion + _spikeCol.size.x / 2f);

                    _neighbours = Physics2D.OverlapCircleAll(new Vector2(_xPos, _spikes[0].transform.position.y), minDistantion + _spikeCol.size.x / 2f, 1 << 15); //15 layer with ice spikes
                }
                while (_neighbours.Length > 0);
            }

            _spikes.Add(_inst = Instantiate(_spikeRes, new Vector3(_xPos, transform.position.y, 0f), Quaternion.identity));
            _inst.GetComponent<IceSpike>().spell = spell;
            _spikeCol = _spikeCol ?? _spikes[i].GetComponent<BoxCollider2D>();

            yield return new WaitForSeconds(spell.periodicityDamage);
        }

        Destroy(gameObject);
    }
}