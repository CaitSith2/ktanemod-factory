﻿using Assets.Scripts.Pacing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FactoryAssembly
{
    internal class FiniteSequenceMode : FactoryGameMode
    {
        internal override float RemainingTime
        {
            get
            {
                if (_currentBomb != null)
                {
                    return _currentBomb.Timer.TimeRemaining;
                }

                return float.PositiveInfinity;
            }
        }

        protected Queue<FactoryBomb> _bombQueue = new Queue<FactoryBomb>();

        protected FactoryBomb _currentBomb = null;
        protected FactoryBomb _oldBomb = null;

        private Selectable _roomSelectable = null;
        private Selectable[] _roomChildren = null;
        private int _bombSelectableIndex = 0;

        internal override void Update()
        {
            if (_currentBomb != null && _currentBomb.IsReadyToShip)
            {
                SetSelectableBomb(null);
                _currentBomb.DisableBomb();
                GetNextBomb();
            }
        }

        internal override void Setup(FactoryRoom room)
        {
            base.Setup(room);

            InvoiceData.Start();

            foreach (FactoryBomb bomb in Bombs)
            {
                bomb.SetupStartPosition(Room.InitialSpawn);
                _bombQueue.Enqueue(bomb);
            }

            _roomSelectable = Room.RoomSelectable;

            Room.StartCoroutine(StartGameplay());
            Room.StartCoroutine(AdjustSelectableGrid());
        }

        /// <summary>
        /// Starts the 'current' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        internal override void OnStartBomb()
        {
            if (_currentBomb != null)
            {
                SetSelectableBomb(_currentBomb);
                _currentBomb.StartBomb();
            }
        }

        /// <summary>
        /// Ends the 'old' bomb.
        /// </summary>
        /// <remarks>Invoked by animation event.</remarks>
        internal override void OnEndBomb()
        {
            _oldBomb?.EndBomb();
            _oldBomb = null;
        }

        /// <summary>
        /// Requests the next bomb to show up.
        /// </summary>
        protected virtual void GetNextBomb()
        {
            _oldBomb = _currentBomb;

            if (_bombQueue.Count != 0)
            {
                _currentBomb = _bombQueue.Dequeue();
                _currentBomb.AttachToConveyor(Room.GetNextConveyorNode());
            }
            else
            {
                _currentBomb = null;
            }

            if (_currentBomb != null)
            {
                //For any adaptations, let them know a bomb has started
                foreach (FactoryGameModeAdaptation adaptation in Adaptations)
                {
                    adaptation.OnStartBomb(_oldBomb, _currentBomb);
                }

                _currentBomb.BombConfigured();
            }

            Room.GetNextBomb();
        }

        /// <summary>
        /// Coroutine to replace the standard GameplayState start round coroutine.
        /// </summary>
        private IEnumerator StartGameplay()
        {
            GameplayState gameplayState = SceneManager.Instance.GameplayState;
            gameplayState.StopAllCoroutines();

            yield return new WaitForSeconds(2.0f);

            gameplayState.Room.ActivateCeilingLights();
            GameplayState.OnLightsOnEvent?.Invoke();

            if ((KTInputManager.Instance.GetCurrentSelectable() == null || KTInputManager.Instance.GetCurrentSelectable().Parent == KTInputManager.Instance.RootSelectable) && !KTInputManager.Instance.IsMotionControlMode())
            {
                KTInputManager.Instance.SelectRootDefault();
            }

            if (KTInputManager.Instance.IsMotionControlMode())
            {
                KTInputManager.Instance.SelectableManager.EnableMotionControls();
            }

            yield return new WaitForSeconds(1.0f);

            GetNextBomb();

            PropertyInfo roundStartedProperty = typeof(GameplayState).GetProperty("RoundStarted", BindingFlags.Instance | BindingFlags.Public);
            roundStartedProperty.SetValue(gameplayState, true, null);

            FieldInfo paceMakerField = typeof(GameplayState).GetField("paceMaker", BindingFlags.Instance | BindingFlags.NonPublic);
            ((PaceMaker)paceMakerField.GetValue(gameplayState)).StartRound(gameplayState.Mission);
        }

        /// <summary>
        /// Coroutine to adjust the selectable grid, which is manipulated by Multiple Bombs initially to normally accomodate multiple bombs in the selectable grid.
        /// </summary>
        private IEnumerator AdjustSelectableGrid()
        {
            _roomChildren = new Selectable[_roomSelectable.Children.Length];
            Array.Copy(_roomSelectable.Children, _roomChildren, _roomChildren.Length);

            int roomChildRowLength = _roomSelectable.ChildRowLength;
            int roomDefaultSelectableIndex = _roomSelectable.DefaultSelectableIndex;

            _bombSelectableIndex = Array.FindIndex(_roomChildren, (x) => x != null && x.GetComponent<Bomb>() != null);

            yield return null;
            yield return null;

            _roomSelectable.Children = _roomChildren;
            _roomSelectable.ChildRowLength = roomChildRowLength;
            _roomSelectable.DefaultSelectableIndex = roomDefaultSelectableIndex;

            SetSelectableBomb(null);
        }

        private void SetSelectableBomb(FactoryBomb bomb)
        {
            KTInputManager.Instance.ClearSelection();

            Selectable selectable = bomb?.Selectable;

            _roomChildren[_bombSelectableIndex] = selectable;

            KTInputManager.Instance.RootSelectable = _roomSelectable;
            KTInputManager.Instance.SelectableManager.ConfigureSelectableAreas(KTInputManager.Instance.RootSelectable);
            KTInputManager.Instance.SelectRootDefault();

            _roomSelectable.Init();
        }
    }
}
