﻿using UnityEngine;

namespace FactoryAssembly
{
    public class FactoryService : MonoBehaviour
    {
        private KMGameInfo _gameInfo = null;

        private void Awake()
        {
            _gameInfo = GetComponent<KMGameInfo>();
            _gameInfo.OnStateChange += OnStateChange;
        }

        private void OnDestroy()
        {
            _gameInfo.OnStateChange -= OnStateChange;
        }

        private void OnStateChange(KMGameInfo.State state)
        {
            switch (state)
            {
                case KMGameInfo.State.Setup:
                    FactoryGameModePicker.UpdateCompatibleMissions();
                    break;

                case KMGameInfo.State.Gameplay:
                    //If going into a custom mission, we must ensure the custom component pool is gone before we start, otherwise problems happen!
                    if (GameplayState.MissionToLoad == ModMission.CUSTOM_MISSION_ID)
                    {
                        FactoryGameModePicker.UpdateMission(GameplayState.CustomMission, true);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}
