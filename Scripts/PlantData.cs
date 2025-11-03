using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TriInspector;

[CreateAssetMenu]
public class PlantDataSO : ScriptableObject
{
    [ListDrawerSettings(Draggable = true,
        HideAddButton = true,
        HideRemoveButton = true,
        AlwaysExpanded = false, ShowElementLabels = true)]
    public List<PlantData> plantDatas = new List<PlantData>();

    public void AddNewPlant()
    {
        PlantData newPlant = new PlantData();
        if (plantDatas.Count == 0) newPlant.ID = 1;
        else newPlant.ID = plantDatas.Max(p => p.ID) + 1;
        plantDatas.Add(newPlant);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    public void RemovePlant(PlantData plant)
    {
        plantDatas.Remove(plant);
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    [Button(ButtonSizes.Large, "Add New Plant")]
    private void AddPlantButton()
    {
        AddNewPlant(); // call your existing method }
    }

    [Serializable]
    public class PlantData
    {
        [SerializeField] public string name = "New Plant";

        public PlantData()
        {
            name = "New Plant";
            ID = 1;
            NeedTemp = true;
            FixedAmount = true;
            IdelTemp = 25f;
            PenaltyRange = 10f;
            PenaltyRangeCap = 10f;
            PenaltyMaxDamage = .5f;
            Amount = 1;
            MinAmount = 1;
            MaxAmount = 2;
            DrainWater = 1;
            PlantingTime = 1;
            HarvestTime = 1;
        }

        [SerializeField] public int ID;
        [SerializeField] public bool BlockPath;
        public GameObject Prefab;
        public Item yield;
        public float PlantingTime;
        [SerializeField] public float[] ProgressTarget;

        [Title("Growth")] [SerializeField] public bool DifferentWater; //using different water in each stage

        [HideIf(nameof(DifferentWater), true)] [SerializeField]
        public float DrainWater;

        [HideIf(nameof(DifferentWater), false)] [SerializeField]
        public float[] DrainWaters;

        [SerializeField] public bool NeedTemp;

        [HideIf(nameof(NeedTemp), false)] [SerializeField]
        public float IdelTemp;

        [HideIf(nameof(NeedTemp), false)] [SerializeField]
        public float PenaltyRange; //+- ideal temp range before decrease growth rate

        [HideIf(nameof(NeedTemp), false)] [SerializeField]
        public float PenaltyRangeCap; //penalty range

        [HideIf(nameof(NeedTemp), false)] [SerializeField]
        public float PenaltyMaxDamage; //penalty growth speed loss

        [Title("Harvest")]
        public float HarvestTime;
        public int BaseSellCost;
        [SerializeField] public bool Repeatable;

        [HideIf(nameof(Repeatable), false)] [SerializeField]
        public int BackToStage;

        [SerializeField] public bool FixedAmount;

        [HideIf(nameof(FixedAmount), false)] [SerializeField]
        public int Amount;

        [HideIf(nameof(FixedAmount), true)] [SerializeField]
        public int MinAmount;

        [HideIf(nameof(FixedAmount), true)] [SerializeField]
        public int MaxAmount;

        [Button("Delete This Plant")]
        private void DeleteMe()
        {
#if UNITY_EDITOR
            // Get parent ScriptableObject in editor context
            var so = UnityEditor.AssetDatabase.LoadAssetAtPath<PlantDataSO>(
                UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject));

            if (so != null)
            {
                so.RemovePlant(this);
            }
#endif
        }
    }
}