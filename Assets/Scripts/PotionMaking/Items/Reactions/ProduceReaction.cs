using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProduceReaction : AbstractReaction
{
    [SerializeField] private List<TransformationInfo> _infos;
    
    public override void Execute(IReactionPart target)
    {
        foreach (var info in _infos)
        {
            for (var i = 0; i < info.NumberOfItems; i++)
            {
                var item = Object.Instantiate(info.Item);
                item.transform.localScale = GetScaleVector(info.Scale);
                item.transform.position = GetPosition(target);
                item.transform.rotation = GetRotation();
            }
        }
    }

    private Vector3 GetScaleVector(float scale)
    {
        return Vector3.one * scale;
    }

    private Vector3 GetPosition(IReactionPart target)
    {
        return target.Position + (Vector3)Random.insideUnitCircle * target.Scale;
    }
    
    private Quaternion GetRotation()
    {
        var z = Random.Range(0f, 360f);
        return Quaternion.Euler(new Vector3(0f, 0f, z));
    }
    
    [System.Serializable]
    private class TransformationInfo
    {
        public PotItem Item;
        public int NumberOfItems;
        public float Scale = 1f;
    }
}
