﻿using System.Collections;
using UnityEngine;

namespace FactoryAssembly
{
    public class FactoryService : MonoBehaviour
    {
        private KMGameInfo _gameInfo = null;
        private APIProperties _properties = null;
        private ResultBinderConverter _binderConverter = null;

        private bool _fromSetupRoom = false;

        private void Awake()
        {
            _gameInfo = GetComponent<KMGameInfo>();
            _gameInfo.OnStateChange += OnStateChange;

            _properties = GetComponentInChildren<APIProperties>();
            _properties.Add("SupportedModes", () => FactoryGameModePicker.GetModeNames, null);
            _properties.Add("EnabledModes", () => FactoryGameModePicker.GetModeSupport, null);

            _binderConverter = GetComponentInChildren<ResultBinderConverter>(true);
        }

        private void OnEnable()
        {
            InvoiceData.Enabled = false;
            InvoiceData.ClearData();
        }

        private void OnDisable()
        {
            InvoiceData.Enabled = false;
            InvoiceData.ClearData();
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
                    Logging.Log("State Change: Setup");

                    InvoiceData.Enabled = false;
                    InvoiceData.ClearData();

                    MultipleBombsInterface.RediscoverMultipleBombs();
                    FactoryGameModePicker.UpdateCompatibleMissions();
                    _fromSetupRoom = true;

                    _binderConverter.Revert();
                    break;

                case KMGameInfo.State.Gameplay:
                    Logging.Log("State Change: Gameplay");

                    if (GameplayState.MissionToLoad == ModMission.CUSTOM_MISSION_ID && _fromSetupRoom)
                    {
                        FactoryGameModePicker.UpdateMission(GameplayState.CustomMission, true, false, true);

                        StartCoroutine(FixCustomMission());
                    }
                    _fromSetupRoom = false;

                    _binderConverter.Revert();
                    break;

                case KMGameInfo.State.PostGame:
                    Logging.Log("Stage Change: PostGame");

                    if (InvoiceData.Enabled)
                    {
                        _binderConverter.Convert();
                    }
                    break;

                default:
                    break;
            }
        }

        private IEnumerator FixCustomMission()
        {
            for (int frameCount = 0; frameCount < 4; ++frameCount)
            {
                yield return null;
            }

            string missionID = SceneManager.Instance.GameplayState.Mission.ID;

            FactoryGameModePicker.GameMode? gameMode = FactoryGameModePicker.GetGameModeForMission(missionID);
            if (gameMode.HasValue && gameMode.Value.GetGameModeType() == typeof(InfiniteSequenceMode))
            {
                SceneManager.Instance.GameplayState.Mission.name = "__fixedCustomMission";
                FactoryGameModePicker.UpdateMission(SceneManager.Instance.GameplayState.Mission, true, true, true);
                SceneManager.Instance.GameplayState.Mission.name = missionID;
            }
        }
    }
}
