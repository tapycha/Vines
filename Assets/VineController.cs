using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineController : MonoBehaviour
{
    [Serializable]
    public class Vine
    {
        public IvyGenerator generator;
        public float startAwait;
        public float growTime;
    }

    public List<Vine> vines;
    void Start()
    {
        foreach (var vine in vines)
        {
            StartCoroutine(Perform(vine));
        }
    }

    IEnumerator Perform(Vine vine)
    {
        yield return new WaitForSeconds(vine.startAwait);
        var elapseTime = 0f;
        var material = vine.generator.GetComponent<MeshRenderer>().material;
        while (elapseTime<vine.growTime)
        {
            material.SetFloat("Grow",elapseTime/vine.growTime);
            elapseTime += Time.deltaTime;
            yield return null;
        }
    }
}
