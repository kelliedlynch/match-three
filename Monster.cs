using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MatchThree;

public class Monster
{
    private int _maxHealth;
    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            if (_maxHealth == value) return;
            _maxHealth = value;
            HealthChanged?.Invoke();
        }
    }

    private int _currentHealth;

    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            if (_currentHealth == value) return;
            _currentHealth = value;
            HealthChanged?.Invoke();
        }
    }
    public event Action HealthChanged;
    
    public int Atk;
    public int Def;
    public string Name;
    public string FileName;

    public bool Targeted = false;

}
