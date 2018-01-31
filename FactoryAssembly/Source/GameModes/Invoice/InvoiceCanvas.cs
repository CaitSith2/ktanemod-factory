﻿using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace FactoryAssembly
{
    public class InvoiceCanvas : MonoBehaviour
    {
        public Text InvoiceNumber;
        public Text MissionItem;
        public Text Quantity;
        public Text Totals;

        public void OnEnable()
        {
            InvoiceNumber.text = $"Invoice #29875-AB";

            StringBuilder missionBuilder = new StringBuilder();

            missionBuilder.Append($"{InvoiceData.MissionName}\n");
            missionBuilder.Append(GetMissionProperty(InvoiceData.GameMode.GetFriendlyName()));
            missionBuilder.Append(GetMissionProperty($"{InvoiceData.InitialModuleCount} {SingularPlural(InvoiceData.InitialModuleCount, "module", "modules")}"));
            missionBuilder.Append(GetMissionProperty(InvoiceData.InitialTime.GetBombTime()));
            missionBuilder.Append(GetMissionProperty($"{InvoiceData.InitialStrikesToLose} {SingularPlural(InvoiceData.InitialStrikesToLose, "strike", "strikes")}"));

            MissionItem.text = missionBuilder.ToString();

            Quantity.text = $"x{InvoiceData.BombCount}";

            Totals.text = $"{InvoiceData.FinalTime.GetBombTime()}\n{InvoiceData.TotalBombRemainingTime.GetBombTime()}\n{InvoiceData.TotalStrikes}";
        }

        private string GetMissionProperty(string property)
        {
            return $"<size=28>  • {property}</size>\n";
        }

        private string SingularPlural(int number, string singular, string plural)
        {
            if (number == 1)
            {
                return singular;
            }

            return plural;
        }
    }
}
