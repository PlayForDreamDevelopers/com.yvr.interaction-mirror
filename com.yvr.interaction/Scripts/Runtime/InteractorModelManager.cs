using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractorModelManager : MonoBehaviour
{
    [SerializeField] Transform m_ModelPrefab;

    /// <summary>
    /// The prefab of a controller model to show for this controller that this behavior automatically instantiates.
    /// </summary>
    /// <remarks>
    /// This behavior automatically instantiates an instance of the prefab as a child
    /// of <see cref="modelParent"/> upon startup unless <see cref="model"/> is already set,
    /// in which case this value is ignored.
    /// </remarks>
    /// <seealso cref="model"/>
    public Transform modelPrefab
    {
        get => m_ModelPrefab;
        set => m_ModelPrefab = value;
    }

    [SerializeField]
    Transform m_ModelParent;

    /// <summary>
    /// The transform that this behavior uses as the parent for the model prefab when it is instantiated.
    /// </summary>
    /// <remarks>
    /// Automatically instantiated and set in <see cref="Awake"/> if not already set.
    /// Setting this will not automatically destroy the previous object.
    /// </remarks>
    public Transform modelParent
    {
        get => m_ModelParent;
        set
        {
            m_ModelParent = value;

            if (m_ModelPrefab != null)
                m_ModelPrefab.parent = m_ModelParent;
        }
    }

    [SerializeField] Transform m_AttachModel;

    /// <summary>
    /// The instance of the controller model in the scene. You can set this to an existing object instead of using <see cref="modelPrefab"/>.
    /// </summary>
    /// <remarks>
    /// If set, it should reference a child GameObject of this behavior so it will update with the controller pose.
    /// </remarks>
    public Transform attachModel
    {
        get => m_AttachModel;
        set => m_AttachModel = value;
    }

    protected void Awake()
    {
        if (m_ModelParent == null)
        {
            m_ModelParent = new GameObject($"[{gameObject.name}] Model Parent").transform;
            m_ModelParent.SetParent(transform, false);
            m_ModelParent.localPosition = Vector3.zero;
            m_ModelParent.localRotation = Quaternion.identity;
        }

        SetupModel();
    }

    void SetupModel()
    {
        if (m_AttachModel == null)
        {
            var prefab = GetModelPrefab();
            if (prefab != null)
                m_AttachModel = Instantiate(prefab, m_ModelParent).transform;
        }

        if (m_AttachModel != null)
            m_AttachModel.gameObject.SetActive(true);
    }

    protected virtual GameObject GetModelPrefab()
    {
        return m_ModelPrefab != null ? m_ModelPrefab.gameObject : null;
    }
}