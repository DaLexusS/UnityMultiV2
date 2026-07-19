using Fusion;
using UnityEngine;

public class PlayerSkinChanger : NetworkBehaviour
{
    [SerializeField] private Transform modelContainer;
    
    [SerializeField] private GameObject[] skinModelPrefabs = new GameObject[ReadyManager.SkinCount];

    [Networked, OnChangedRender(nameof(OnSkinChanged))] public int SkinId { get; private set; }

    public GameObject CurrentModel { get; private set; }
    public Animator CurrentAnimator { get; private set; }

    private int appliedSkinId;

    public override void Spawned()
    {
        ApplySkin();
    }

    public void SetInitialSkin(int skinId)
    {
        if (!IsValidSkin(skinId))
        {
            Debug.LogWarning(
                $"Invalid skin ID: {skinId}. Skin 1 will be used.",
                this
            );

            skinId = 1;
        }

        SkinId = skinId;
    }

    private void OnSkinChanged()
    {
        ApplySkin();
    }

    private void ApplySkin()
    {
        if (!IsValidSkin(SkinId))
        {
            Debug.LogWarning(
                $"Cannot apply invalid skin ID: {SkinId}.",
                this
            );

            return;
        }
        if (CurrentModel != null &&
            appliedSkinId == SkinId)
        {
            return;
        }

        if (modelContainer == null)
        {
            Debug.LogError(
                "ModelContainer is not assigned.",
                this
            );

            return;
        }

        int skinIndex = SkinId - 1;

        if (skinModelPrefabs == null ||
            skinIndex >= skinModelPrefabs.Length)
        {
            Debug.LogError(
                $"No model prefab found for Skin {SkinId}.",
                this
            );

            return;
        }

        GameObject selectedPrefab =
            skinModelPrefabs[skinIndex];

        if (selectedPrefab == null)
        {
            Debug.LogError(
                $"Skin model prefab {SkinId} is null.",
                this
            );

            return;
        }

        RemoveCurrentModel();

        CurrentModel = Instantiate(
            selectedPrefab,
            modelContainer,
            false
        );

        ResetModelTransform();

        CurrentAnimator =
            CurrentModel.GetComponentInChildren<Animator>();

        appliedSkinId = SkinId;
    }

    private void RemoveCurrentModel()
    {
        CurrentAnimator = null;
        appliedSkinId = 0;

        if (CurrentModel == null)
            return;

        Destroy(CurrentModel);
        CurrentModel = null;
    }

    private void ResetModelTransform()
    {
        Transform modelTransform = CurrentModel.transform;

        modelTransform.localPosition = Vector3.zero;
        modelTransform.localRotation = Quaternion.identity;
        modelTransform.localScale = Vector3.one;
    }

    private static bool IsValidSkin(int skinId)
    {
        return skinId >= 1 &&
               skinId <= ReadyManager.SkinCount;
    }
}
