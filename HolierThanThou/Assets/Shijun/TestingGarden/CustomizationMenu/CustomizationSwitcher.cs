﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomizationSwitcher : MonoBehaviour, IInitializer<CustomizationSwitcher>
{
	[SerializeField] private ClothingOptions customizationType = default;
    private GameObject[] customizations;

	public void Start()
	{
		for (int i = 0; i < customizations.Length; i++)
		{
			GameObject option = Instantiate(customizations[i]);
			option.SetActive(false);
			option.transform.parent = transform;
			option.transform.localPosition += option.transform.parent.transform.position;
			//option.transform.localRotation = Quaternion.identity;
			//option.transform.localScale = Vector3.one;
		}
		if(transform.childCount > 0)
		{
			SwitchCustomization(0);
		}
	}

	public bool Equals(CustomizationSwitcher switcher) //this method is required for IGrabber.
	{
		return this.customizations == switcher.customizations &&
			this.customizationType == switcher.customizationType;
	}

	public void Initialize(GameObject[] obj) //this method is required for IGrabber.
	{
		customizations = obj;
	}

	public void SwitchCustomization(int index)
    {
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(false);
		}
		transform.GetChild(index).gameObject.SetActive(true);
    }
}
