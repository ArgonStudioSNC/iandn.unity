﻿/*===============================================================================
Copyright (c) 2018 PTC Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
===============================================================================*/

using UnityEngine;

/* <summary>
 * Force the orientation of the current scene.
 * </summary>
 * */
public class SceneOrientation : MonoBehaviour
{

    #region PUBLIC_MEMBERS

    // possible orientation for this project
    public enum Orientation
    {
        AUTOROTATION,
        PORTRAIT,
        LANDSCAPE
    }

    public Orientation sceneOrientation;

    #endregion // PUBLIC_MEMBERS


    #region MONOBEHAVIOUR_METHODS

    protected void Awake()
    {
        SetSceneOrientation();
    }

    #endregion // MONOBEHAVIOUR_METHODS


    #region PRIVATE_METHODS

    // set the scene orientation
    private void SetSceneOrientation()
    {
        switch (sceneOrientation)
        {
            case Orientation.AUTOROTATION:
                Screen.orientation = ScreenOrientation.AutoRotation;
                break;
            case Orientation.PORTRAIT:
                Screen.orientation = ScreenOrientation.Portrait;
                break;
            case Orientation.LANDSCAPE:
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                break;
        }
    }

    #endregion // PRIVATE_METHODS
}
