using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TableUI : MonoBehaviour
{
    public List<Button> buttons;
    public TMP_Text phaseText;
    public void ChangeButtonsActive(bool enable)
    {
        GetComponent<GraphicRaycaster>().enabled = enable;
    }

    public void AgreePrior()
    {
        G.Main.PhaseController.trigger = true;
    }
    public void RequestBuy()
    {
        if (G.Main.PhaseController.currentPhase == Phase.Action)
        {
            G.Main.PhaseController.StartBuying().Forget();
        }
    }

    public void EndTurn()
    {
        G.Main.PhaseController.StartEndPhase().Forget();
    }

    public void SetPhaseText(string text)
    {
        phaseText.text = text;
    }

    public void AgreeStack()
    {
        G.Main.StackSystem.ResolveTopUnit().Forget();
    }
}
