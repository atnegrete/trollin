using GameSparks.Core;
using System;
using UnityEngine;

public class GSPlayerDetailsContainer {
    public GSPlayerDetails[] AllPlayerDetails;
}

[Serializable]
public class GSPlayerDetails
{
    public int peerId;
    public string playerId;
    public RGBColor color;

    public GSPlayerDetails() {
        color = new RGBColor(255, 255, 255); // default
    }

    public GSPlayerDetails(GSData details) : this()
    {
        GSData gsColor = details.GetGSData("color");
        color = new RGBColor()
        {
            red = gsColor.GetFloat("red") ?? 255,
            green = gsColor.GetFloat("green") ?? 255,
            blue = gsColor.GetFloat("blue") ?? 255,
        };
    }

    public Color MaterialColor { get { return new Color(color.red/100, color.green/100, color.blue/100); } }

    public void SetRGBMaterialColor0to1(float r, float g, float b)
    {
        color = new RGBColor(r*100, g*100, b*100);
    }

    [Serializable]
    public class RGBColor
    {
        public RGBColor()
        {

        }

        public RGBColor(float r, float g, float b)
        {
            this.red = r;
            this.green = g;
            this.blue = b;
        }

        public float red;
        public float green;
        public float blue;
    }
}