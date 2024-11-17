using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;

namespace MatchThree;

public class BattleManager(Game game, GameBoard board, Battlefield field) : GameComponent(game)
{
    private List<Monster> _monsters;
    private GameBoard _board = board;
    private Battlefield _field = field;

    public event Action MonsterChanged;

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
    }

    public void OnMatchActivated(MatchEventArgs args)
    {
        var damage = args.Pieces.Sum(piece => piece.Value);
        _monsters.First().CurrentHitPoints -= damage;
        _field.UpdateMonster(_monsters.First());
        foreach (var piece in args.Pieces)
        {
            Game.Components.Remove(piece);
            _board.GamePieces[piece.GridPosition.X, piece.GridPosition.Y] = null;
        }
    }

    public void OnMonsterChanged(Monster monster)
    {
        _field.UpdateMonster(monster);
    }
    
    
}

public delegate void MatchEventHandler(MatchEventArgs args);

public class MatchEventArgs(Bag<GamePiece> pieces) : EventArgs
{
    public Bag<GamePiece> Pieces = pieces;
}