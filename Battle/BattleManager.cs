using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;

namespace MatchThree;

public class BattleManager(Game game, GameBoard board, Battlefield field) : GameComponent(game)
{
    private List<Monster> _monsters;
    private Player _player;
    private Monster _playerTarget;

    private Monster CurrentTarget
    {
        get
        {
            if (_playerTarget == null)
            {
                return _monsters.First();
            }

            return _playerTarget;
        }
    }
    private GameBoard _board = board;
    private Battlefield _field = field;

    public void InitializeBattle(List<Monster> monsters)
    {
        _monsters = monsters;
        foreach (var mon in monsters)
        {
            _field.AddMonster(mon);
        }
    }

    public void BeginBattle()
    {
        _board.MatchActivated += OnMatchActivated;
        _field.MonsterTouched += OnMonsterTouched;
        _field.BattlefieldTouched += OnBattlefieldTouched;
    }

    public void OnMonsterTouched(BattlefieldEntity monster)
    {
        if (_playerTarget == null)
        {
            _playerTarget = monster.Monster;
            monster.Monster.Targeted = true;
            return;
        }

        if (monster.Monster == _playerTarget)
        {
            _playerTarget.Targeted = false;
            _playerTarget = null;
            return;
        }
        
        _playerTarget = monster.Monster;
        _playerTarget.Targeted = true;
    }

    public void OnBattlefieldTouched()
    {
        if (_playerTarget == null) return;
        _playerTarget.Targeted = false;
        _playerTarget = null;
    }
    
    public void OnMatchActivated(Bag<GamePiece> set)
    {
        var damage = set.Sum(piece => piece.Value);
        CurrentTarget.CurrentHealth -= damage;
        foreach (var piece in set)
        {
            Game.Components.Remove(piece);
            _board.GamePieces[piece.GridPosition.X, piece.GridPosition.Y] = null;
        }
    }
  
    
}

public delegate void MatchEventHandler(MatchEventArgs args);

public class MatchEventArgs(Bag<GamePiece> pieces) : EventArgs
{
    public Bag<GamePiece> Pieces = pieces;
}