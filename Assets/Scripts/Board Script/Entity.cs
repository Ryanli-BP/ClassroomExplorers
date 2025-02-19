using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;

public abstract class Entity : MonoBehaviourPun
{
    public int Health { get; protected set; }
    public Status Status { get; protected set; }

    public abstract void LoseHealth(int amount);
    public abstract void Dies();
    public abstract void TeleportTo(Vector3 position, Tile destinationTile);

    public abstract void AddBuff(BuffType type, int value, int duration);
    public abstract void UpdateBuffDurations();
}

[System.Serializable]
public class Buff
{
    public BuffType Type { get; private set; }
    public int Value { get; private set; }
    public int RoundsRemaining { get; private set; }

    public Buff(BuffType type, int value, int duration)
    {
        Type = type;
        Value = value;
        RoundsRemaining = duration;
    }

    public void DecrementDuration()
    {
        RoundsRemaining--;
        Debug.Log($"Buff {Type} has {RoundsRemaining} rounds duration.");
    }
}

public enum BuffType
{
    AttackUp,
    DefenseUp,
    EvadeUp,
    ExtraDice,
    DoublePoints, // Only used by Players
    TriplePoints  // Only used by Players
}

[System.Serializable]
public class EntityBuffs
{
    protected List<Buff> activeBuffs = new List<Buff>();

    public int AttackBonus => activeBuffs.Where(b => b.Type == BuffType.AttackUp).Sum(b => b.Value);
    public int DefenseBonus => activeBuffs.Where(b => b.Type == BuffType.DefenseUp).Sum(b => b.Value);
    public int EvadeBonus => activeBuffs.Where(b => b.Type == BuffType.EvadeUp).Sum(b => b.Value);
    public int ExtraDiceBonus => activeBuffs.Where(b => b.Type == BuffType.ExtraDice).Sum(b => b.Value);

    public virtual void AddBuff(BuffType type, int value, int duration)
    {
        activeBuffs.Add(new Buff(type, value, duration));
    }

    public void UpdateBuffDurations()
    {
        foreach (var buff in activeBuffs.ToList())
        {
            buff.DecrementDuration();
            if (buff.RoundsRemaining <= 0)
            {
                activeBuffs.Remove(buff);
                Debug.Log($"Buff {buff.Type} has expired.");
            }
        }
    }

    public void Reset()
    {
        activeBuffs.Clear();
    }

    public List<Buff> GetActiveBuffs() => activeBuffs;
}