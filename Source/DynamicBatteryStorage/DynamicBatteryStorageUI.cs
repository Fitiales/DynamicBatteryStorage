﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DynamicBatteryStorage
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class DynamicBatteryStorageUI : MonoBehaviour
    {
        Vessel activeVessel;
        int partCount = 0;
        ModuleDynamicBatteryStorage store;
        bool showWindow = false;

        public void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                //RenderingManager.AddToPostDrawQueue(0, DrawCapacitorGUI);
                FindController();

            }
        }

        public void ToggleWindow()
        {
         
            showWindow = !showWindow;
        }

        public void FindController()
        {
            activeVessel = FlightGlobals.ActiveVessel;
            partCount = activeVessel.parts.Count;

            //Debug.Log("NFE: Capacitor Manager: Finding Capcitors");
            store = activeVessel.GetComponent<ModuleDynamicBatteryStorage>();
        }

        // GUI VARS
        // ----------
        public Rect windowPos = new Rect(200f, 200f, 500f, 600f);
        public Vector2 scrollPosition = Vector2.zero;
        int windowID = new System.Random(3256231).Next();
        bool initStyles = false;

        GUIStyle gui_bg;
        GUIStyle gui_text;
        GUIStyle gui_header;
        GUIStyle gui_header2;
        GUIStyle gui_toggle;

        GUIStyle gui_window;

        // Set up the GUI styles
        private void InitStyles()
        {
            gui_window = new GUIStyle(HighLogic.Skin.window);
            gui_header = new GUIStyle(HighLogic.Skin.label);
            gui_header.fontStyle = FontStyle.Bold;
            gui_header.alignment = TextAnchor.UpperLeft;
            gui_header.fontSize = 12;
            gui_header.stretchWidth = true;

            gui_header2 = new GUIStyle(gui_header);
            gui_header2.alignment = TextAnchor.MiddleLeft;

            gui_text = new GUIStyle(HighLogic.Skin.label);
            gui_text.fontSize = 11;
            gui_text.alignment = TextAnchor.MiddleLeft;

            gui_bg = new GUIStyle(HighLogic.Skin.textArea);
            gui_bg.active = gui_bg.hover = gui_bg.normal;

            gui_toggle = new GUIStyle(HighLogic.Skin.toggle);
            gui_toggle.normal.textColor = gui_header.normal.textColor;

            windowPos = new Rect(200f, 200f, 610f, 315f);

            initStyles = true;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.Repaint || Event.current.isMouse)
            {
            }
            DrawGUI();
        }


        private void DrawGUI()
        {
            //Debug.Log("NFE: Start Capacitor UI Draw");
            Vessel activeVessel = FlightGlobals.ActiveVessel;

            if (activeVessel != null)
            {
                if (!initStyles)
                    InitStyles();

                if (showWindow)
                {
                    if (store == null)
                        FindController();
                    // Debug.Log(windowPos.ToString());
                    GUI.skin = HighLogic.Skin;
                    gui_window.padding.top = 5;

                    windowPos = GUI.Window(windowID, windowPos, PowerWindow, new GUIContent(), gui_window);
                }
            }
            //Debug.Log("NFE: Stop Capacitor UI Draw");
        }


        // GUI function for the window
        private void PowerWindow(int windowId)
        {
            GUI.skin = HighLogic.Skin;
            GUILayout.BeginHorizontal();
            GUILayout.Label("DBS Controller", gui_header, GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f), GUILayout.MinWidth(120f));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("X", GUILayout.MaxWidth(26f), GUILayout.MinWidth(26f), GUILayout.MaxHeight(26f), GUILayout.MinHeight(26f)))
            {
                ToggleWindow();
            }
            GUILayout.EndHorizontal();

            if (store != null && store.AnalyticMode)
            {
                
                double gain = store.DetermineShipPowerProduction();
                double draw = store.DetermineShipPowerConsumption();
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.MinWidth(600f), GUILayout.MinHeight(271f));
                GUILayout.BeginHorizontal();
                //windowPos.height = 175f + 70f;
            
                
                GUILayout.BeginVertical(gui_bg);
                for (int i = 0; i < store.powerProducers.Count; i++)
                {
                    DrawPowerProducer(store.powerProducers[i]);
                }
                GUILayout.Label(String.Format("Total Power Generation: {0:F2} Ec/s", gain), gui_header);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(gui_bg);
                for (int i = 0; i < store.powerConsumers.Count; i++)
                {
                    DrawPowerConsumer(store.powerConsumers[i]);
                }

                GUILayout.Label(String.Format("Total Power Consumption: {0:F2} Ec/s", draw), gui_header);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical(gui_bg);
                GUILayout.Label(String.Format("Part used for Buffer: {0} ", store.BufferPart.partInfo.name), gui_header);
                GUILayout.Label(String.Format("Requested Buffer Size: {0:F2} Ec ", store.BufferScale * draw * (double)TimeWarp.fixedDeltaTime), gui_header);

                GUILayout.Label(String.Format("Part base EC capacity: {0:F2} Ec", store.SavedMaxEC), gui_header);
                GUILayout.Label(String.Format("Vessel base EC capacity: {0:F2} Ec/s", store.SavedVesselMaxEC), gui_header);

                GUILayout.Label(String.Format("Net Power Deficit: {0:F2} Ec/s", (gain - draw) * (-1.0)), gui_header);
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("Below config threshold");
            }
            GUI.DragWindow();
        }

        private void DrawPowerProducer(PowerProducer prod)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(prod.ProducerType, gui_header);
            GUILayout.Label(String.Format("Producing: {0:F1} Ec/s", prod.GetPowerProduction()), gui_text);
            GUILayout.EndHorizontal();
        }
        private void DrawPowerConsumer(PowerConsumer cons)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(cons.ConsumerType, gui_header);
            GUILayout.Label(String.Format("Consuming: {0:F1} Ec/s", cons.GetPowerConsumption()), gui_text);
            GUILayout.EndHorizontal();
        }

        void Update()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                if (activeVessel != null)
                {
                    if (partCount != activeVessel.parts.Count || activeVessel != FlightGlobals.ActiveVessel)
                    {
                        FindController();
                    }
                }
                else
                {
                    FindController();
                }

            }
            if (activeVessel != null)
            {
                if (partCount != activeVessel.parts.Count || activeVessel != FlightGlobals.ActiveVessel)
                {
                    FindController();

                }
            }
            if ((Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftControl)) &&
              (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) &&
              Input.GetKeyDown(KeyCode.P))
            {
                ToggleWindow();
                // CTRL + SHIFT + P
            }
        }
    }
}