#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ProjectUtilities : MonoBehaviour
{
    [MenuItem("Project Tools/Create Materials based on current selected textures")]
    private static void CreateMaterials()
    {
        foreach (Object o in Selection.objects)
        {
            if (o.GetType() != typeof(Texture2D))
            {
                continue;
            }

            Debug.Log("Creating material from: " + o);

            Texture2D selected = o as Texture2D;

            Material material1 = new Material(Shader.Find("Standard (Specular setup)"));
            material1.mainTexture = (Texture)o;

            string savePath = AssetDatabase.GetAssetPath(selected);
            savePath = savePath.Substring(0, savePath.LastIndexOf('/') + 1);

            string newAssetName = savePath + selected.name + ".mat";

            AssetDatabase.CreateAsset(material1, newAssetName);

            AssetDatabase.SaveAssets();

        }
        Debug.Log("Done!");
    }
    [SerializeField] static GameObject baseGameObjectToApplyTextureTo;
    [MenuItem("Project Tools/Create Card From Template")]
    private static void CreateCardFromTemplate()
    {
        foreach (Transform transformSelected in Selection.transforms)
        {
            baseGameObjectToApplyTextureTo = transformSelected.gameObject;
        }
        foreach (Object o in Selection.objects)
        {
            if (o.GetType() == typeof(CardSO))
            {
                CardSO cardToCreate = o as CardSO;



                GameObject InstantiatedCard = Instantiate(baseGameObjectToApplyTextureTo);
                InstantiatedCard.transform.parent = FindObjectOfType<Canvas>().transform;
                InstantiatedCard.name = cardToCreate.cardName;
                CardInHand cardTemplate = InstantiatedCard.GetComponentInChildren<CardInHand>();

                //todo make it do the anchor position is center!!!!
                cardTemplate.cardType = cardToCreate.cardType;
                cardTemplate.cardArt.sprite = cardToCreate.cardArt;
                cardTemplate.blackManaCost = cardToCreate.blackManaCost;
                cardTemplate.blueManaCost = cardToCreate.blueManaCost;
                cardTemplate.whiteManaCost = cardToCreate.whiteManaCost;
                cardTemplate.redManaCost = cardToCreate.redManaCost;
                cardTemplate.greenManaCost = cardToCreate.greenManaCost;
                cardTemplate.genericManaCost = cardToCreate.genericManaCost;
                cardTemplate.currentAttack = cardToCreate.attack;
                cardTemplate.currentHealth = cardToCreate.health;
                cardTemplate.cardTitle.text = cardToCreate.cardName;
                cardTemplate.cardAssignedToObject = cardToCreate.cardAssigned;

                cardTemplate.creatureType = cardToCreate.creatureType;
                cardTemplate.rarity = cardToCreate.rarity;
                if (cardToCreate.gameObjectToInstantiate != null)
                {
                    cardTemplate.GameObjectToInstantiate = cardToCreate.gameObjectToInstantiate.transform;
                }
                if (cardToCreate.gameObjectToInstantiate == null)
                {
                    cardTemplate.GameObjectToInstantiate = null;
                    Debug.Log("Gameobject on " + cardToCreate.cardName + " is null" );
                }
                cardTemplate.traversableType = cardToCreate.traversableType;
                cardTemplate.cardAbilityText.text = cardToCreate.cardText;
                cardTemplate.UpdateMana();
                cardTemplate.UpdateAttack();
                cardTemplate.UpdateRarity();

            }
        }
    }
}
#endif