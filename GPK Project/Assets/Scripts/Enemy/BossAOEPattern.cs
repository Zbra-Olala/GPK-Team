﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAOEPattern : MonoBehaviour
{
    public bool isTesting;
    [Header("Aoe settings")]
    public List<Aoe> patternAoes;
    [Header("Prefabs")]
    public GameObject warningZonePrefab;
    public GameObject aoeFx;
    public AudioClip aoeSound;
    public AudioClip warningSound;
    public float warningSoundOffset;

    private AudioSource source;

    void Start()
    {
        source = GetComponent<AudioSource>();
        if (!isTesting)
            StartCoroutine(LaunchAoepattern());
    }

    private void Update()
    {
        if(isTesting && Input.GetKeyDown(KeyCode.A))
        {
            StartCoroutine(LaunchAoepattern());
        }
    }

    private IEnumerator LaunchAoepattern()
    {
        for(int i = 0; i < patternAoes.Count; i++)
        {
            StartCoroutine(SpawnAOE(patternAoes[i].Info()));

            while (patternAoes[i].beatTimeBeforeNextAOE == 0)
            {
                i++;
                StartCoroutine(SpawnAOE(patternAoes[i].Info()));
            }

            yield return new WaitForSeconds(patternAoes[i].beatTimeBeforeNextAOE * BeatManager.Instance.BeatTime);

        }

        if(!isTesting)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator SpawnAOE(Aoe aoe)
    {
        GameObject warningZone = Instantiate(warningZonePrefab, aoe.position, Quaternion.identity);
        warningZone.transform.localScale = Vector2.one * aoe.radius * 2;
        //Invoke("PlayWarningSound", aoe.warningBeatTime * BeatManager.Instance.BeatTime - warningSoundOffset);

        yield return new WaitForSeconds(aoe.warningBeatTime * BeatManager.Instance.BeatTime);

        Destroy(warningZone);

        GameObject fx = Instantiate(aoeFx, aoe.position, Quaternion.identity);
        fx.transform.localScale = Vector2.one * aoe.radius * 2;
        source.PlayOneShot(aoeSound);
        if (Physics2D.OverlapCircle(aoe.position, aoe.radius, LayerMask.GetMask("Player")))
        {
            GameManager.Instance.playerManager.TakeDamage(aoe.damage);
        }
    }

    private void PlayWarningSound()
    {
        source.PlayOneShot(warningSound);
    }

    /// <summary>
    /// Retourne le temps de ce pattern en secondes.
    /// </summary>
    /// <returns></returns>
    public float PatternTime()
    {
        float patternTime = 0;

        foreach (Aoe aoe in patternAoes)
        {
            patternTime += aoe.beatTimeBeforeNextAOE * BeatManager.Instance.BeatTime;
        }
        return patternTime;
    }

    [System.Serializable]
    public class Aoe
    {
        [SerializeField] private Transform spawnPosition = null;
        [HideInInspector] public Vector2 position;
        [Range(0f, 10f)] public float beatTimeBeforeNextAOE;
        [Range(1f, 20f)] public int damage;
        [Range(0.5f, 8f)] public float radius;
        [Range(1f, 5f)] public float warningBeatTime;

        public Aoe Info()
        {
            Aoe aoe = this;
            aoe.position = spawnPosition.position;
            return aoe;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach(Aoe aoe in patternAoes)
        {
            Aoe aoeInfo = aoe.Info();
            Gizmos.DrawWireSphere(aoeInfo.position, aoeInfo.radius);
        }
    }
}
